using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using NGRID.Communication.Messages;
using NGRID.Exceptions;
using NGRID.Serialization;

namespace NGRID.Storage.MySqlStorage
{
    /// <summary>
    /// This class is used to perform database operations on MySQL database engine using ODBC.
    /// </summary>
    public class MySqlOdbcStorageManager : IStorageManager
    {
        #region Private fields

        /// <summary>
        /// Connection string to connect database.
        /// </summary>
        public string ConnectionString
        {
            get { return _connectionString; }
            set { _connectionString = value; }
        }
        private string _connectionString = "uid=root;server=localhost;driver={MySQL ODBC 3.51 Driver};database=ngrid;STMT=set names latin5";

        #endregion

        #region Public methods

        #region Unimplemented methods

        public void Start()
        {
            //No action
        }

        public void Stop(bool waitToStop)
        {
            //No action
        }

        public void WaitToStop()
        {
            //No action
        }

        #endregion

        /// <summary>
        /// Saves a NGRIDMessageRecord.
        /// </summary>
        /// <param name="messageRecord">NGRIDMessageRecord object to save</param>
        /// <returns>Auto Increment Id of saved message</returns>
        public int StoreMessage(NGRIDMessageRecord messageRecord)
        {
            var bytesOfMessage = NGRIDSerializationHelper.SerializeToByteArray(messageRecord.Message);
            var id = InsertAndGetLastId(
                "INSERT INTO messages(MessageId, DestServer, NextServer, DestApplication, MessageData, MessageDataLength, RecordDate) VALUES(?,?,?,?,?,?,now());",
                new OdbcParameter("?MessageId", messageRecord.MessageId),
                new OdbcParameter("?DestServer", messageRecord.DestServer),
                new OdbcParameter("?NextServer", messageRecord.NextServer),
                new OdbcParameter("?DestApplication", messageRecord.DestApplication),
                new OdbcParameter("?MessageData", bytesOfMessage),
                new OdbcParameter("?MessageDataLength", bytesOfMessage.Length)
                );
            messageRecord.Id = id;
            return id;
        }

        /// <summary>
        /// Gets waiting messages for an application.
        /// </summary>
        /// <param name="nextServer">Next server name</param>
        /// <param name="destApplication">Destination application name</param>
        /// <param name="minId">Minimum Id (as start Id)</param>
        /// <param name="maxCount">Max record count to get</param>
        /// <returns>Records gotten from database.</returns>
        public List<NGRIDMessageRecord> GetWaitingMessagesOfApplication(string nextServer, string destApplication, int minId, int maxCount)
        {
            var recordsTable = GetTable(
                "SELECT * FROM messages WHERE NextServer = ? AND DestApplication = ? AND Id >= ? ORDER BY Id ASC Limit ?;",
                new OdbcParameter("?NextServer", nextServer),
                new OdbcParameter("?DestApplication", destApplication),
                new OdbcParameter("?Id", minId),
                new OdbcParameter("?LimitValue", maxCount)
                );

            var recordsList = new List<NGRIDMessageRecord>(recordsTable.Rows.Count);
            foreach (DataRow recordRow in recordsTable.Rows)
            {
                recordsList.Add(
                    new NGRIDMessageRecord
                        {
                            Id = Convert.ToInt32(recordRow["Id"]),
                            DestApplication = recordRow["DestApplication"] as string,
                            DestServer = recordRow["DestServer"] as string,
                            NextServer = recordRow["NextServer"] as string,
                            Message = NGRIDSerializationHelper.DeserializeFromByteArray(() => new NGRIDDataTransferMessage(), (byte[])recordRow["MessageData"]),
                            MessageId = recordRow["MessageId"] as string,
                            RecordDate = (DateTime) recordRow["RecordDate"]
                        }
                    );
            }

            return recordsList;
        }

        /// <summary>
        /// Gets last (biggest) Id of waiting messages for an application.
        /// </summary>
        /// <param name="nextServer">Next server name</param>
        /// <param name="destApplication">Destination application name</param>
        /// <returns>last (biggest) Id of waiting messages</returns>
        public int GetMaxWaitingMessageIdOfApplication(string nextServer, string destApplication)
        {
            return GetScalarField(
                "SELECT Id FROM messages WHERE NextServer = ? AND DestApplication = ? ORDER BY Id DESC Limit 1;",
                new OdbcParameter("?NextServer", nextServer),
                new OdbcParameter("?DestApplication", destApplication)
                );
        }

        /// <summary>
        /// Gets waiting messages for an NGRID server.
        /// </summary>
        /// <param name="nextServer">Next server name</param>
        /// <param name="minId">Minimum Id (as start Id)</param>
        /// <param name="maxCount">Max record count to get</param>
        /// <returns>Records gotten from database.</returns>
        public List<NGRIDMessageRecord> GetWaitingMessagesOfServer(string nextServer, int minId, int maxCount)
        {
            var recordsTable = GetTable(
                "SELECT * FROM messages WHERE NextServer = ? AND Id >= ? ORDER BY Id ASC Limit ?;",
                new OdbcParameter("?NextServer", nextServer),
                new OdbcParameter("?Id", minId),
                new OdbcParameter("?LimitValue", maxCount)
                );

            var recordsList = new List<NGRIDMessageRecord>(recordsTable.Rows.Count);
            foreach (DataRow recordRow in recordsTable.Rows)
            {
                recordsList.Add(
                    new NGRIDMessageRecord
                        {
                            Id = Convert.ToInt32(recordRow["Id"]),
                            DestApplication = recordRow["DestApplication"] as string,
                            DestServer = recordRow["DestServer"] as string,
                            NextServer = recordRow["NextServer"] as string,
                            Message = NGRIDSerializationHelper.DeserializeFromByteArray(() => new NGRIDDataTransferMessage(), (byte[])recordRow["MessageData"]),
                            MessageId = recordRow["MessageId"] as string,
                            RecordDate = (DateTime) recordRow["RecordDate"]
                        }
                    );
            }

            return recordsList;
        }

        /// <summary>
        /// Gets last (biggest) Id of waiting messages for an NGRID server.
        /// </summary>
        /// <param name="nextServer">Next server name</param>
        /// <returns>last (biggest) Id of waiting messages</returns>
        public int GetMaxWaitingMessageIdOfServer(string nextServer)
        {
            return GetScalarField(
                "SELECT Id FROM messages WHERE NextServer = ? ORDER BY Id DESC Limit 1;",
                new OdbcParameter("?NextServer", nextServer)
                );
        }

        /// <summary>
        /// Removes a message.
        /// </summary>
        /// <param name="id">id of message to remove</param>
        /// <returns>Effected rows count</returns>
        public int RemoveMessage(int id)
        {
            return ExecuteNonQuery(
                "DELETE FROM messages WHERE Id = ?;",
                new OdbcParameter("?Id", id)
                );
        }

        /// <summary>
        /// This method is used to set Next Server for a Destination Server. 
        /// It is used to update database records when Server Graph changed.
        /// </summary>
        /// <param name="destServer">Destination server of messages</param>
        /// <param name="nextServer">Next server of messages for destServer</param>
        public void UpdateNextServer(string destServer, string nextServer)
        {
            ExecuteNonQuery(
                "UPDATE messages SET NextServer = ? WHERE DestServer = ?;",
                new OdbcParameter("?NextServer", nextServer),
                new OdbcParameter("?DestServer", destServer)
                );
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Executes a query and returns effected rows count.
        /// </summary>
        /// <param name="query">Query to execute</param>
        /// <param name="parameters">Parameters</param>
        /// <returns>Effected rows count</returns>
        private int ExecuteNonQuery(string query, params OdbcParameter[] parameters)
        {
            using (var connection = new OdbcConnection(ConnectionString))
            {
                connection.Open();
                using (var command = new OdbcCommand(query, connection))
                {
                    foreach (var parameter in parameters)
                    {
                        command.Parameters.Add(parameter);
                    }

                    return command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// This method is used to run an insert query and get inserted row's auto increment column's value.
        /// </summary>
        /// <param name="query">Insert query to be executed</param>
        /// <param name="parameters">Parameters</param>
        /// <returns>Auto increment column's value of inserted row</returns>
        private int InsertAndGetLastId(string query, params OdbcParameter[] parameters)
        {
            using (var connection = new OdbcConnection(ConnectionString))
            {
                connection.Open();
                using (var command = new OdbcCommand(query, connection))
                {
                    foreach (var parameter in parameters)
                    {
                        command.Parameters.Add(parameter);
                    }

                    command.ExecuteNonQuery();
                }

                const string queryForLastId = "SELECT LAST_INSERT_ID() AS LastId;";
                using (var command = new OdbcCommand(queryForLastId, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return Convert.ToInt32(reader["LastId"]);
                        }

                        throw new NGRIDException("Can not be obtained last inserted id for query: " + query);
                    }
                }
            }
        }

        /// <summary>
        /// Runs a query and returns a DataTable.
        /// </summary>
        /// <param name="query">Query to execute</param>
        /// <param name="parameters">Parameters</param>
        /// <returns>Selected table</returns>
        private DataTable GetTable(string query, params OdbcParameter[] parameters)
        {
            var table = new DataTable();
            using (var connection = new OdbcConnection(ConnectionString))
            {
                using (var command = new OdbcCommand(query, connection))
                {
                    foreach (var parameter in parameters)
                    {
                        command.Parameters.Add(parameter);
                    }

                    using (var adapter = new OdbcDataAdapter(command))
                    {
                        adapter.Fill(table);
                        return table;
                    }
                }
            }
        }

        /// <summary>
        /// Gets a record from a table.
        /// </summary>
        /// <param name="query">Select query</param>
        /// <param name="parameters">Select parameters</param>
        /// <returns>Returns found row as TableRecord object. If there is no row returns null</returns>
        public TableRecord GetTableRecord(string query, params OdbcParameter[] parameters)
        {
            using (var connection = new OdbcConnection(ConnectionString))
            {
                connection.Open();
                using (var command = new OdbcCommand(query, connection))
                {
                    foreach (var parameter in parameters)
                    {
                        command.Parameters.Add(parameter);
                    }

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var record = new TableRecord();
                            for (var i = 0; i < reader.FieldCount; i++)
                            {
                                record[reader.GetName(i)] = reader[i];
                            }

                            return record;
                        }

                        return null;
                    }
                }
            }
        }

        /// <summary>
        /// Executes a query and gets a Integer result.
        /// If query returns no data, method returns 0.
        /// </summary>
        /// <param name="query">Query to execute</param>
        /// <param name="parameters">Parameters</param>
        /// <returns>Query result or 0</returns>
        public int GetScalarField(string query, params OdbcParameter[] parameters)
        {
            using (var connection = new OdbcConnection(ConnectionString))
            {
                connection.Open();
                using (var command = new OdbcCommand(query, connection))
                {
                    foreach (var parameter in parameters)
                    {
                        command.Parameters.Add(parameter);
                    }

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return Convert.ToInt32(reader[0]);
                        }

                        return 0;
                    }
                }
            }
        }

        #endregion
    }
}
