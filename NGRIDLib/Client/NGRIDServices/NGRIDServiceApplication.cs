using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using NGRID.Communication;
using NGRID.Communication.Messages;
using NGRID.Exceptions;

namespace NGRID.Client.NGRIDServices
{
    /// <summary>
    /// This class ensures to use a class that is derived from NGRIDService class, as a service on NGRID.
    /// </summary>
    public class NGRIDServiceApplication : IDisposable
    {
        #region Private fields

        /// <summary>
        /// Underlying NGRIDClient object to send/receive NGRID messages.
        /// </summary>
        private readonly NGRIDClient _ngridClient;

        /// <summary>
        /// Reference to logger.
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The service object that handles all method invokes.
        /// </summary>
        private SortedList<string, ServiceObject> _serviceObjects;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new NGRIDServiceApplication object with default values to connect to NGRID server.
        /// </summary>
        /// <param name="applicationName">Name of the application</param>
        public NGRIDServiceApplication(string applicationName)
        {
            _ngridClient = new NGRIDClient(applicationName, CommunicationConsts.DefaultIpAddress, CommunicationConsts.DefaultNGRIDPort);
            Initialize();
        }

        /// <summary>
        /// Creates a new NGRIDServiceApplication object with default port to connect to NGRID server.
        /// </summary>
        /// <param name="applicationName">Name of the application</param>
        /// <param name="ipAddress">IP address of NGRID server</param>
        public NGRIDServiceApplication(string applicationName, string ipAddress)
        {
            _ngridClient = new NGRIDClient(applicationName, ipAddress, CommunicationConsts.DefaultNGRIDPort);
            Initialize();
        }

        /// <summary>
        /// Creates a new NGRIDServiceApplication object.
        /// </summary>
        /// <param name="applicationName">Name of the application</param>
        /// <param name="ipAddress">IP address of NGRID server</param>
        /// <param name="port">TCP port of NGRID server</param>
        public NGRIDServiceApplication(string applicationName, string ipAddress, int port) 
        {
            _ngridClient = new NGRIDClient(applicationName, ipAddress, port);
            Initialize();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// This method connects to NGRID server using underlying NGRIDClient object.
        /// </summary>
        public void Connect()
        {
            _ngridClient.Connect();
        }

        /// <summary>
        /// This method disconnects from NGRID server using underlying NGRIDClient object.
        /// </summary>
        public void Disconnect()
        {
            _ngridClient.Disconnect();
        }
        
        /// <summary>
        /// Adds a new NGRIDService for this service application.
        /// </summary>
        /// <param name="service">Service to add</param>
        public void AddService(NGRIDService service)
        {
            if (service == null)
            {
                throw new ArgumentNullException("service");
            }

            var type = service.GetType();
            var attributes = type.GetCustomAttributes(typeof (NGRIDServiceAttribute), true);
            if(attributes.Length <= 0)
            {
                throw new NGRIDException("Service class must has NGRIDService attribute to be added.");
            }

            if (_serviceObjects.ContainsKey(type.Name))
            {
                throw new NGRIDException("Service '" + type.Name + "' is already added.");
            }

            _serviceObjects.Add(type.Name, new ServiceObject(service, (NGRIDServiceAttribute)attributes[0]));
        }

        /// <summary>
        /// Removes a NGRIDService from this service application.
        /// </summary>
        /// <param name="service">Service to add</param>
        public void RemoveService(NGRIDService service)
        {
            if (service == null)
            {
                throw new ArgumentNullException("service");
            }

            var type = service.GetType();
            if (!_serviceObjects.ContainsKey(type.Name))
            {
                return;
            }

            _serviceObjects.Remove(type.Name);
        }

        /// <summary>
        /// Disposes this object, disposes/closes underlying NGRIDClient object.
        /// </summary>
        public void Dispose()
        {
            _ngridClient.Dispose();
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Initializes this object.
        /// </summary>
        private void Initialize()
        {
            _serviceObjects = new SortedList<string, ServiceObject>();
            _ngridClient.MessageReceived += NgridClient_MessageReceived;
        }
        
        /// <summary>
        /// This method handles all incoming messages from NGRID server.
        /// </summary>
        /// <param name="sender">Creator object of method (NGRIDClient object)</param>
        /// <param name="e">Message event arguments</param>
        private void NgridClient_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            //Deserialize message
            NGRIDRemoteInvokeMessage invokeMessage;
            try
            {
                invokeMessage = (NGRIDRemoteInvokeMessage)GeneralHelper.DeserializeObject(e.Message.MessageData);
            }
            catch (Exception ex)
            {
                AcknowledgeMessage(e.Message);
                SendException(e.Message, new NGRIDRemoteException("Incoming message can not be deserialized to NGRIDRemoteInvokeMessage.", ex));
                return;
            }
            
            //Check service class name
            if (!_serviceObjects.ContainsKey(invokeMessage.ServiceClassName))
            {
                AcknowledgeMessage(e.Message);
                SendException(e.Message, new NGRIDRemoteException("There is no service with name '" + invokeMessage.ServiceClassName + "'"));
                return;
            }

            //Get service object
            var serviceObject = _serviceObjects[invokeMessage.ServiceClassName];
            
            //Invoke service method and get return value
            object returnValue;
            try
            {
                //Set request variables to service object and invoke method
                serviceObject.Service.IncomingMessage = e.Message;
                returnValue = serviceObject.InvokeMethod(invokeMessage.MethodName, invokeMessage.Parameters);
            }
            catch (Exception ex)
            {
                SendException(e.Message,
                              new NGRIDRemoteException(
                                  ex.Message + Environment.NewLine + "Service Class: " +
                                  invokeMessage.ServiceClassName + " " + Environment.NewLine +
                                  "Service Version: " + serviceObject.ServiceAttribute.Version, ex));
                return;
            }

            //Send return value to sender application
            SendReturnValue(e.Message, returnValue);
        }


        /// <summary>
        /// Sends an Exception to the remote application that invoked a service method
        /// </summary>
        /// <param name="incomingMessage">Incoming invoke message from remote application</param>
        /// <param name="exception">Exception to send</param>
        private static void SendException(IIncomingMessage incomingMessage, NGRIDRemoteException exception)
        {
            if (incomingMessage.TransmitRule != MessageTransmitRules.DirectlySend)
            {
                return;
            }

            try
            {
                //Create return message
                var returnMessage = new NGRIDRemoteInvokeReturnMessage { RemoteException = exception };
                //Create response message and send
                var responseMessage = incomingMessage.CreateResponseMessage();
                responseMessage.MessageData = GeneralHelper.SerializeObject(returnMessage);
                responseMessage.Send();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }
        }

        /// <summary>
        /// Sends return value to the remote application that invoked a service method.
        /// </summary>
        /// <param name="incomingMessage">Incoming invoke message from remote application</param>
        /// <param name="returnValue">Return value to send</param>
        private static void SendReturnValue(IIncomingMessage incomingMessage, object returnValue)
        {
            if (incomingMessage.TransmitRule != MessageTransmitRules.DirectlySend)
            {
                return;
            }

            try
            {
                //Create return message
                var returnMessage = new NGRIDRemoteInvokeReturnMessage { ReturnValue = returnValue };
                //Create response message and send
                var responseMessage = incomingMessage.CreateResponseMessage();
                responseMessage.MessageData = GeneralHelper.SerializeObject(returnMessage);
                responseMessage.Send();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }
        }

        /// <summary>
        /// Acknowledges a message.
        /// </summary>
        /// <param name="message">Message to acknowledge</param>
        private static void AcknowledgeMessage(IIncomingMessage message)
        {
            try
            {
                message.Acknowledge();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }
        }

        #endregion

        #region Sub classes

        /// <summary>
        /// Represents a NGRIDService object.
        /// </summary>
        private class ServiceObject
        {
            /// <summary>
            /// The service object that is used to invoke methods on.
            /// </summary>
            public NGRIDService Service { get; private set; }

            /// <summary>
            /// NGRIDService attribute of Service object's class.
            /// </summary>
            public NGRIDServiceAttribute ServiceAttribute { get; private set; }

            /// <summary>
            /// Name of the Service object's class.
            /// </summary>
            private readonly string _serviceClassName;
            
            /// <summary>
            /// This collection stores a list of all methods of T, if that can be invoked from remote applications or not.
            /// Key: Method name
            /// Value: True, if it can be invoked from remote application. 
            /// </summary>
            private readonly SortedList<string, bool> _methods;
            
            /// <summary>
            /// Creates a new ServiceObject.
            /// </summary>
            /// <param name="service">The service object that is used to invoke methods on</param>
            /// <param name="serviceAttribute">NGRIDService attribute of service object's class</param>
            public ServiceObject(NGRIDService service, NGRIDServiceAttribute serviceAttribute)
            {
                Service = service;
                ServiceAttribute = serviceAttribute;

                _serviceClassName = service.GetType().Name;

                //Find all methods
                _methods = new SortedList<string, bool>();
                foreach (var methodInfo in Service.GetType().GetMethods())
                {
                    var attributes = methodInfo.GetCustomAttributes(typeof(NGRIDServiceMethodAttribute), true);
                    _methods.Add(methodInfo.Name, attributes.Length > 0);
                }
            }

            /// <summary>
            /// Invokes a method of Service object.
            /// </summary>
            /// <param name="methodName">Name of the method to invoke</param>
            /// <param name="parameters">Parameters of method</param>
            /// <returns>Return value of method</returns>
            public object InvokeMethod(string methodName, params object[] parameters)
            {
                //Check if there is a method with name methodName
                if (!_methods.ContainsKey(methodName))
                {
                    AcknowledgeMessage(Service.IncomingMessage);
                    throw new MethodAccessException("There is not a method with name '" + methodName + "' in service '" + _serviceClassName + "'.");
                }

                //Check if method has NGRIDServiceMethod attribute
                if (!_methods[methodName])
                {
                    AcknowledgeMessage(Service.IncomingMessage);
                    throw new MethodAccessException("Method '" + methodName + "' has not NGRIDServiceMethod attribute. It can not be invoked from remote applications.");
                }

                //Set object properties before method invocation
                Service.RemoteApplication =
                    new NGRIDRemoteAppEndPoint(Service.IncomingMessage.SourceServerName,
                                             Service.IncomingMessage.SourceApplicationName,
                                             Service.IncomingMessage.SourceCommunicatorId);

                //If Transmit rule is DirectlySend than message must be acknowledged now.
                //If any exception occurs on method invocation, exception will be sent to
                //remote application in a NGRIDRemoteInvokeReturnMessage.
                if (Service.IncomingMessage.TransmitRule == MessageTransmitRules.DirectlySend)
                {
                    AcknowledgeMessage(Service.IncomingMessage);
                }

                //Invoke method and get return value
                try
                {
                    var returnValue = Service.GetType().GetMethod(methodName).Invoke(Service, parameters);
                    if (Service.IncomingMessage.TransmitRule != MessageTransmitRules.DirectlySend)
                    {
                        AcknowledgeMessage(Service.IncomingMessage);
                    }

                    return returnValue;
                }
                catch (Exception ex)
                {
                    if (Service.IncomingMessage.TransmitRule != MessageTransmitRules.DirectlySend)
                    {
                        RejectMessage(Service.IncomingMessage, ex.Message);
                    }

                    if (ex.InnerException != null)
                    {
                        throw ex.InnerException;
                    }

                    throw;
                }
            }

            /// <summary>
            /// Rejects a message.
            /// </summary>
            /// <param name="message">Message to reject</param>
            /// <param name="reason">Reject reason</param>
            private static void RejectMessage(IIncomingMessage message, string reason)
            {
                try
                {
                    message.Reject(reason);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.Message, ex);
                }
            }
        }

        #endregion
    }
}
