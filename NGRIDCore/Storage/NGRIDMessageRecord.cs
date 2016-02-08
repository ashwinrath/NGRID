using System;
using NGRID.Communication.Messages;

namespace NGRID.Storage
{
    /// <summary>
    /// Represents a message record in database/storage manager.
    /// </summary>
    public class NGRIDMessageRecord
    {
        /// <summary>
        /// Auto Increment ID in database.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// MessageId of message.
        /// </summary>
        public string MessageId { get; set; }

        /// <summary>
        /// Message object.
        /// </summary>
        public NGRIDDataTransferMessage Message { get; set; }

        /// <summary>
        /// Destination server.
        /// </summary>
        public string DestServer { get; set; }

        /// <summary>
        /// Next server.
        /// </summary>
        public string NextServer { get; set; }

        /// <summary>
        /// Destination application in destination server
        /// </summary>
        public string DestApplication { get; set; }

        /// <summary>
        /// Storing time of message on this server.
        /// </summary>
        public DateTime RecordDate { get; set; }

        /// <summary>
        /// Empty contructor.
        /// </summary>
        public NGRIDMessageRecord()
        {
            
        }

        /// <summary>
        /// Creates a NGRIDMessageRecord object using a NGRIDDataTransferMessage.
        /// </summary>
        /// <param name="message">Message object</param>
        public NGRIDMessageRecord(NGRIDDataTransferMessage message)
        {
            Message = message;
            MessageId = message.MessageId;
            DestServer = message.DestinationServerName;
            DestApplication = message.DestinationApplicationName;
            RecordDate = DateTime.Now;
        }
    }
}
