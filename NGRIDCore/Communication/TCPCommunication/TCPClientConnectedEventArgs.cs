using System;
using System.Net.Sockets;

namespace NGRID.Communication.TCPCommunication
{
    /// <summary>
    /// A delegate to create events for connected TCP clients to this server.
    /// </summary>
    /// <param name="sender">The object which raises event</param>
    /// <param name="e">Event arguments</param>
    public delegate void TCPClientConnectedHandler(object sender, TCPClientConnectedEventArgs e);

    /// <summary>
    /// Stores informations about connected client.
    /// </summary>
    public class TCPClientConnectedEventArgs : EventArgs
    {
        /// <summary>
        /// Client Socket Connection
        /// </summary>
        public Socket ClientSocket { get; set; }
    }
}
