using System;

namespace NGRID.Communication.Events
{
    /// <summary>
    /// A delegate to create events when a communicator connection established.
    /// </summary>
    /// <param name="sender">The object which raises event</param>
    /// <param name="e">Event arguments</param>
    public delegate void CommunicatorConnectedHandler(object sender, CommunicatorConnectedEventArgs e);

    /// <summary>
    /// Stores communicator reference.
    /// </summary>
    public class CommunicatorConnectedEventArgs : EventArgs
    {
        /// <summary>
        /// Communicator.
        /// </summary>
        public ICommunicator Communicator { get; set; }
    }
}
