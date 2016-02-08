using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Data.SQLite;
using NGRID.Communication.Messages;
using NGRID.Exceptions;
using NGRID.Serialization;

namespace NGRID.Storage.SQLiteStorage
{
    /// <summary>
    /// This class is used to perform database operations on SQLite database engine.
    /// To use this storage manager, SqliteDB\NGRID.s3db file must be in root folder of NGRID server.
    /// </summary>
    public class SqliteStorageManager : IStorageManager
    {
        #region Private fields

        /// <summary>
        /// Connection string to connect database.
        /// </summary>
        private string ConnectionString { get; set; }

        /// <summary>
        /// Open database connection.
        /// </summary>
        private SQLiteConnection _connection;

        /// <summary>
        /// This object is used to synchronized access to database. One thread in a time.
        /// </summary>
        private readonly object _syncObj = new object();

        #endregion

        #region Public methods

        /// <summary>
        /// Creates a new SqliteStorageManager object.
        /// </summary>
        public SqliteStorageManager()
        {
            var csb = new SQLiteConnectionStringBuilder
            {
                DataSource = Path.Combine(GeneralHelper.GetCurrentDirectory(), @"SqliteDB\NGRID.s3db"),
                Version = 3,
                Pooling = true,
                ReadOnly = false,
                SyncMode = SynchronizationModes.Off
            };
            ConnectionString = csb.ConnectionString;
        }

        /// <summary>
        /// Initializes SQLite connection.
        /// </summary>
        public void Start()
        {
            _connection = new SQLiteConnection(ConnectionString);
            _connection.Open();
        }

        public void Stop(bool waitToStop)
        {
            _connection.Dispose();
        }

        public void WaitToStop()
        {
            //No action
        }

        /// <summary>
        /// Saves a NGRIDMessageRecord.
        /// </summary>
        /// <param name="messageRecord">NGRIDMessageRecord object to save</param>
        /// <returns>Auto Increment Id of saved message</returns>
        public int StoreMessage(NGRIDMessageRecord messageRecord)
        {
            lock (_syncObj)
            {
                var bytesOfMessage = NGRIDSerializationHelper.SerializeToByteArray(messageRecord.Message);
                var id = InsertAndGetLastId(
                    "INSERT INTO messages(MessageId, DestServer, NextServer, DestApplication, MessageData, MessageDataLength, RecordDate) VALUES(@MessageId,@DestServer,@NextServer,@DestApplication,@MessageData,@MessageDataLength,@RecordDate);",
                    new SQLiteParameter("@MessageId", messageRecord.MessageId),
                    new SQLiteParameter("@DestServer", messageRecord.DestServer),
                    new SQLiteParameter("@NextServer", messageRecord.NextServer),
                    new SQLiteParameter("@DestApplication", messageRecord.DestApplication),
                    new SQLiteParameter("@MessageData", bytesOfMessage),
                    new SQLiteParameter("@MessageDataLength", bytesOfMessage.Length),
                    new SQLiteParameter("@RecordDate", DateTime.Now)
                    );
                messageRecord.Id = id;
                return id;
            }
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
            DataTable recordsTable;
            lock (_syncObj)
            {
                recordsTable = GetTable(
                    "SELECT * FROM messages WHERE NextServer = @NextServer AND DestApplication = @DestApplication AND Id >= @Id ORDER BY Id ASC Limit @LimitValue",
                    new SQLiteParameter("@NextServer", nextServer),
                    new SQLiteParameter("@DestApplication", destApplication),
                    new SQLiteParameter("@Id", minId),
                    new SQLiteParameter("@LimitValue", maxCount)
                    );
            }

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
                            RecordDate = (DateTime)recordRow["RecordDate"]
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
            lock (_syncObj)
            {
                return GetScalarField(
                    "SELECT Id FROM messages WHERE NextServer = @NextServer AND DestApplication = @DestApplication ORDER BY Id DESC Limit 1",
                    new SQLiteParameter("@NextServer", nextServer),
                    new SQLiteParameter("@DestApplication", destApplication)
                    );
            }
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
            DataTable recordsTable;
            lock (_syncObj)
            {
                recordsTable = GetTable(
                    "SELECT * FROM messages WHERE NextServer = @NextServer AND Id >= @Id ORDER BY Id ASC Limit @LimitValue",
                    new SQLiteParameter("@NextServer", nextServer),
                    new SQLiteParameter("@Id", minId),
                    new SQLiteParameter("@LimitValue", maxCount)
                    );
            }

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
                        });
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
            lock (_syncObj)
            {
                return GetScalarField(
                    "SELECT Id FROM messages WHERE NextServer = @NextServer ORDER BY Id DESC Limit 1",
                    new SQLiteParameter("@NextServer", nextServer)
                    );
            }
        }

        /// <summary>
        /// Removes a message.
        /// </summary>
        /// <param name="id">id of message to remove</param>
        /// <returns>Effected rows count</returns>
        public int RemoveMessage(int id)
        {
            lock (_syncObj)
            {
                return ExecuteNonQuery(
                    "DELETE FROM messages WHERE Id = @Id",
                    new SQLiteParameter("@Id", id)
                    );
            }
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
                "UPDATE messages SET NextServer = @NextServer WHERE DestServer = @DestServer",
                new SQLiteParameter("@NextServer", nextServer),
                new SQLiteParameter("@DestServer", destServer)
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
        private int ExecuteNonQuery(string query, params SQLiteParameter[] parameters)
        {
            int result;
            using (var transaction = _connection.BeginTransaction())
            {
                using (var command = new SQLiteCommand(query, _connection))
                {
                    foreach (var parameter in parameters)
                    {
                        command.Parameters.Add(parameter);
                    }

                    result = command.ExecuteNonQuery();
                }

                transaction.Commit();
            }

            return result;
        }

        /// <summary>
        /// This method is used to run an insert query and get inserted row's auto increment column's value.
        /// </summary>
        /// <param name="query">Insert query to be executed</param>
        /// <param name="parameters">Parameters</param>
        /// <returns>Auto increment column's value of inserted row</returns>
        private int InsertAndGetLastId(string query, params SQLiteParameter[] parameters)
        {
            var insertedId = 0;
            using (var transaction = _connection.BeginTransaction())
            {
                using (var command = new SQLiteCommand(query, _connection, transaction))
                {
                    foreach (var parameter in parameters)
                    {
                        command.Parameters.Add(parameter);
                    }

                    command.ExecuteNonQuery();
                }

                const string queryForLastId = "SELECT last_insert_rowid() AS LastId;";
                using (var command = new SQLiteCommand(queryForLastId, _connection, transaction))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            insertedId = Convert.ToInt32(reader["LastId"]);
                        }
                    }
                }

                transaction.Commit();
            }

            if(insertedId <= 0)
            {
                throw new NGRIDException("Can not be obtained last inserted id for query: " + query);
            }

            return insertedId;
        }

        /// <summary>
        /// Runs a query and returns a DataTable.
        /// </summary>
        /// <param name="query">Query to execute</param>
        /// <param name="parameters">Parameters</param>
        /// <returns>Selected table</returns>
        private DataTable GetTable(string query, params SQLiteParameter[] parameters)
        {
            var table = new DataTable();
            using (var command = new SQLiteCommand(query, _connection))
            {
                foreach (var parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }

                using (var adapter = new SQLiteDataAdapter(command))
                {
                    adapter.Fill(table);
                    return table;
                }
            }
        }

        /// <summary>
        /// Gets a record from a table.
        /// </summary>
        /// <param name="query">Select query</param>
        /// <param name="parameters">Select parameters</param>
        /// <returns>Returns found row as TableRecord object. If there is no row returns null</returns>
        public TableRecord GetTableRecord(string query, params SQLiteParameter[] parameters)
        {
            using (var command = new SQLiteCommand(query, _connection))
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

        /// <summary>
        /// Executes a query and gets a Integer result.
        /// If query returns no data, method returns 0.
        /// </summary>
        /// <param name="query">Query to execute</param>
        /// <param name="parameters">Parameters</param>
        /// <returns>Query result or 0</returns>
        public int GetScalarField(string query, params SQLiteParameter[] parameters)
        {
            using (var command = new SQLiteCommand(query, _connection))
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

        #endregion
    }
}
