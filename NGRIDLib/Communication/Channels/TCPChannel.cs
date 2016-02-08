using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using log4net;
using NGRID.Communication.Protocols;
using NGRID.Exceptions;
using NGRID.Communication.Messages;
using NGRID.Serialization;

namespace NGRID.Communication.Channels
{
    /// <summary>
    /// This class is used to connect to NGRID server via TCP sockets.
    /// </summary>
    public class TCPChannel : ICommunicationChannel
    {
        #region Events

        /// <summary>
        /// This event is raised when the state of the client changes.
        /// </summary>
        public event CommunicationStateChangedHandler StateChanged;

        /// <summary>
        /// This event is raised when a NGRIDMessage received.
        /// </summary>
        public event MessageReceivedHandler MessageReceived;

        #endregion

        #region Public Properties

        /// <summary>
        /// Unique identifier for this communicator in connected NGRID server.
        /// This field is not set by communication channel,
        /// it is set by another classes (NGRIDClient) that are using
        /// communication channel. 
        /// </summary>
        public long ComminicatorId { get; set; }

        /// <summary>
        /// Gets the connection state of communicator.
        /// </summary>
        public CommunicationStates State
        {
            get { return _state; }
        }
        private volatile CommunicationStates _state;

        /// <summary>
        /// Communication way for this channel.
        /// This field is not set by communication channel,
        /// it is set by another classes (NGRIDClient) that are using
        /// communication channel. 
        /// </summary>
        public CommunicationWays CommunicationWay { get; set; }

        #endregion

        #region Private Fields

        /// <summary>
        /// Reference to logger.
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// IP address of NGRID server
        /// </summary>
        private readonly string _ipAddress;

        /// <summary>
        /// TCP port of NGRID server
        /// </summary>
        private readonly int _port;

        /// <summary>
        /// The TCP socket to the remote application.
        /// </summary>
        private Socket _socket;

        /// <summary>
        /// The main stream wraps socket to send/receive data.
        /// </summary>
        private NetworkStream _networkStream;

        /// <summary>
        /// This object is used to send/receive messages as byte array.
        /// </summary>
        private readonly INGRIDWireProtocol _wireProtocol;

        /// <summary>
        /// The thread that listens incoming data.
        /// </summary>
        private Thread _thread;

        /// <summary>
        /// Used to send only one message in a time by locking.
        /// </summary>
        private readonly object _sendLock;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new TCPChannel object.
        /// </summary>
        /// <param name="ipAddress">IP address of NGRID server</param>
        /// <param name="port">TCP port of NGRID server</param>
        public TCPChannel(string ipAddress, int port)
        {
            _ipAddress = ipAddress;
            _port = port;
            _state = CommunicationStates.Closed;
            CommunicationWay = CommunicationWays.SendAndReceive;
            _wireProtocol = new NGRIDDefaultWireProtocol();
            _sendLock = new object();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Connects to NGRID server.
        /// </summary>
        public void Connect()
        {
            ChangeState(CommunicationStates.Connecting);

            try
            {
                var ip = IPAddress.Parse(_ipAddress);

                _socket = GeneralHelper.ConnectToServerWithTimeout(new IPEndPoint(ip, _port), 5000); //5 seconds
                if (!_socket.Connected)
                {
                    throw new NGRIDException("TCP connection can not be established.");
                }

                _socket.NoDelay = true;
                //Create stream object to read/write from/to socket.
                _networkStream = new NetworkStream(_socket);

                //Create and start a new thread to communicate with NGRID server
                _thread = new Thread(DoCommunicateAsThread);
                _thread.Start();

                ChangeState(CommunicationStates.Connected);
            }
            catch (Exception)
            {
                ChangeState(CommunicationStates.Closed);
                throw;
            }
        }

        /// <summary>
        /// Disconnects from NGRID server.
        /// </summary>
        public void Disconnect()
        {
            if (_socket == null)
            {
                return;
            }

            ChangeState(CommunicationStates.Closing);
            try
            {
                if (_socket.Connected)
                {
                    _socket.Shutdown(SocketShutdown.Send);
                    _socket.Close();
                }
            }
            finally
            {
                _socket = null;
                ChangeState(CommunicationStates.Closed);
            }
        }

        /// <summary>
        /// Sends a NGRIDMessage to the NGRID server
        /// </summary>
        /// <param name="message">Message to send</param>
        public void SendMessage(NGRIDMessage message)
        {
            lock (_sendLock)
            {
                if (State != CommunicationStates.Connected || !_socket.Connected)
                {
                    throw new NGRIDException("Client's state is not connected. It can not send message.");
                }

                SendMessageToSocket(message);
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Entrance point of the thread.
        /// This method run by thread to listen incoming data from communicator.
        /// </summary>
        private void DoCommunicateAsThread()
        {
            while (State == CommunicationStates.Connected || State == CommunicationStates.Connecting)
            {
                try
                {
                    //Read a message from _networkStream (socket) and raise MessageReceived event
                    OnMessageReceived(
                        _wireProtocol.ReadMessage(
                            new NGRIDDefaultDeserializer(_networkStream)
                            ));
                }
                catch
                {
                    //Stop listening on an error case
                    break;
                }
            } //while

            //if socket is steel connected, then close it
            try
            {
                Disconnect();
            }
            catch(Exception ex)
            {
                Logger.Debug(ex.Message, ex);
            }

            _thread = null;
        }

        /// <summary>
        /// Sends NGRIDMessage object to the socket.
        /// </summary>
        /// <param name="message">Message to be sent</param>
        private void SendMessageToSocket(NGRIDMessage message)
        {
            //Create MemoryStream to write message to a byte array
            var memoryStream = new MemoryStream();

            //Write message
            _wireProtocol.WriteMessage(new NGRIDDefaultSerializer(memoryStream), message);

            //Check the length of message data
            if (memoryStream.Length > CommunicationConsts.MaxMessageSize)
            {
                throw new Exception("Message is too big to send.");
            }
            
            //SendMessage message (contents of created memory stream)
            var sendBuffer = memoryStream.ToArray();
            var length = sendBuffer.Length;
            var totalSent = 0;
            while (totalSent < length)
            {
                var sent = _socket.Send(sendBuffer, totalSent, length - totalSent, SocketFlags.None);
                if (sent <= 0)
                {
                    throw new Exception("Message can not be sent via TCP socket. Only " + totalSent + " bytes of " + length + " bytes are sent.");
                }

                totalSent += sent;
            }
        }
        
        /// <summary>
        /// Changes the state of the client and raises StateChanged event.
        /// </summary>
        /// <param name="newState">New state</param>
        private void ChangeState(CommunicationStates newState)
        {
            var oldState = _state;
            _state = newState;
            if (StateChanged != null)
            {
                StateChanged(this, new CommunicationStateChangedEventArgs { OldState = oldState });
            }
        }

        /// <summary>
        /// When a NGRIDMessage received from NGRID server, this method is called to raise MessageReceived event.
        /// </summary>
        /// <param name="message">Incoming message from server</param>
        private void OnMessageReceived(NGRIDMessage message)
        {
            if (MessageReceived == null)
            {
                return;
            }

            try
            {
                MessageReceived(this,
                                new MessageReceivedEventArgs
                                {
                                    Message = message
                                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }
        }

        #endregion
    }
}