namespace NGRID.Client.WebServices
{
    /// <summary>
    /// Represents a response message to a IWebServiceIncomingMessage from a NGRID web service.
    /// </summary>
    public interface IWebServiceResponseMessage
    {
        /// <summary>
        /// Process result of IWebServiceIncomingMessage.
        /// </summary>
        IWebServiceOperationResultMessage Result { get; set; }

        /// <summary>
        /// Response message to IWebServiceIncomingMessage.
        /// This may be null to do not send a response to incoming message.
        /// </summary>
        IWebServiceOutgoingMessage Message { get; set; }
    }
}
