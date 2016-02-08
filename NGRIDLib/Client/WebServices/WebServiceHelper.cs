using System;
using NGRID.Communication.Messages;
using NGRID.Serialization;

namespace NGRID.Client.WebServices
{
    /// <summary>
    /// This class is used by NGRID Web Services to serialize/deserialize/create messages.
    /// </summary>
    public static class WebServiceHelper
    {
        #region Public methods

        /// <summary>
        /// Deserializes an incoming message for Web Service from NGRID server.
        /// </summary>
        /// <param name="bytesOfMessage">Message as byte array</param>
        /// <returns>Deserialized message</returns>
        public static IWebServiceIncomingMessage DeserializeMessage(byte[] bytesOfMessage)
        {
            var dataMessage = NGRIDSerializationHelper.DeserializeFromByteArray(() => new NGRIDDataTransferMessage(), bytesOfMessage);
            return new IncomingDataMessage(dataMessage);
        }

        /// <summary>
        /// Serializes a message to send to NGRID server from Web Service.
        /// </summary>
        /// <param name="responseMessage">Message to serialize</param>
        /// <returns>Serialized message</returns>
        public static byte[] SerializeMessage(IWebServiceResponseMessage responseMessage)
        {
            CheckResponseMessage(responseMessage);
            var response = ((ResponseMessage) responseMessage).CreateDataTransferResponseMessage();
            return NGRIDSerializationHelper.SerializeToByteArray(response);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Checks a response message whether it is a valid response message
        /// </summary>
        /// <param name="responseMessage">Message to check</param>
        private static void CheckResponseMessage(IWebServiceResponseMessage responseMessage)
        {
            if (responseMessage == null)
            {
                throw new ArgumentNullException("responseMessage", "responseMessage can not be null.");
            }

            if (!(responseMessage is ResponseMessage))
            {
                throw new Exception("responseMessage parameter is not known type.");
            }

            if (responseMessage.Result == null)
            {
                throw new ArgumentNullException("responseMessage", "responseMessage.Result can not be null.");
            }

            if( !(responseMessage.Result is ResultMessage))
            {
                throw new Exception("responseMessage.Result is not known type.");
            }

            if(responseMessage.Message != null && !(responseMessage.Message is OutgoingDataMessage))
            {
                throw new Exception("responseMessage.Message is not known type.");
            }
        }

        #endregion

        #region Sub classes

        /// <summary>
        /// Implements IWebServiceIncomingMessage to be used by NGRID web service.
        /// </summary>
        private class IncomingDataMessage : NGRIDDataTransferMessage, IWebServiceIncomingMessage
        {
            /// <summary>
            /// Creates a new IncomingDataMessage object from a NGRIDDataTransferMessage object.
            /// </summary>
            /// <param name="message">NGRIDDataTransferMessage object to create IncomingDataMessage</param>
            public IncomingDataMessage(NGRIDDataTransferMessage message)
            {
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
            /// Creates IWebServiceResponseMessage using this incoming message, to return from web service to NGRID server.
            /// </summary>
            /// <returns>Response message to this message</returns>
            public IWebServiceResponseMessage CreateResponseMessage()
            {
                return new ResponseMessage { Result = new ResultMessage { RepliedMessageId = MessageId } };
            }

            /// <summary>
            /// Creates IWebServiceOutgoingMessage using this incoming message, to return from web service to NGRID server.
            /// </summary>
            /// <returns>Response message to this message</returns>
            public IWebServiceOutgoingMessage CreateResponseDataMessage()
            {
                return new OutgoingDataMessage
                {
                    DestinationApplicationName = SourceApplicationName,
                    DestinationCommunicatorId = SourceCommunicatorId,
                    DestinationServerName = SourceServerName,
                    RepliedMessageId = MessageId,
                    TransmitRule = TransmitRule
                };
            }
        }

        /// <summary>
        /// Implements IWebServiceOutgoingMessage to be used by NGRID web service.
        /// </summary>
        private class OutgoingDataMessage : NGRIDDataTransferMessage, IWebServiceOutgoingMessage
        {
            //No data or method
        }
        
        /// <summary>
        /// Implements IWebServiceResponseMessage to be used by NGRID web service.
        /// </summary>
        private class ResponseMessage : IWebServiceResponseMessage
        {
            public IWebServiceOperationResultMessage Result { get; set; }

            public IWebServiceOutgoingMessage Message { get; set; }

            public NGRIDDataTransferResponseMessage CreateDataTransferResponseMessage()
            {
                return new NGRIDDataTransferResponseMessage
                       {
                           Message = (NGRIDDataTransferMessage) Message,
                           Result = (NGRIDOperationResultMessage) Result
                       };
            }
        }

        /// <summary>
        /// Implements IWebServiceOperationResultMessage to be used by NGRID web service.
        /// </summary>
        private class ResultMessage : NGRIDOperationResultMessage, IWebServiceOperationResultMessage
        {
            //No data or method            
        }

        #endregion
    }
}
