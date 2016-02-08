namespace NGRID.Client.AppService
{
    /// <summary>
    /// This interface is used by NGRIDMessageProcessor/NGRIDClientApplicationBase to perform operations on NGRIDServer,
    /// for example; creating messages to send.
    /// </summary>
    public interface INGRIDServer
    {
        /// <summary>
        /// Creates an empty message to send.
        /// </summary>
        /// <returns>Created message</returns>
        IOutgoingMessage CreateMessage();
    }
}
