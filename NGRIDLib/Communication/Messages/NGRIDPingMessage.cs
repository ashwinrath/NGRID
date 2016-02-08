namespace NGRID.Communication.Messages
{
    /// <summary>
    /// This class is used to send Ping messages to check if remote application is connected and working.
    /// NGRID Servers send Ping messages to other NGRID servers and gets response.
    /// Client applications send Ping messages to NGRID servers and gets response.
    /// If there is no Ping message from a remote application for a while, connection is closed and
    /// reconnected if needed.
    /// </summary>
    public class NGRIDPingMessage : NGRIDMessage
    {
        /// <summary>
        /// MessageTypeId of message.
        /// It is used to serialize/deserialize message.
        /// </summary>
        public override int MessageTypeId
        {
            get { return NGRIDMessageFactory.MessageTypeIdNGRIDPingMessage; }
        }
    }
}
