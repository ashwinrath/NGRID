using System.Collections.Generic;

namespace NGRID.Storage.MemoryStorage
{
    /// <summary>
    /// Performs all database/storing operations on memory. So, all stored informations are lost
    /// on server shutdown.
    /// </summary>
    public class MemoryStorageManager : IStorageManager
    {
        /// <summary>
        /// A list to store message records (Messages table).
        /// </summary>
        private readonly SortedList<int, NGRIDMessageRecord> _messages;

        /// <summary>
        /// Stores ID field of last inserted item to the _messages list.
        /// </summary>
        private volatile int _lastRecordId;

        /// <summary>
        /// Creates a new MemoryStorageManager.
        /// </summary>
        public MemoryStorageManager()
        {
            _messages = new SortedList<int, NGRIDMessageRecord>();            
        }

        /// <summary>
        /// Starts the storage manager.
        /// </summary>
        public void Start()
        {
            //No action
        }

        /// <summary>
        /// Stops the storage manager.
        /// </summary>
        public void Stop(bool waitToStop)
        {
            _messages.Clear();
        }

        public void WaitToStop()
        {
            //No action
        }

        /// <summary>
        /// Saves a NGRIDMessageRecord object.
        /// </summary>
        /// <param name="messageRecord"></param>
        public int StoreMessage(NGRIDMessageRecord messageRecord)
        {
            lock (_messages)
            {
                messageRecord.Id = (++_lastRecordId);
                _messages.Add(messageRecord.Id, messageRecord);
                return messageRecord.Id;
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
            var results = new List<NGRIDMessageRecord>();
            lock (_messages)
            {
                foreach (var record in _messages.Values)
                {
                    if (record.NextServer.Equals(nextServer) && record.DestApplication.Equals(destApplication)
                        && record.Id >= minId)
                    {
                        results.Add(record);
                        if (results.Count >= maxCount)
                        {
                            break;
                        }
                    }
                }
            }

            return results; 
        }

        /// <summary>
        /// Gets last (biggest) Id of waiting messages for an application.
        /// </summary>
        /// <param name="nextServer">Next server name</param>
        /// <param name="destApplication">Destination application name</param>
        /// <returns>last (biggest) Id of waiting messages</returns>
        public int GetMaxWaitingMessageIdOfApplication(string nextServer, string destApplication)
        {
            lock (_messages)
            {
                var messageRecords = _messages.Values;
                for (var i = messageRecords.Count - 1; i >= 0; i--)
                {
                    var record = messageRecords[i];
                    if (record.NextServer.Equals(nextServer) && record.DestApplication.Equals(destApplication))
                    {
                        return record.Id;
                    }
                }
            }

            return 0;
        }

        /// <summary>
        /// Gets waiting messages for an application.
        /// </summary>
        /// <param name="nextServer">Next server name</param>
        /// <param name="minId">Minimum Id (as start Id)</param>
        /// <param name="maxCount">Max record count to get</param>
        /// <returns>Records gotten from database.</returns>
        public List<NGRIDMessageRecord> GetWaitingMessagesOfServer(string nextServer, int minId, int maxCount)
        {
            var results = new List<NGRIDMessageRecord>();
            lock (_messages)
            {
                foreach (var record in _messages.Values)
                {
                    if (record.NextServer.Equals(nextServer) && record.Id >= minId)
                    {
                        results.Add(record);
                        if (results.Count >= maxCount)
                        {
                            break;
                        }
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Gets last (biggest) Id of waiting messages for an NGRID server.
        /// </summary>
        /// <param name="nextServer">Next server name</param>
        /// <returns>last (biggest) Id of waiting messages</returns>
        public int GetMaxWaitingMessageIdOfServer(string nextServer)
        {
            lock (_messages)
            {
                var messageRecords = _messages.Values;
                for (var i = messageRecords.Count - 1; i >= 0; i--)
                {
                    var record = messageRecords[i];
                    if (record.NextServer.Equals(nextServer))
                    {
                        return record.Id;
                    }
                }
            }

            return 0;
        }

        /// <summary>
        /// Removes a message record.
        /// </summary>
        /// <param name="recordId">recordId to delete</param>
        /// <returns>Effected rows count</returns>
        public int RemoveMessage(int recordId)
        {
            lock (_messages)
            {
                if(_messages.ContainsKey(recordId))
                {
                    _messages.Remove(recordId);
                    return 1;
                }
            }

            return 0;
        }

        /// <summary>
        /// This method is used to set Next Server for a Destination Server. 
        /// It is used to update database records when Server Graph changed.
        /// </summary>
        /// <param name="destServer">Destination server of messages</param>
        /// <param name="nextServer">Next server of messages for destServer</param>
        public void UpdateNextServer(string destServer, string nextServer)
        {
            lock (_messages)
            {
                foreach (var record in _messages.Values)
                {
                    if (record.DestServer.Equals(nextServer))
                    {
                        record.NextServer = nextServer;
                    }
                }
            }
        }
    }
}
