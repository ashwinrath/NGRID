using System;
using System.Collections.Generic;
using System.IO;
using NGRID.Communication.Events;
using NGRID.Communication.Messages.ControllerMessages;
using NGRID.Exceptions;
using NGRID.Serialization;
using NGRID.Communication;
using NGRID.Communication.Messages;
using NGRID.Settings;

namespace NGRID.Organization
{
    /// <summary>
    /// Represents a NGRID controller that can monitor/manage to this server.
    /// </summary>
    public class NGRIDController : NGRIDRemoteApplication
    {
        #region Public fiedls

        /// <summary>
        /// Communicator type for Controllers.
        /// </summary>
        public override CommunicatorTypes CommunicatorType
        {
            get { return CommunicatorTypes.Controller; }
        }

        /// <summary>
        /// Reference to Organization Layer.
        /// </summary>
        public OrganizationLayer OrganizationLayer { private get; set; }

        #endregion

        #region Private fields

        /// <summary>
        /// Reference to Settings.
        /// </summary>
        private readonly NGRIDSettings _settings;

        /// <summary>
        /// Reference to Design Settings.
        /// </summary>
        private readonly NGRIDDesignSettings _designSettings;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new NGRIDController object.
        /// </summary>
        /// <param name="name">Name of the controller</param>
        public NGRIDController(string name)
            : base(name, CommunicationLayer.CreateApplicationId())
        {
            _settings = NGRIDSettings.Instance;
            //_designSettings = NGRIDDesignSettings.Instance;
            MessageReceived += NGRIDController_MessageReceived;
        }

        #endregion

        #region Public methods

        public override void Start()
        {
            base.Start();

            //Register events of remote applications
            var applicationList = OrganizationLayer.GetClientApplications();
            foreach (var clientApplication in applicationList)
            {
                clientApplication.CommunicatorConnected += ClientApplication_CommunicatorConnected;
                clientApplication.CommunicatorDisconnected += ClientApplication_CommunicatorDisconnected;
            }
        }

        #endregion

        #region Private methods

        #region Message handling and processing mehods

        /// <summary>
        /// Handles MessageReceived event.
        /// All messages received from all controllers comes to this method.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        private void NGRIDController_MessageReceived(object sender, MessageReceivedFromRemoteApplicationEventArgs e)
        {
            try
            {
                //Response to Ping messages
                if ((e.Message.MessageTypeId == NGRIDMessageFactory.MessageTypeIdNGRIDPingMessage) && string.IsNullOrEmpty(e.Message.RepliedMessageId))
                {
                    //Reply ping message
                    SendMessage(new NGRIDPingMessage { RepliedMessageId = e.Message.MessageId }, e.Communicator);
                    return;
                }

                //Do not process messages other than NGRIDControllerMessage
                if (e.Message.MessageTypeId != NGRIDMessageFactory.MessageTypeIdNGRIDControllerMessage)
                {
                    return;
                }

                //Cast message to NGRIDControllerMessage
                var controllerMessage = e.Message as NGRIDControllerMessage;
                if (controllerMessage == null)
                {
                    return;
                }

                //Create (deserialize) ControlMessage from MessageData of controllerMessage object
                var controlMessage = NGRIDSerializationHelper.DeserializeFromByteArray(
                    () =>
                    ControlMessageFactory.CreateMessageByTypeId(controllerMessage.ControllerMessageTypeId),
                    controllerMessage.MessageData);

                //Process message
                ProcessControllerMessage(e.Communicator, controllerMessage, controlMessage);
            }
            catch (Exception ex)
            {
                Logger.Warn(ex.Message, ex);
            }
        }

        /// <summary>
        /// This methods checks type of message (MessageTypeId) and calls appropriate method to process message.
        /// </summary>
        /// <param name="communicator">Communicator that sent message</param>
        /// <param name="controllerMessage">NGRIDControllerMessage object that includes controlMessage</param>
        /// <param name="controlMessage">The message to be processed</param>
        private void ProcessControllerMessage(ICommunicator communicator, NGRIDControllerMessage controllerMessage , ControlMessage controlMessage)
        {
            switch (controlMessage.MessageTypeId)
            {
                case ControlMessageFactory.MessageTypeIdGetApplicationListMessage:
                    ProcessGetApplicationListMessage(communicator, controllerMessage);
                    break;
                case ControlMessageFactory.MessageTypeIdAddNewApplicationMessage:
                    ProcessAddNewApplicationMessage(controlMessage as AddNewApplicationMessage);
                    break;
                case ControlMessageFactory.MessageTypeIdRemoveApplicationMessage:
                    ProcessRemoveApplicationMessage(communicator, controlMessage as RemoveApplicationMessage, controllerMessage);
                    break;
                case ControlMessageFactory.MessageTypeIdGetServerGraphMessage:
                    ProcessGetServerGraphMessage(communicator, controllerMessage);
                    break;
                case ControlMessageFactory.MessageTypeIdUpdateServerGraphMessage:
                    ProcessUpdateServerGraphMessage(communicator, controlMessage as UpdateServerGraphMessage, controllerMessage);
                    break;
                case ControlMessageFactory.MessageTypeIdGetApplicationWebServicesMessage:
                    ProcessGetApplicationWebServicesMessage(communicator, controlMessage as GetApplicationWebServicesMessage, controllerMessage);
                    break;
                case ControlMessageFactory.MessageTypeIdUpdateApplicationWebServicesMessage:
                    ProcessUpdateApplicationWebServicesMessage(communicator, controlMessage as UpdateApplicationWebServicesMessage, controllerMessage);
                    break;
                default:
                    throw new NGRIDException("Undefined MessageTypeId for ControlMessage: " + controlMessage.MessageTypeId);
            }
        }

        #region GetApplicationListMessage

        /// <summary>
        /// Processes GetApplicationListMessage.
        /// </summary>
        /// <param name="communicator">Communicator that sent message</param>
        /// <param name="controllerMessage">NGRIDControllerMessage object that includes controlMessage</param>
        private void ProcessGetApplicationListMessage(ICommunicator communicator, NGRIDControllerMessage controllerMessage)
        {
            //Get all client applications
            var applicationList = OrganizationLayer.GetClientApplications();

            //Create ClientApplicationInfo array
            var clientApplications = new GetApplicationListResponseMessage.ClientApplicationInfo[applicationList.Length];
            for (var i = 0; i < applicationList.Length; i++)
            {
                clientApplications[i] = new GetApplicationListResponseMessage.ClientApplicationInfo
                                         {
                                             Name = applicationList[i].Name,
                                             CommunicatorCount = applicationList[i].ConnectedCommunicatorCount
                                         };
            }

            //Send response message
            ReplyMessageToCommunicator(
                communicator,
                new GetApplicationListResponseMessage
                    {
                        ClientApplications = clientApplications
                    },
                controllerMessage
                );
        }

        #endregion

        #region AddNewApplicationMessage

        /// <summary>
        /// Processes AddNewApplicationMessage.
        /// </summary>
        /// <param name="controlMessage">The message to be processed</param>
        private void ProcessAddNewApplicationMessage(AddNewApplicationMessage controlMessage)
        {
            var addedApplication = OrganizationLayer.AddApplication(controlMessage.ApplicationName);
            addedApplication.CommunicatorConnected += ClientApplication_CommunicatorConnected;
            addedApplication.CommunicatorDisconnected += ClientApplication_CommunicatorDisconnected;
            SendMessageToAllReceivers(
                new ClientApplicationRefreshEventMessage
                    {
                        Name = addedApplication.Name,
                        CommunicatorCount = addedApplication.ConnectedCommunicatorCount
                    });
        }

        #endregion

        #region RemoveApplicationMessage

        /// <summary>
        /// Processes RemoveApplicationMessage.
        /// </summary>
        /// <param name="communicator">Communicator that sent message</param>
        /// <param name="controlMessage">The message to be processed</param>
        /// <param name="controllerMessage">NGRIDControllerMessage object that includes controlMessage</param>
        private void ProcessRemoveApplicationMessage(ICommunicator communicator, RemoveApplicationMessage controlMessage, NGRIDControllerMessage controllerMessage)
        {
            try
            {
                var removedApplication = OrganizationLayer.RemoveApplication(controlMessage.ApplicationName);
                removedApplication.CommunicatorConnected -= ClientApplication_CommunicatorConnected;
                removedApplication.CommunicatorDisconnected -= ClientApplication_CommunicatorDisconnected;

                ReplyMessageToCommunicator(
                    communicator,
                    new RemoveApplicationResponseMessage
                        {
                            ApplicationName = controlMessage.ApplicationName,
                            Removed = true,
                            ResultMessage = "Success."
                        },
                    controllerMessage
                    );

                SendMessageToAllReceivers(
                    new ClientApplicationRemovedEventMessage
                        {
                            ApplicationName = removedApplication.Name
                        });
            }
            catch (Exception ex)
            {
                Logger.Warn(ex.Message, ex);
                ReplyMessageToCommunicator(
                    communicator,
                    new RemoveApplicationResponseMessage
                        {
                            ApplicationName = controlMessage.ApplicationName,
                            Removed = false,
                            ResultMessage = ex.Message
                        },
                    controllerMessage
                    );
            }
        }

        #endregion

        #region GetServerGraphMessage

        /// <summary>
        /// Processes GetServerGraphMessage.
        /// </summary>
        /// <param name="communicator">Communicator that sent message</param>
        /// <param name="controllerMessage">NGRIDControllerMessage object that includes controlMessage</param>
        private void ProcessGetServerGraphMessage(ICommunicator communicator, NGRIDControllerMessage controllerMessage)
        {
            //Create response message
            var responseMessage =
                new GetServerGraphResponseMessage
                    {
                        ServerGraph =
                            new ServerGraphInfo
                                {
                                    ThisServerName = _settings.ThisServerName,
                                    Servers = new ServerGraphInfo.ServerOnGraph[_settings.Servers.Count]
                                }
                    };

            //Fill server settings
            for (var i = 0; i < _settings.Servers.Count; i++)
            {
                responseMessage.ServerGraph.Servers[i] = new ServerGraphInfo.ServerOnGraph
                                                             {
                                                                 Name = _settings.Servers[i].Name,
                                                                 IpAddress = _settings.Servers[i].IpAddress,
                                                                 Port = _settings.Servers[i].Port,
                                                                 Adjacents = _settings.Servers[i].Adjacents
                                                             };
            }

            //Fill server design settings
            for (var i = 0; i < responseMessage.ServerGraph.Servers.Length; i++)
            {
                foreach (var serverDesignItem in _designSettings.Servers)
                {
                    if (responseMessage.ServerGraph.Servers[i].Name == serverDesignItem.Name)
                    {
                        responseMessage.ServerGraph.Servers[i].Location = serverDesignItem.Location;
                        break;
                    }
                }
            }

            //Send response message
            ReplyMessageToCommunicator(
                communicator,
                responseMessage,
                controllerMessage
                );
        }

        #endregion

        #region UpdateServerGraphMessage

        /// <summary>
        /// Processes UpdateServerGraphMessage.
        /// </summary>
        /// <param name="communicator">Communicator that sent message</param>
        /// <param name="controlMessage">The message to be processed</param>
        /// <param name="controllerMessage">NGRIDControllerMessage object that includes controlMessage</param>
        private void ProcessUpdateServerGraphMessage(ICommunicator communicator, UpdateServerGraphMessage controlMessage, NGRIDControllerMessage controllerMessage)
        {
            try
            {
                var newSettings = new NGRIDSettings(Path.Combine(GeneralHelper.GetCurrentDirectory(), "NGRIDSettings.xml"));
                //var newDesignSettings = new NGRIDDesignSettings(Path.Combine(GeneralHelper.GetCurrentDirectory(), "NGRIDSettings.design.xml"));

                //Clear existing server lists
                newSettings.Servers.Clear();
                //newDesignSettings.Servers.Clear();

                //Add servers from UpdateServerGraphMessage
                newSettings.ThisServerName = controlMessage.ServerGraph.ThisServerName;
                foreach (var server in controlMessage.ServerGraph.Servers)
                {
                    //Settings
                    newSettings.Servers.Add(
                        new ServerInfoItem
                        {
                            Name = server.Name,
                            IpAddress = server.IpAddress,
                            Port = server.Port,
                            Adjacents = server.Adjacents
                        });
                    /*Design settings
                    newDesignSettings.Servers.Add(
                        new ServerDesignItem
                        {
                            Name = server.Name,
                            Location = server.Location
                        });*/
                }

                //Save settings
                newSettings.SaveToXml();
                //newDesignSettings.SaveToXml();
            }
            catch (Exception ex)
            {
                //Send fail message
                ReplyMessageToCommunicator(
                    communicator,
                    new OperationResultMessage {Success = false, ResultMessage = ex.Message},
                    controllerMessage
                    );
                return;
            }

            //Send success message
            ReplyMessageToCommunicator(
                communicator,
                new OperationResultMessage {Success = true, ResultMessage = "Success"},
                controllerMessage
                );
        }

        #endregion

        #region GetApplicationWebServicesMessage

        private void ProcessGetApplicationWebServicesMessage(ICommunicator communicator, GetApplicationWebServicesMessage message, NGRIDControllerMessage controllerMessage)
        {
            try
            {
                //Find application
                ApplicationInfoItem application = null;
                foreach (var applicationInfoItem in _settings.Applications)
                {
                    if(applicationInfoItem.Name == message.ApplicationName)
                    {
                        application = applicationInfoItem;
                    }
                }

                if(application == null)
                {
                    //Send message
                    ReplyMessageToCommunicator(
                        communicator,
                        new GetApplicationWebServicesResponseMessage
                            {
                                WebServices = null,
                                Success = false,
                                ResultText = "No application found with name '" + message.ApplicationName + "'."
                            },
                        controllerMessage
                        );

                    return;
                }

                var webServiceList = new List<ApplicationWebServiceInfo>();
                foreach (var channel in application.CommunicationChannels)
                {
                    if ("WebService".Equals(channel.CommunicationType, StringComparison.OrdinalIgnoreCase))
                    {
                        webServiceList.Add(new ApplicationWebServiceInfo {Url = channel.CommunicationSettings["Url"]});
                    }
                }

                //Send web service list
                ReplyMessageToCommunicator(
                    communicator,
                    new GetApplicationWebServicesResponseMessage
                        {
                            WebServices = webServiceList.ToArray(),
                            Success = true,
                            ResultText = "Success."
                        },
                    controllerMessage
                    );
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }
        }

        #endregion

        #region UpdateApplicationWebServicesMessage

        private void ProcessUpdateApplicationWebServicesMessage(ICommunicator communicator, UpdateApplicationWebServicesMessage message, NGRIDControllerMessage controllerMessage)
        {
            try
            {
                //Find application
                ApplicationInfoItem application = null;
                foreach (var applicationInfoItem in _settings.Applications)
                {
                    if (applicationInfoItem.Name == message.ApplicationName)
                    {
                        application = applicationInfoItem;
                    }
                }

                if (application == null)
                {
                    //Send message
                    ReplyMessageToCommunicator(
                        communicator,
                        new OperationResultMessage()
                        {
                            Success = false,
                            ResultMessage = "No application found with name '" + message.ApplicationName + "'."
                        },
                        controllerMessage
                        );
                    return;
                }

                //Delete old service list
                application.CommunicationChannels.Clear();

                //Add new services
                if (message.WebServices != null && message.WebServices.Length > 0)
                {
                    foreach (var webServiceInfo in message.WebServices)
                    {
                        var channelInfo = new ApplicationInfoItem.CommunicationChannelInfoItem { CommunicationType = "WebService" };
                        channelInfo.CommunicationSettings["Url"] = webServiceInfo.Url;
                        application.CommunicationChannels.Add(channelInfo);
                    }
                }

                try
                {
                    //Save settings
                    _settings.SaveToXml();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.Message, ex);
                    ReplyMessageToCommunicator(
                        communicator,
                        new OperationResultMessage()
                        {
                            Success = false,
                            ResultMessage = "Can not save XML configuration file (NGRIDSettings.xml)."
                        },
                        controllerMessage
                        );
                    return;
                }

                //Send success message
                ReplyMessageToCommunicator(
                    communicator,
                    new OperationResultMessage()
                    {
                        Success = true,
                        ResultMessage = "Success."
                    },
                    controllerMessage
                    );
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                ReplyMessageToCommunicator(
                    communicator,
                    new OperationResultMessage()
                    {
                        Success = false,
                        ResultMessage = ex.Message
                    },
                    controllerMessage
                    );
                return;
            }
        }

        #endregion

        #endregion

        #region Other Event handling methods

        /// <summary>
        /// Handles CommunicatorConnected event of all client applications.
        /// </summary>
        /// <param name="sender">Creates of event (application)</param>
        /// <param name="e">Event arguments</param>
        private void ClientApplication_CommunicatorConnected(object sender, CommunicatorConnectedEventArgs e)
        {
            var application = sender as NGRIDRemoteApplication;
            if (application == null)
            {
                return;
            }

            SendMessageToAllReceivers(new ClientApplicationRefreshEventMessage
                                          {
                                              Name = application.Name,
                                              CommunicatorCount = application.ConnectedCommunicatorCount
                                          });
        }

        /// <summary>
        /// Handles CommunicatorDisconnected event of all client applications.
        /// </summary>
        /// <param name="sender">Creates of event (application)</param>
        /// <param name="e">Event arguments</param>
        void ClientApplication_CommunicatorDisconnected(object sender, CommunicatorDisconnectedEventArgs e)
        {
            var application = sender as NGRIDRemoteApplication;
            if (application == null)
            {
                return;
            }

            SendMessageToAllReceivers(new ClientApplicationRefreshEventMessage
                                          {
                                              Name = application.Name,
                                              CommunicatorCount = application.ConnectedCommunicatorCount
                                          });
        }

        #endregion

        #region Other private methods

        /// <summary>
        /// Sends a ControlMessage to all connected NGRIDController instances.
        /// </summary>
        /// <param name="message">Message to send</param>
        private void SendMessageToAllReceivers(ControlMessage message)
        {
            var outgoingMessage = new NGRIDControllerMessage
            {
                ControllerMessageTypeId = message.MessageTypeId,
                MessageData = NGRIDSerializationHelper.SerializeToByteArray(message)
            };

            var receivers = GetAllReceiverCommunicators();
            foreach (var receiver in receivers)
            {
                try
                {
                    SendMessage(outgoingMessage, receiver);
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex.Message, ex);
                }
            }
        }

        /// <summary>
        /// Sends a message to a spesific communicator as a reply to an incoming message.
        /// </summary>
        /// <param name="communicator">Communicator to send message</param>
        /// <param name="message">Message to send</param>
        /// <param name="incomingMessage">Incoming message which is being replied</param>
        private void ReplyMessageToCommunicator(ICommunicator communicator, ControlMessage message, NGRIDControllerMessage incomingMessage)
        {
            //Create NGRIDControllerMessage that includes serialized GetApplicationListResponseMessage message
            var outgoingMessage = new NGRIDControllerMessage
            {
                ControllerMessageTypeId = message.MessageTypeId,
                MessageData = NGRIDSerializationHelper.SerializeToByteArray(message),
                RepliedMessageId = incomingMessage.MessageId
            };
            //Send message to communicator that sent to message
            SendMessage(outgoingMessage, communicator);
        }

        #endregion

        #endregion
    }
}
