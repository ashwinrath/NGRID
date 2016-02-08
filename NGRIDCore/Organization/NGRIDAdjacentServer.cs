using System;
using System.Collections.Generic;
using System.Net;
using NGRID.Communication;
using NGRID.Communication.Events;
using NGRID.Communication.Messages;
using NGRID.Communication.TCPCommunication;
using NGRID.Exceptions;
using NGRID.Storage;

namespace NGRID.Organization
{
    /// <summary>
    /// An NGRIDAsjacentServer is a server in NGRIDServerGraph that is an adjacent of this server
    /// and directly communicates with this server. 
    /// </summary>
    public class NGRIDAdjacentServer : NGRIDPersistentRemoteApplicationBase
    {
        #region Public properties

        /// <summary>
        /// IP Address of Server on network
        /// </summary>
        public string IpAddress
        {
            get { return _ipAddress; }
        }
        private readonly string _ipAddress;

        /// <summary>
        /// Listening port number of NGRID on Server.
        /// </summary>
        public int Port
        {
            get { return _port; }
        }
        private readonly int _port;
        
        /// <summary>
        /// Communicator Type for NGRID server
        /// </summary>
        public override CommunicatorTypes CommunicatorType
        {
            get { return CommunicatorTypes.NgridServer; }
        }

        /// <summary>
        /// This field is used to determine maximum allowed communicator count.
        /// No more communicator added if communicator count is equal to this number.
        /// For infinit communicator, returns -1;
        /// 
        /// Only 1 communicator is allowed in any time for a NGRIDAdjacentServer.
        /// </summary>
        protected override int MaxAllowedCommunicatorCount
        {
            get { return 1; }
        }

        #endregion

        #region Private fields

        /// <summary>
        /// The time last connection attempt to remote NGRID server.
        /// </summary>
        private DateTime _lastConnectionAttemptTime;

        /// <summary>
        /// Consecutive error count on trying to connect to remote NGRID server.
        /// </summary>
        private int _connectionErrorCount;

        /// <summary>
        /// This communicator object is temporary used to reconnect to remote NGRID server.
        /// After connection it is added to communicators list and set to null.
        /// </summary>
        private TCPCommunicator _reconnectingCommunicator;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructur.
        /// </summary>
        /// <param name="name">name of server</param>
        /// <param name="ipAddress">IP Address of server</param>
        /// <param name="port">Listening TCP port of server</param>
        public NGRIDAdjacentServer(string name, string ipAddress, int port)
            : base(name)
        {
            _ipAddress = ipAddress;
            _port = port;
            _lastConnectionAttemptTime = DateTime.MinValue;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// This method is responsible to ensure connection with communicating NGRID server.
        /// Checks connection, reconnects if disconnected and sends ping message.
        /// </summary>
        public void CheckConnection()
        {
            try
            {
                if (IsThereCommunicator())
                {
                    SendPingMessageIfNeeded();
                }

                CheckConnectionAndReConnectIfNeeded();
            }
            catch (Exception ex)
            {
                Logger.Warn("Can not connected to NGRID Server: " + Name);
                Logger.Warn(ex.Message, ex);
            }
        }

        #endregion

        #region Protected / Overrive methods

        /// <summary>
        /// Gets messages from database to be sent to this server.
        /// </summary>
        /// <param name="minId">Minimum Id of message record to get (minId included)</param>
        /// <param name="maxCount">Maximum number of records to get</param>
        /// <returns>List of messages</returns>
        protected override List<NGRIDMessageRecord> GetWaitingMessages(int minId, int maxCount)
        {
            return StorageManager.GetWaitingMessagesOfServer(Name, minId, maxCount);
        }

        /// <summary>
        /// Gets Id of last incoming message that will be sent to this server.
        /// </summary>
        /// <returns>Id of last incoming message</returns>
        protected override int GetMaxWaitingMessageId()
        {
            return StorageManager.GetMaxWaitingMessageIdOfServer(Name);
        }

        /// <summary>
        /// Finds Next server for a message.
        /// </summary>
        /// <returns>Next server</returns>
        protected override string GetNextServerForMessage(NGRIDDataTransferMessage message)
        {
            //Next server is this NGRIDAdjacentServer because message is being sent to that now.
            return Name;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Checks connection and reconnects to NGRID server if needed.
        /// </summary>
        private void CheckConnectionAndReConnectIfNeeded()
        {
            var now = DateTime.Now;

            if (IsThereCommunicator())
            {
                // If any (dublex) communication made within last 90 seconds, then do not create connection.
                if (now.Subtract(LastIncomingMessageTime).TotalSeconds <= 90 &&
                    now.Subtract(LastOutgoingMessageTime).TotalSeconds <= 90)
                {
                    return;
                }
            }

            //Wait 1 second more on every failed connection attempt (Maximum 30 seconds).
            var waitSeconds = Math.Min(++_connectionErrorCount, 30);
            if (now.Subtract(_lastConnectionAttemptTime).TotalSeconds < waitSeconds)
            {
                return;
            }

            Logger.Info("Connecting remote NGRID Server: " + Name + " (Attempt: " + _connectionErrorCount + ")");
            _lastConnectionAttemptTime = DateTime.Now;
            ConnectToServer();
            _connectionErrorCount = 0;
        }

        /// <summary>
        /// Sends a Ping message to NGRID server if 60 seconds passed after last communication.
        /// </summary>
        private void SendPingMessageIfNeeded()
        {
            var now = DateTime.Now;
            if (now.Subtract(LastIncomingMessageTime).TotalSeconds < 60 &&
                now.Subtract(LastOutgoingMessageTime).TotalSeconds < 60)
            {
                return;
            }

            try
            {
                SendMessage(new NGRIDPingMessage());
            }
            catch (Exception ex)
            {
                Logger.Warn(ex.Message, ex);
            }
        }

        /// <summary>
        /// Creates a new TCPCommunicator to communicate with NGRID server.
        /// </summary>
        private void ConnectToServer()
        {
            var ip = IPAddress.Parse(_ipAddress);
            if (ip == null)
            {
                throw new NGRIDException("IP address is not valid: " + _ipAddress);
            }

            var socket = GeneralHelper.ConnectToServerWithTimeout(new IPEndPoint(ip, _port), 10000); //10 seconds
            if (!socket.Connected)
            {
                throw new NGRIDException("TCP connection can not be established.");
            }

            //Create communicator object.
            _reconnectingCommunicator = new TCPCommunicator(socket, CommunicationLayer.CreateCommunicatorId());

            //Register MessageReceived event to receive response of Register message
            _reconnectingCommunicator.MessageReceived += Communicator_MessageReceived;

            //Start communicator and send a register message
            _reconnectingCommunicator.Start();
            _reconnectingCommunicator.SendMessage(new NGRIDRegisterMessage
                                               {
                                                   CommunicationWay = CommunicationWays.SendAndReceive,
                                                   CommunicatorType = CommunicatorTypes.NgridServer,
                                                   Name = Settings.ThisServerName,
                                                   Password = "" //Not implemented yet
                                               });
        }

        /// <summary>
        /// This method is just used to make a new connection with NGRID server.
        /// It receives response of register message and adds communicator to Communicators if successfuly registered.
        /// </summary>
        /// <param name="sender">Creator object of event</param>
        /// <param name="e">Event arguments</param>
        private void Communicator_MessageReceived(object sender, MessageReceivedFromCommunicatorEventArgs e)
        {
            try
            {
                //Message must be NGRIDOperationResultMessage
                var message = e.Message as NGRIDOperationResultMessage;
                if (message == null)
                {
                    throw new NGRIDException("First message must be NGRIDOperationResultMessage");
                }

                //Check if remote NGRID server accepted connection
                if(!message.Success)
                {
                    throw new NGRIDException("Remote NGRID server did not accept connection.");
                }

                //Unregister from event and add communicator to Communicators list.
                e.Communicator.MessageReceived -= Communicator_MessageReceived;
                try
                {
                    AddCommunicator(e.Communicator);
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex.Message, ex);
                    e.Communicator.Stop(false);
                }
            }
            catch (Exception ex)
            {
                Logger.Warn("Can not connected to remote NGRID server: '" + Name + "'. Connection is being closed.");
                Logger.Warn(ex.Message, ex);
                try
                {
                    e.Communicator.Stop(false);
                }
                catch (Exception ex2)
                {
                    Logger.Warn(ex2.Message, ex2);                    
                }
            }
            finally
            {
                _reconnectingCommunicator = null;
            }
        }

        #endregion
    }
}
