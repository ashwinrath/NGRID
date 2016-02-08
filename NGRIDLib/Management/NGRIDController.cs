using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using log4net;
using NGRID.Communication;
using NGRID.Exceptions;
using NGRID.Serialization;
using NGRID.Communication.Channels;
using NGRID.Communication.Messages;
using NGRID.Communication.Messages.ControllerMessages;
using NGRID.Threading;

namespace NGRID.Management
{
    /// <summary>
    /// This class is used to connect to and communicate with NGRID server from NGRID Manager (Controller).
    /// </summary>
    public class NGRIDController :IDisposable
    {
        #region Events

        /// <summary>
        /// This event is raised when a data transfer message received from NGRID server.
        /// </summary>
        public event ControlMessageReceivedHandler ControlMessageReceived;

        #endregion

        #region Public properties

        /// <summary>
        /// Gets sets Reconnecting option on any error case.
        /// If this is true, controller application attempts to reconnec to NGRID server until it is connected,
        /// NGRIDController doesn't throw exceptions while connecting.  
        /// Default value: True.
        /// </summary>
        public bool ReConnectServerOnError { get; set; }

        #endregion

        #region Private fields

        /// <summary>
        /// Reference to logger.
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Communication channel that is used to communicate with NGRID server.
        /// </summary>
        private readonly ICommunicationChannel _communicationChannel;

        /// <summary>
        /// This queue is used to queue NGRIDMessage objects received from NGRID server and process them sequentially.
        /// </summary>
        private readonly QueueProcessorThread<NGRIDMessage> _incomingMessageQueue;

        /// <summary>
        /// This collection is used to send message and get response in SendMessageAndGetResponse method.
        /// SendMessageAndGetResponse method must wait until response received. It waits using this collection.
        /// Key: Message ID to wait response.
        /// Value: ManualResetEvent to wait thread until response received.
        /// </summary>
        private readonly SortedList<string, WaitingMessage> _waitingMessages;

        /// <summary>
        /// Time of last message received from NGRID server.
        /// </summary>
        public DateTime LastIncomingMessageTime { get; set; }

        /// <summary>
        /// Time of last message sent to NGRID server.
        /// </summary>
        public DateTime LastOutgoingMessageTime { get; set; }

        /// <summary>
        /// This timer is used to reconnect to NGRID server if it is disconnected.
        /// </summary>
        private readonly Timer _reconnectTimer;

        /// <summary>
        /// Used to Start/Stop NGRIDController, and indicates the state.
        /// </summary>
        private volatile bool _running;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new NGRIDClient object.
        /// </summary>
        /// <param name="ipAddress">Ip address of the NGRID server</param>
        /// <param name="port">Listening TCP Port of NGRID server</param>
        public NGRIDController(string ipAddress, int port)
        {
            ReConnectServerOnError = true;

            _reconnectTimer = new Timer(ReconnectTimer_Tick, null, Timeout.Infinite, Timeout.Infinite);
            _waitingMessages = new SortedList<string, WaitingMessage>();

            _incomingMessageQueue = new QueueProcessorThread<NGRIDMessage>();
            _incomingMessageQueue.ProcessItem += IncomingMessageQueue_ProcessItem;

            _communicationChannel = new TCPChannel(ipAddress, port);
            _communicationChannel.MessageReceived += CommunicationChannel_MessageReceived;
            _communicationChannel.StateChanged += CommunicationChannel_StateChanged;

            LastIncomingMessageTime = DateTime.MinValue;
            LastOutgoingMessageTime = DateTime.MinValue;
        }

        #endregion

        #region Public methods

        #region Connect / Disconnect / Dispose methods

        /// <summary>
        /// Connects to NGRID server.
        /// </summary>
        public void Connect()
        {
            _incomingMessageQueue.Start();

            try
            {
                _running = true;
                ConnectAndRegister();
            }
            catch (Exception)
            {
                if (!ReConnectServerOnError)
                {
                    _running = false;
                    throw;
                }
            }

            _reconnectTimer.Change(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        }

        /// <summary>
        /// Disconnects from NGRID server.
        /// </summary>
        public void Disconnect()
        {
            lock (_reconnectTimer)
            {
                _running = false;
                _reconnectTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }

            CloseCommunicationChannel();
            _incomingMessageQueue.Stop(true);
        }


        /// <summary>
        /// Disposes NGRIDController object.
        /// It also disconnects from server if it is connected.
        /// </summary>
        public void Dispose()
        {
            if (_communicationChannel != null)
            {
                Disconnect();
            }
        }

        #endregion

        #region Message sending methods

        /// <summary>
        /// Sends a ControlMessage to NGRID server.
        /// </summary>
        /// <param name="message">Message to send</param>
        public void SendMessage(ControlMessage message)
        {
            SendMessageInternal(new NGRIDControllerMessage
                                    {
                                        MessageData = NGRIDSerializationHelper.SerializeToByteArray(message),
                                        ControllerMessageTypeId = message.MessageTypeId
                                    });
        }

        /// <summary>
        /// Sends a ControlMessage to NGRID server and gets it's response message.
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <returns>Response message from server</returns>
        public ControlMessage SendMessageAndGetResponse(ControlMessage message)
        {
            //Create a WaitingMessage to wait and get response message and add it to waiting messages
            var outgoingMessage = new NGRIDControllerMessage
                                      {
                                          MessageData = NGRIDSerializationHelper.SerializeToByteArray(message),
                                          ControllerMessageTypeId = message.MessageTypeId
                                      };
            var waitingMessage = new WaitingMessage();
            lock (_waitingMessages)
            {
                _waitingMessages[outgoingMessage.MessageId] = waitingMessage;
            }

            try
            {
                //Send message to the server
                SendMessageInternal(outgoingMessage);

                //Wait until thread is signalled by another thread to get response (Signalled by CommunicationChannel_MessageReceived method)
                waitingMessage.WaitEvent.WaitOne(TimeSpan.FromSeconds(90));

                //Check if response received or timeout occured
                if(waitingMessage.ResponseMessage == null)
                {
                    throw new NGRIDException("Timeout occured. Response message did not received.");
                }
                
                return DeserializeControlMessage(waitingMessage.ResponseMessage);
            }
            finally
            {
                //Remove message from waiting messages
                lock (_waitingMessages)
                {
                    if (_waitingMessages.ContainsKey(outgoingMessage.MessageId))
                    {
                        _waitingMessages.Remove(outgoingMessage.MessageId);
                    }
                }
            }
        }

        #endregion

        #endregion

        #region Private methods

        /// <summary>
        /// Connects and registers to NGRID server.
        /// </summary>
        private void ConnectAndRegister()
        {
            _communicationChannel.Connect();
            try
            {
                SendMessageInternal(
                    new NGRIDRegisterMessage
                        {
                            CommunicationWay = CommunicationWays.SendAndReceive,
                            CommunicatorType = CommunicatorTypes.Controller,
                            Name = "NGRIDController",
                            Password = ""
                        });
            }
            catch (NGRIDTimeoutException)
            {
                CloseCommunicationChannel();
                throw new NGRIDTimeoutException("Timeout occured. Can not registered to NGRID server.");
            }
        }

        /// <summary>
        /// Sends a NGRIDMessage object to NGRID server.
        /// </summary>
        /// <param name="message"></param>
        private void SendMessageInternal(NGRIDMessage message)
        {
            try
            {
                _communicationChannel.SendMessage(message);
                LastOutgoingMessageTime = DateTime.Now;
            }
            catch (Exception)
            {
                CloseCommunicationChannel();
                throw;
            }
        }

        /// <summary>
        /// This event handles incoming messages from communication channel.
        /// </summary>
        /// <param name="sender">Communication channel that received message</param>
        /// <param name="e">Event arguments</param>
        private void CommunicationChannel_MessageReceived(ICommunicationChannel sender, MessageReceivedEventArgs e)
        {
            LastIncomingMessageTime = DateTime.Now;

            if ((e.Message.MessageTypeId == NGRIDMessageFactory.MessageTypeIdNGRIDControllerMessage) && (!string.IsNullOrEmpty(e.Message.RepliedMessageId)))
            {
                //Find and send signal/pulse to waiting thread for this message
                WaitingMessage waitingMessage = null;
                lock (_waitingMessages)
                {
                    if (_waitingMessages.ContainsKey(e.Message.RepliedMessageId))
                    {
                        waitingMessage = _waitingMessages[e.Message.RepliedMessageId];
                    }
                }

                if (waitingMessage != null)
                {
                    waitingMessage.ResponseMessage = e.Message as NGRIDControllerMessage;
                    waitingMessage.WaitEvent.Set();
                    return;
                }
            }
            
            //Add message to queue to process in a seperated thread
            _incomingMessageQueue.Add(e.Message);
        }

        private void CommunicationChannel_StateChanged(ICommunicationChannel sender, CommunicationStateChangedEventArgs e)
        {
            //Process only Closed event
            if (sender.State != CommunicationStates.Closed)
            {
                return;
            }

            //Pulse waiting threads for incoming messages, because disconnected to the server, and can not receive message anymore.
            lock (_waitingMessages)
            {
                foreach (var waitingMessage in _waitingMessages.Values)
                {
                    waitingMessage.WaitEvent.Set();
                }

                _waitingMessages.Clear();
            }
        }

        /// <summary>
        /// This event handles processing messages when a message is added to queue (_incomingMessageQueue).
        /// </summary>
        /// <param name="sender">Reference to message queue</param>
        /// <param name="e">Event arguments</param>
        private void IncomingMessageQueue_ProcessItem(object sender, ProcessQueueItemEventArgs<NGRIDMessage> e)
        {
            try
            {
                if (e.ProcessItem.MessageTypeId == NGRIDMessageFactory.MessageTypeIdNGRIDControllerMessage && ControlMessageReceived != null)
                {
                    var controllerMessage = e.ProcessItem as NGRIDControllerMessage;
                    if (controllerMessage == null)
                    {
                        return;
                    }

                    ControlMessageReceived(this, new ControlMessageReceivedEventArgs(DeserializeControlMessage(controllerMessage)));
                }
            }
            catch(Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }
        }

        /// <summary>
        /// This method is called by _reconnectTimer_Tick to reconnect NGRID server if disconnected.
        /// </summary>
        /// <param name="state">This argument is not used</param>
        private void ReconnectTimer_Tick(object state)
        {
            try
            {
                _reconnectTimer.Change(Timeout.Infinite, Timeout.Infinite);

                //Send Ping message if connected and needed to send a Ping message
                if (_running && IsConnectedToServer())
                {
                    SendPingMessageIfNeeded();
                }

                //Reconnect if disconnected
                if (_running && !IsConnectedToServer())
                {
                    ConnectAndRegister();
                }
            }
            catch
            {
                //No action on error case
            }
            finally
            {
                lock (_reconnectTimer)
                {
                    if (_running)
                    {
                        _reconnectTimer.Change(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
                    }
                }
            }
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
                SendMessageInternal(new NGRIDPingMessage());
            }
            catch
            {

            }
        }

        /// <summary>
        /// Closes communication channel, thus disconnects from NGRID server if it is connected.
        /// </summary>
        private void CloseCommunicationChannel()
        {
            try
            {
                _communicationChannel.Disconnect();
            }
            catch
            {

            }
        }

        /// <summary>
        /// Checks if client application is connected to NGRID server.
        /// </summary>
        /// <returns>True, if connected.</returns>
        private bool IsConnectedToServer()
        {
            return (_communicationChannel != null && _communicationChannel.State == CommunicationStates.Connected);
        }
        
        /// <summary>
        /// Deserializes a ControlMessage from a NGRIDControllerMessage.
        /// </summary>
        /// <param name="controllerMessage">NGRIDControllerMessage that includes ControlMessage</param>
        /// <returns>Deserialized ControlMessage object.</returns>
        private static ControlMessage DeserializeControlMessage(NGRIDControllerMessage controllerMessage)
        {
            return NGRIDSerializationHelper.DeserializeFromByteArray(
                () =>
                ControlMessageFactory.CreateMessageByTypeId(controllerMessage.ControllerMessageTypeId),
                controllerMessage.MessageData);
        }

        #endregion

        #region Sub classes

        /// <summary>
        /// This class is used as item in _waitingMessages collection.
        /// Key: Message ID to wait response.
        /// Value: ManualResetEvent to wait thread until response received.
        /// </summary>
        /// </summary>
        private class WaitingMessage
        {
            /// <summary>
            /// ManualResetEvent to wait thread until response received.
            /// </summary>
            public ManualResetEvent WaitEvent { get; private set; }

            /// <summary>
            /// Response message received for sent message
            /// </summary>
            public NGRIDControllerMessage ResponseMessage { get; set; }

            /// <summary>
            /// Creates a new WaitingMessage.
            /// </summary>
            public WaitingMessage()
            {
                WaitEvent = new ManualResetEvent(false);
            }
        }

        #endregion
    }
}
