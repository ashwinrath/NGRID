using System;
using NGRID.Communication.Messages;

namespace NGRID.Communication.Events
{
    /// <summary>
    /// A delegate to create events when a message received from a communicator.
    /// </summary>
    /// <param name="sender">The object which raises event</param>
    /// <param name="e">Event arguments</param>
    public delegate void MessageReceivedFromCommunicatorHandler(object sender, MessageReceivedFromCommunicatorEventArgs e);

    /// <summary>
    /// Stores communicator and message informations.
    /// </summary>
    public class MessageReceivedFromCommunicatorEventArgs : EventArgs
    {
        /// <summary>
        /// Communicator.
        /// </summary>
        public ICommunicator Communicator { get; set; }

        /// <summary>
        /// Received message from communicator.
        /// </summary>
        public NGRIDMessage Message { get; set; }
    }
}
