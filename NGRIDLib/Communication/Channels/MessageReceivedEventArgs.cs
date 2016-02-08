using System;
using NGRID.Communication.Messages;

namespace NGRID.Communication.Channels
{
    /// <summary>
    /// A delegate to create events by Communication Channels, when a NGRIDMessage received from NGRID server.
    /// </summary>
    /// <param name="sender">The object which raises event</param>
    /// <param name="e">Event arguments</param>
    public delegate void MessageReceivedHandler(ICommunicationChannel sender, MessageReceivedEventArgs e);

    /// <summary>
    /// Stores message informations.
    /// </summary>
    public class MessageReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Received message from NGRID server.
        /// </summary>
        public NGRIDMessage Message { get; set; }
    }
}
