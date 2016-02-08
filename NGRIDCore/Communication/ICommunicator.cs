using NGRID.Communication.Events;
using NGRID.Communication.Messages;
using NGRID.Threading;

namespace NGRID.Communication
{
    /// <summary>
    /// Communicators is used by upper layers by this interface,
    /// all communicator classes must implement it.
    /// </summary>
    public interface ICommunicator : IRunnable
    {
        /// <summary>
        /// This event is raised when the state of the communicator changes.
        /// </summary>
        event CommunicatorStateChangedHandler StateChanged;

        /// <summary>
        /// This event is raised when a NgridMessage received.
        /// </summary>
        event MessageReceivedFromCommunicatorHandler MessageReceived;

        /// <summary>
        ///  Unique identifier for this communicator in this server.
        /// </summary>
        long ComminicatorId { get; }

        /// <summary>
        /// Connection state of communicator.
        /// </summary>
        CommunicationStates State { get; }

        /// <summary>
        /// Communication way for this communicator.
        /// </summary>
        CommunicationWays CommunicationWay { get; set; }

        /// <summary>
        /// Sends a message to the communicator.
        /// </summary>
        /// <param name="message"></param>
        void SendMessage(NGRIDMessage message);
    }
}
