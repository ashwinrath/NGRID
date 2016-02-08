namespace NGRID.Client.WebServices
{
    /// <summary>
    /// Represents an outgoing data message from a NGRID web service to NGRID server.
    /// </summary>
    public interface IWebServiceOutgoingMessage
    {
        #region Properties

        /// <summary>
        /// Essential application message data to be sent.
        /// </summary>
        byte[] MessageData { get; set; }

        #endregion
    }
}
