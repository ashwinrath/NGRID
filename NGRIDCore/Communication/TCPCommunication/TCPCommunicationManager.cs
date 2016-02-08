using System;
using System.Reflection;
using log4net;

namespace NGRID.Communication.TCPCommunication
{
    /// <summary>
    /// A communication manager that listens and handles incoming connections and messages using TCP.
    /// </summary>
    public class TCPCommunicationManager : CommunicationManagerBase
    {
        #region Private fields

        /// <summary>
        /// Reference to logger
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The object to listen and handle incoming connection requests.
        /// </summary>
        private readonly TCPConnectionListener _connectionListener;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="port">Server's listening TCP port number</param>
        public TCPCommunicationManager(int port)
        {
            _connectionListener = new TCPConnectionListener(port);
            _connectionListener.TCPClientConnected += ConnectionListener_ClientConnected;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Starts listening of port and handling connections.
        /// </summary>
        public override void Start()
        {
            _connectionListener.Start();
        }

        /// <summary>
        /// Stops listening of port and handling connections. 
        /// Closes all open communicator connection.
        /// </summary>
        /// <param name="waitToStop">Indicates that caller thread waits stopping of manager</param>
        public override void Stop(bool waitToStop)
        {
            _connectionListener.Stop(waitToStop);
        }

        /// <summary>
        /// Waits stopping of communication manager. 
        /// </summary>
        public override void WaitToStop()
        {
            _connectionListener.WaitToStop();
        }

        #endregion

        #region Private methods

        /// <summary>
        /// When TCPConnectionListener handles a connection, it is taken by this method to create and 
        /// register it's neccessary events and to add it to _communicators collection. 
        /// This method is also starts the communicator.
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Event informations</param>
        private void ConnectionListener_ClientConnected(object sender, TCPClientConnectedEventArgs e)
        {
            try
            {
                OnCommunicatorConnected(new TCPCommunicator(e.ClientSocket, CommunicationLayer.CreateCommunicatorId()));
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }
        }

        #endregion
    }
}
