using System;

namespace NGRID.Client
{
    /// <summary>
    /// A delegate to create events when a data transfer message received from NGRID server.
    /// </summary>
    /// <param name="sender">The object which raises event</param>
    /// <param name="e">Event arguments</param>
    public delegate void MessageReceivedHandler(object sender, MessageReceivedEventArgs e);

    /// <summary>
    /// Stores message informations.
    /// </summary>
    public class MessageReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Received message from NGRID server.
        /// </summary>
        public IIncomingMessage Message { get; set; }
    }
}
