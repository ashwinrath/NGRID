using System;

namespace NGRID.Communication.Events
{
    /// <summary>
    /// A delegate to create events when a communicator connection closed.
    /// </summary>
    /// <param name="sender">The object which raises event</param>
    /// <param name="e">Event arguments</param>
    public delegate void CommunicatorDisconnectedHandler(object sender, CommunicatorDisconnectedEventArgs e);

    /// <summary>
    /// Stores communicator reference.
    /// </summary>
    public class CommunicatorDisconnectedEventArgs : EventArgs
    {
        /// <summary>
        /// Communicator.
        /// </summary>
        public ICommunicator Communicator { get; set; }
    }
}
