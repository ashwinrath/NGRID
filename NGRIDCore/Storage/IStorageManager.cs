using System.Collections.Generic;
using NGRID.Threading;

namespace NGRID.Storage
{
    /// <summary>
    /// Defines an interface for (database) storing operations. Thus, NGRID Server can use more than one
    /// storage engine for messages and other database operations.
    /// </summary>
    public interface IStorageManager : IRunnable
    {
        /// <summary>
        /// Saves a NGRIDMessageRecord.
        /// </summary>
        /// <param name="messageRecord">NGRIDMessageRecord object to save</param>
        /// <returns>Auto Increment Id of saved message</returns>
        int StoreMessage(NGRIDMessageRecord messageRecord);

        /// <summary>
        /// Gets waiting messages for an application.
        /// </summary>
        /// <param name="nextServer">Next server name</param>
        /// <param name="destApplication">Destination application name</param>
        /// <param name="minId">Minimum Id (as start Id)</param>
        /// <param name="maxCount">Max record count to get</param>
        /// <returns>Records gotten from database.</returns>
        List<NGRIDMessageRecord> GetWaitingMessagesOfApplication(string nextServer, string destApplication, int minId, int maxCount);

        /// <summary>
        /// Gets last (biggest) Id of waiting messages for an application.
        /// </summary>
        /// <param name="nextServer">Next server name</param>
        /// <param name="destApplication">Destination application name</param>
        /// <returns>last (biggest) Id of waiting messages</returns>
        int GetMaxWaitingMessageIdOfApplication(string nextServer, string destApplication);

        /// <summary>
        /// Gets waiting messages for an application.
        /// </summary>
        /// <param name="nextServer">Next server name</param>
        /// <param name="minId">Minimum Id (as start Id)</param>
        /// <param name="maxCount">Max record count to get</param>
        /// <returns>Records gotten from database.</returns>
        List<NGRIDMessageRecord> GetWaitingMessagesOfServer(string nextServer, int minId, int maxCount);

        /// <summary>
        /// Gets last (biggest) Id of waiting messages for an NGRID server.
        /// </summary>
        /// <param name="nextServer">Next server name</param>
        /// <returns>last (biggest) Id of waiting messages</returns>
        int GetMaxWaitingMessageIdOfServer(string nextServer);

        /// <summary>
        /// Removes a message.
        /// </summary>
        /// <param name="id">id of message to remove</param>
        /// <returns>Effected rows count</returns>
        int RemoveMessage(int id);

        /// <summary>
        /// This method is used to set Next Server for a Destination Server. 
        /// It is used to update database records when Server Graph changed.
        /// </summary>
        /// <param name="destServer">Destination server of messages</param>
        /// <param name="nextServer">Next server of messages for destServer</param>
        void UpdateNextServer(string destServer, string nextServer);
    }
}
