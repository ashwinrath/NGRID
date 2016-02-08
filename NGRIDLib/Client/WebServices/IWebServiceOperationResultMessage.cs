namespace NGRID.Client.WebServices
{
    /// <summary>
    /// Represents a result message for an incoming message to NGRID Web Service.
    /// </summary>
    public interface IWebServiceOperationResultMessage
    {
        /// <summary>
        /// Operation result.
        /// True, if operation is successful.
        /// </summary>
        bool Success { get; set; }

        /// <summary>
        /// A text that may be used as a description for result of operation.
        /// </summary>
        string ResultText { get; set; }
    }
}
