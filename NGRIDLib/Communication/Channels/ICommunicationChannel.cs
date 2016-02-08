using NGRID.Communication.Messages;

namespace NGRID.Communication.Channels
{
    /// <summary>
    /// All Communication channels implements this interface.
    /// It is used by NGRIDClient and NGRIDController classes to communicate with NGRID server.
    /// </summary>
    public interface ICommunicationChannel
    {
        /// <summary>
        /// This event is raised when the state of the communication channel changes.
        /// </summary>
        event CommunicationStateChangedHandler StateChanged;

        /// <summary>
        /// This event is raised when a NGRIDMessage object is received from NGRID server.
        /// </summary>
        event MessageReceivedHandler MessageReceived;
        
        /// <summary>
        /// Unique identifier for this communicator in connected NGRID server.
        /// This field is not set by communication channel,
        /// it is set by another classes (NGRIDClient) that are using
        /// communication channel. 
        /// </summary>
        long ComminicatorId { get; set; }

        /// <summary>
        /// Gets the state of communication channel.
        /// </summary>
        CommunicationStates State { get; }

        /// <summary>
        /// Communication way for this channel.
        /// This field is not set by communication channel,
        /// it is set by another classes (NGRIDClient) that are using
        /// communication channel. 
        /// </summary>
        CommunicationWays CommunicationWay { get; set; }
        
        /// <summary>
        /// Connects to NGRID server.
        /// </summary>
        void Connect();

        /// <summary>
        /// Disconnects from NGRID server.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Sends a NGRIDMessage to the NGRID server
        /// </summary>
        /// <param name="message">Message to be sent</param>
        void SendMessage(NGRIDMessage message);
    }
}
