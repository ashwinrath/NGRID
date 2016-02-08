using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using log4net;
using NGRID.Communication;
using NGRID.Communication.Channels;
using NGRID.Communication.Messages;
using NGRID.Exceptions;
using NGRID.Threading;

namespace NGRID.Client
{
    /// <summary>
    /// This is the main class that is used by an client application to communicate with NGRID server.
    /// </summary>
    public class NGRIDClient : IDisposable
    {
        #region Events

        /// <summary>
        /// This event is raised when a data transfer message received from NGRID server.
        /// </summary>
        public event MessageReceivedHandler MessageReceived;

        #endregion

        #region Public properties

        /// <summary>
        /// Name of the client application.
        /// </summary>
        public string ApplicationName { get; private set; }

        /// <summary>
        /// Gets/Sets Communication Way between NGRID server.
        /// A receiver may want to do not receive messages anymore, by changing it's communication way to 'Send'.
        /// Setting this property sends a NGRIDChangeCommunicationWayMessage message to NGRID server.
        /// Default value: CommunicationWays.SendAndReceive
        /// </summary>
        public CommunicationWays CommunicationWay
        {
            get { return _communicationChannel.CommunicationWay; }
            set { ChangeCommunicationWay(value); }
        }

        /// <summary>
        /// Communicator Id of this instance of application in NGRID.
        /// This field is valid only if client application is connected and registered to the NGRID server.
        /// </summary>
        public long CommunicatorId
        {
            get { return _communicationChannel.ComminicatorId; }
        }

        /// <summary>
        /// Gets/sets Reconnecting option on any error case.
        /// If this is true, client application attempts to reconnect to NGRID server until it is connected and
        /// doesn't throw exceptions while connecting. 
        /// Default value: True.
        /// </summary>
        public bool ReConnectServerOnError { get; set; }

        /// <summary>
        /// Used to get/set if messages are auto acknowledged.
        /// If AutoAcknowledgeMessages is true, then messages are automatically acknowledged after MessageReceived event,
        /// if they are not acknowledged/rejected before by application.
        /// Default: false.
        /// </summary>
        public bool AutoAcknowledgeMessages { get; set; }

        /// <summary>
        /// Timeout value for outgoing data messages.
        /// Default value: 300000 (5 minutes).
        /// </summary>
        public int DataMessageTimeout { get; set; }
        
        /// <summary>
        /// Time of last message received from NGRID server.
        /// </summary>
        public DateTime LastIncomingMessageTime { get; private set; }

        /// <summary>
        /// Time of last message sent to NGRID server.
        /// </summary>
        public DateTime LastOutgoingMessageTime { get; private set; }

        /// <summary>
        /// MessageId of last received and acknowledged message's Id.
        /// This field is used to do not receive/accept same message again.
        /// If a message is send by NGRID server with same MessageId,
        /// message is discarded and ACK message is sent to server.
        /// </summary>
        public string LastAcknowledgedMessageId { get; private set; }

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
        /// This messages are waiting for a response.
        /// Key: MessageID of waiting request message.
        /// Value: A WaitingMessage instance.
        /// </summary>
        private readonly SortedList<string, WaitingMessage> _waitingMessages;

        /// <summary>
        /// This queue is used to queue and process sequentially messages that are received from NGRID server.
        /// </summary>
        private readonly QueueProcessorThread<NGRIDMessage> _incomingMessageQueue;

        /// <summary>
        /// This timer is used to reconnect to NGRID server if it is disconnected.
        /// </summary>
        private readonly Timer _reconnectTimer;

        /// <summary>
        /// Used to Start/Stop NGRIDClient, and indicates the state.
        /// </summary>
        private volatile bool _running;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new NGRIDClient object using default IP address (127.0.0.1) and default TCP port.
        /// </summary>
        /// <param name="applicationName">Name of the client application that connects to NGRID server</param>
        public NGRIDClient(string applicationName)
            : this(applicationName, CommunicationConsts.DefaultIpAddress, CommunicationConsts.DefaultNGRIDPort)
        {

        }

        /// <summary>
        /// Creates a new NGRIDClient object using default TCP port.
        /// </summary>
        /// <param name="applicationName">Name of the client application that connects to NGRID server</param>
        /// <param name="ipAddress">Ip address of the NGRID server</param>
        public NGRIDClient(string applicationName, string ipAddress)
            : this(applicationName, ipAddress, CommunicationConsts.DefaultNGRIDPort)
        {

        }

        /// <summary>
        /// Creates a new NGRIDClient object.
        /// </summary>
        /// <param name="applicationName">Name of the client application that connects to NGRID server</param>
        /// <param name="ipAddress">Ip address of the NGRID server</param>
        /// <param name="port">Listening TCP Port of NGRID server</param>
        public NGRIDClient(string applicationName, string ipAddress, int port)
        {
            DataMessageTimeout = 300000;

            ApplicationName = applicationName;
            ReConnectServerOnError = true;

            _waitingMessages = new SortedList<string, WaitingMessage>();
            _reconnectTimer = new Timer(ReconnectTimer_Tick, null, Timeout.Infinite, Timeout.Infinite);

            _incomingMessageQueue = new QueueProcessorThread<NGRIDMessage>();
            _incomingMessageQueue.ProcessItem += IncomingMessageQueue_ProcessItem;

            _communicationChannel = new TCPChannel(ipAddress, port);
            _communicationChannel.StateChanged += CommunicationChannel_StateChanged;
            _communicationChannel.MessageReceived += CommunicationChannel_MessageReceived;

            LastIncomingMessageTime = DateTime.MinValue;
            LastOutgoingMessageTime = DateTime.MinValue;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Connects to NGRID server.
        /// If ReConnectServerOnError is true, than does not throw any Exception.
        /// Else, throws Exception if can not connect to the Server.
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
        /// Creates an empty message to send.
        /// </summary>
        /// <returns>Created message</returns>
        public IOutgoingMessage CreateMessage()
        {
            return new OutgoingDataMessage(this);
        }

        /// <summary>
        /// Disposes this object.
        /// </summary>
        public void Dispose()
        {
            if (_communicationChannel != null)
            {
                Disconnect();
            }
        }

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
                var registerMessage = new NGRIDRegisterMessage
                                          {
                                              CommunicationWay = _communicationChannel.CommunicationWay,
                                              CommunicatorType = CommunicatorTypes.Application,
                                              Name = ApplicationName,
                                              Password = ""
                                          };
                var reply = SendAndWaitForReply(
                                registerMessage,
                                NGRIDMessageFactory.MessageTypeIdNGRIDOperationResultMessage,
                                30000) as NGRIDOperationResultMessage;

                if (reply == null)
                {
                    throw new NGRIDException("Can not send register message to the server.");
                }

                if (!reply.Success)
                {
                    CloseCommunicationChannel();
                    throw new NGRIDException("Can not register to server. Detail: " + (reply.ResultText ?? ""));
                }

                //reply.ResultText must be CommunicatorId if successfully registered.
                _communicationChannel.ComminicatorId = Convert.ToInt64(reply.ResultText);
            }
            catch (NGRIDTimeoutException)
            {
                CloseCommunicationChannel();
                throw new NGRIDTimeoutException("Timeout occured. Can not registered to NGRID server.");
            }
        }
        
        /// <summary>
        /// Changes communication way of this application.
        /// So, a receiver may want to do not receive messages anymore by changing it's communication way to 'Send'.
        /// </summary>
        /// <param name="newCommunicationWay">New communication way</param>
        private void ChangeCommunicationWay(CommunicationWays newCommunicationWay)
        {
            if (_communicationChannel == null || 
                _communicationChannel.State != CommunicationStates.Connected ||
                _communicationChannel.CommunicationWay == newCommunicationWay)
            {
                return;
            }

            _communicationChannel.SendMessage(
                new NGRIDChangeCommunicationWayMessage
                    {
                        NewCommunicationWay = newCommunicationWay
                    });
            _communicationChannel.CommunicationWay = newCommunicationWay;
        }

        /// <summary>
        /// Sends a mssage to NGRID server and waits a response for timeoutMilliseconds milliseconds.
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <param name="waitingResponseType">What type of response is being waited</param>
        /// <param name="timeoutMilliseconds">Maximum waiting time for response</param>
        /// <returns>Received message from server</returns>
        private NGRIDMessage SendAndWaitForReply(NGRIDMessage message, int waitingResponseType, int timeoutMilliseconds)
        {
            //Create a WaitingMessage to wait and get response message and add it to waiting messages
            var waitingMessage = new WaitingMessage(waitingResponseType);
            lock (_waitingMessages)
            {
                _waitingMessages[message.MessageId] = waitingMessage;
            }

            try
            {
                //Create a WaitingMessage to wait and get response message and add it to waiting messages
                SendMessageInternal(message);

                //Send message to the server
                waitingMessage.WaitEvent.WaitOne(timeoutMilliseconds);

                //Check for errors
                switch (waitingMessage.State)
                {
                    case WaitingMessageStates.WaitingForResponse:
                        throw new NGRIDTimeoutException("Timeout occured. Can not received response to message.");
                    case WaitingMessageStates.ClientDisconnected:
                        throw new NGRIDException("Client disconnected from NGRID server before response received to message.");
                    case WaitingMessageStates.MessageRejected:
                        throw new NGRIDException("Message is rejected by NGRID server or destination application.");
                }

                if (waitingMessage.ResponseMessage == null)
                {
                    throw new NGRIDException("An unexpected error occured, message can not send.");
                }

                return waitingMessage.ResponseMessage;
            }
            finally
            {
                //Remove message from waiting messages
                lock (_waitingMessages)
                {
                    if (_waitingMessages.ContainsKey(message.MessageId))
                    {
                        _waitingMessages.Remove(message.MessageId);
                    }
                }
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
        /// This method handles StateChanged event of communication channel.
        /// </summary>
        /// <param name="sender">The communication channel</param>
        /// <param name="e">Event arguments</param>
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
                    waitingMessage.State = WaitingMessageStates.ClientDisconnected;
                    waitingMessage.WaitEvent.Set();
                }

                _waitingMessages.Clear();
            }
        }

        /// <summary>
        /// This event handles incoming messages from communication channel.
        /// </summary>
        /// <param name="sender">Communication channel that received message</param>
        /// <param name="e">Event arguments</param>
        private void CommunicationChannel_MessageReceived(ICommunicationChannel sender, Communication.Channels.MessageReceivedEventArgs e)
        {
            //Update last incoming message time
            LastIncomingMessageTime = DateTime.Now;

            //Check for duplicate messages.
            if (e.Message.MessageTypeId == NGRIDMessageFactory.MessageTypeIdNGRIDDataTransferMessage)
            {
                var dataMessage = e.Message as NGRIDDataTransferMessage;
                if (dataMessage != null)
                {
                    if (dataMessage.MessageId == LastAcknowledgedMessageId)
                    {
                        try
                        {
                            SendMessageInternal(new NGRIDOperationResultMessage
                            {
                                RepliedMessageId = dataMessage.MessageId,
                                Success = true,
                                ResultText = "Duplicate message."
                            });
                        }
                        catch (Exception ex)
                        {
                            Logger.Warn(ex.Message, ex);
                        }

                        return;
                    }
                }
            }

            //Check if there is a waiting thread for this message (in SendAndWaitForReply method)
            if (!string.IsNullOrEmpty(e.Message.RepliedMessageId))
            {
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
                    if (waitingMessage.WaitingResponseType == e.Message.MessageTypeId)
                    {
                        waitingMessage.ResponseMessage = e.Message;
                        waitingMessage.State = WaitingMessageStates.ResponseReceived;
                        waitingMessage.WaitEvent.Set();
                        return;
                    }
                    
                    if(e.Message.MessageTypeId == NGRIDMessageFactory.MessageTypeIdNGRIDOperationResultMessage)
                    {
                        var resultMessage = e.Message as NGRIDOperationResultMessage;
                        if ((resultMessage != null) && (!resultMessage.Success))
                        {
                            waitingMessage.State = WaitingMessageStates.MessageRejected;
                            waitingMessage.WaitEvent.Set();
                            return;
                        }
                    }
                }
            }

            //If this message is not a response, then add it to message process queue
            _incomingMessageQueue.Add(e.Message);
        }

        /// <summary>
        /// This event handles processing messages when a message is added to queue (_incomingMessageQueue).
        /// </summary>
        /// <param name="sender">Reference to message queue</param>
        /// <param name="e">Event arguments</param>
        private void IncomingMessageQueue_ProcessItem(object sender, ProcessQueueItemEventArgs<NGRIDMessage> e)
        {
            //Process only NGRIDDataTransferMessage objects.
            if (e.ProcessItem.MessageTypeId != NGRIDMessageFactory.MessageTypeIdNGRIDDataTransferMessage)
            {
                return;
            }

            //Create IncomingDataMessage from NGRIDDataTransferMessage
            var dataMessage = new IncomingDataMessage(this, e.ProcessItem as NGRIDDataTransferMessage);

            try
            {
                //Check if client application registered to MessageReceived event.
                if (MessageReceived == null)
                {
                    dataMessage.Reject("Client application did not registered to MessageReceived event to receive messages.");
                    return;
                }

                //Raise MessageReceived event
                MessageReceived(this, new MessageReceivedEventArgs { Message = dataMessage });

                //Check if client application acknowledged or rejected message
                if (dataMessage.AckState != MessageAckStates.WaitingForAck)
                {
                    return;
                }

                //Check if auto acknowledge is active
                if (!AutoAcknowledgeMessages)
                {
                    dataMessage.Reject("Client application did not acknowledged or rejected message.");
                    return;
                }

                //Auto acknowledge message
                dataMessage.Acknowledge();
            }
            catch
            {
                try
                {
                    dataMessage.Reject("An unhandled exception occured during processing message by client application.");
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex.Message, ex);
                }
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
                //Stop timer until method finishes
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
            catch (Exception ex)
            {
                Logger.Warn(ex.Message, ex);
            }
            finally
            {
                lock (_reconnectTimer)
                {
                    if (_running)
                    {
                        //Restart timer
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
            catch (Exception ex)
            {
                Logger.Warn(ex.Message, ex);
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
            catch (Exception ex)
            {
                Logger.Debug(ex.Message, ex);
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

        #endregion

        #region Sub classes

        #region IncomingDataMessage class

        /// <summary>
        /// Implements IIncomingMessage to send Ack/Reject via NGRIDClient.
        /// </summary>
        private class IncomingDataMessage : NGRIDDataTransferMessage, IIncomingMessage
        {
            /// <summary>
            /// Acknowledgment state of the message.
            /// </summary>
            public MessageAckStates AckState { get; private set; }

            /// <summary>
            /// Reference to the NGRIDClient object.
            /// </summary>
            private readonly NGRIDClient _client;

            /// <summary>
            /// Creates a new IncomingDataMessage object from a NGRIDDataTransferMessage object.
            /// </summary>
            /// <param name="client">Reference to the NGRIDClient object</param>
            /// <param name="message">NGRIDDataTransferMessage object to create IncomingDataMessage</param>
            public IncomingDataMessage(NGRIDClient client, NGRIDDataTransferMessage message)
            {
                _client = client;

                DestinationApplicationName = message.DestinationApplicationName;
                DestinationCommunicatorId = message.DestinationCommunicatorId;
                DestinationServerName = message.DestinationServerName;
                MessageData = message.MessageData;
                MessageId = message.MessageId;
                PassedServers = message.PassedServers;
                RepliedMessageId = message.RepliedMessageId;
                SourceApplicationName = message.SourceApplicationName;
                SourceCommunicatorId = message.SourceCommunicatorId;
                SourceServerName = message.SourceServerName;
                TransmitRule = message.TransmitRule;
            }

            /// <summary>
            /// Used to Acknowledge this message.
            /// Confirms that the message is received safely by client application.
            /// A message must be acknowledged by client application to remove message from message queue.
            /// NGRID server doesn't send next message to the client application until the message is acknowledged.
            /// Also, NGRID server sends same message again if the message is not acknowledged in a certain time.
            /// </summary>
            public void Acknowledge()
            {
                if (AckState == MessageAckStates.Acknowledged)
                {
                    return;
                }
                
                if (AckState == MessageAckStates.Rejected)
                {
                    throw new NGRIDException("Message is rejected before.");
                }

                _client.LastAcknowledgedMessageId = MessageId;

                _client.SendMessageInternal(
                    new NGRIDOperationResultMessage
                    {
                        RepliedMessageId = MessageId,
                        Success = true
                    });

                AckState = MessageAckStates.Acknowledged;
            }

            /// <summary>
            /// Used to reject (to not acknowledge) this message.
            /// Indicates that the message can not received correctly or can not handled the message, and the message 
            /// must be sent to client application later again.
            /// If NGRID server gets reject for a message,
            /// it doesn't send any message to the client application instance for a while.
            /// If message is persistent, than it is sent to another instance of application or to same application instance again. 
            /// If message is not persistent, it is deleted.
            /// </summary>
            public void Reject()
            {
                Reject("");
            }

            /// <summary>
            /// Used to reject (to not acknowledge) this message.
            /// Indicates that the message can not received correctly or can not handled the message, and the message 
            /// must be sent to client application later again.
            /// If NGRID server gets reject for a message,
            /// it doesn't send any message to the client application instance for a while.
            /// If message is persistent, than it is sent to another instance of application or to same application instance again. 
            /// If message is not persistent, it is deleted.
            /// </summary>
            /// <param name="reason">Reject reason</param>
            public void Reject(string reason)
            {
                if (AckState == MessageAckStates.Rejected)
                {
                    return;
                }

                if (AckState == MessageAckStates.Acknowledged)
                {
                    throw new NGRIDException("Message is acknowledged before.");
                }

                _client.SendMessageInternal(
                    new NGRIDOperationResultMessage
                    {
                        RepliedMessageId = MessageId,
                        Success = false,
                        ResultText = reason
                    });

                AckState = MessageAckStates.Rejected;
            }

            /// <summary>
            /// Creates a empty response message for this message.
            /// </summary>
            /// <returns>Response message object</returns>
            public IOutgoingMessage CreateResponseMessage()
            {
                return new OutgoingDataMessage(_client)
                       {
                           DestinationServerName = SourceServerName,
                           DestinationApplicationName = SourceApplicationName,
                           DestinationCommunicatorId = SourceCommunicatorId,
                           RepliedMessageId = MessageId,
                           TransmitRule = TransmitRule
                       };
            }
        }

        #endregion

        #region OutgoingDataMessage class

        /// <summary>
        /// Implements IOutgoingMessage to send message via NGRIDClient.
        /// </summary>
        private class OutgoingDataMessage : NGRIDDataTransferMessage, IOutgoingMessage
        {
            /// <summary>
            /// Reference to the NGRIDClient object.
            /// </summary>
            private readonly NGRIDClient _client;

            /// <summary>
            /// Creates a new OutgoingDataMessage object.
            /// </summary>
            /// <param name="client">Reference to the NGRIDClient object</param>
            public OutgoingDataMessage(NGRIDClient client)
            {
                _client = client;
            }

            /// <summary>
            /// Sends the message to the NGRID server.
            /// If this method does not throw an exception,
            /// message is correctly delivered to NGRID server (persistent message)
            /// or to the destination application (non persistent message).
            /// </summary>
            public void Send()
            {
                Send(_client.DataMessageTimeout);
            }

            /// <summary>
            /// Sends the message to the NGRID server.
            /// If this method does not throw an exception,
            /// message is correctly delivered to NGRID server (persistent message)
            /// or to the destination application (non persistent message).
            /// </summary>
            /// <param name="timeoutMilliseconds">Timeout to send message as milliseconds</param>
            public void Send(int timeoutMilliseconds)
            {
                var reply = _client.SendAndWaitForReply(
                    this,
                    NGRIDMessageFactory.MessageTypeIdNGRIDOperationResultMessage,
                    timeoutMilliseconds
                    ) as NGRIDOperationResultMessage; //Timeout: 5 minutes or 1 minute.

                if (reply == null)
                {
                    throw new NGRIDException("Can not send message. MessageId=(" + MessageId + ").");
                }

                if (!reply.Success)
                {
                    throw new NGRIDException("Can not send message. MessageId=(" + MessageId + "). Detail: " + reply.ResultText);
                }
            }

            /// <summary>
            /// Sends the message and waits for an incoming message for that message.
            /// NGRID can be used for Request/Response type messaging with this method.
            /// Default timeout value: 5 minutes.
            /// </summary>
            /// <returns>Response message</returns>
            public IIncomingMessage SendAndGetResponse()
            {
                return SendAndGetResponse(_client.DataMessageTimeout);
            }

            /// <summary>
            /// Sends the message and waits for an incoming message for that message.
            /// NGRID can be used for Request/Response type messaging with this method.
            /// </summary>
            /// <param name="timeoutMilliseconds">Timeout to get response message as milliseconds</param>
            /// <returns>Response message</returns>
            public IIncomingMessage SendAndGetResponse(int timeoutMilliseconds)
            {
                var response = _client.SendAndWaitForReply(
                                   this,
                                   NGRIDMessageFactory.MessageTypeIdNGRIDDataTransferMessage,
                                   timeoutMilliseconds
                                   ) as NGRIDDataTransferMessage;
                var message = new IncomingDataMessage(_client, response);
                if (_client.AutoAcknowledgeMessages)
                {
                    message.Acknowledge();
                }

                return message;
            }
        }

        #endregion

        #region WaitingMessage class

        /// <summary>
        /// This class is used as item in _waitingMessages collection.
        /// Key: Message ID to wait response.
        /// Value: ManualResetEvent to wait thread until response received.
        /// </summary>
        private class WaitingMessage
        {
            /// <summary>
            /// What type of message is being waited.
            /// For NGRIDOperationResultMessage, it is NGRIDMessageFactory.MessageTypeIdNGRIDOperationResultMessage.
            /// For NGRIDDataTransferMessage, it is NGRIDMessageFactory.MessageTypeIdNGRIDDataTransferMessage. 
            /// </summary>
            public int WaitingResponseType { get; private set; }

            /// <summary>
            /// Response message received for sent message
            /// This message may be NGRIDOperationResultMessage
            /// or NGRIDDataTransferMessage according to WaitingResponseType.
            /// </summary>
            public NGRIDMessage ResponseMessage { get; set; }

            /// <summary>
            /// ManualResetEvent to wait thread until response received.
            /// </summary>
            public ManualResetEvent WaitEvent { get; private set; }

            /// <summary>
            /// State of the message.
            /// </summary>
            public WaitingMessageStates State { get; set; }

            /// <summary>
            /// Creates a new WaitingMessage.
            /// </summary>
            public WaitingMessage(int waitingResponseType)
            {
                WaitingResponseType = waitingResponseType;
                WaitEvent = new ManualResetEvent(false);
                State = WaitingMessageStates.WaitingForResponse;
            }
        }

        public enum WaitingMessageStates
        {
            WaitingForResponse,
            ClientDisconnected,
            MessageRejected,
            ResponseReceived
        }

        #endregion

        #endregion
    }
}
