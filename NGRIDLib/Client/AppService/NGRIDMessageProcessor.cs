namespace NGRID.Client.AppService
{
    /// <summary> 
    /// This class is used as base class for classes that are processing messages of an application concurrently. Thus,
    /// an application can process more than one message in a time.
    /// NGRID creates an instance of this class for every incoming message to process it.
    /// Maximum limit of messages that are being processed at the same time is configurable for individual applications.
    /// </summary>
    public abstract class NGRIDMessageProcessor : NGRIDAppServiceBase
    {
        /// <summary>
        /// Used to get/set if messages are auto acknowledged.
        /// If AutoAcknowledgeMessages is true, then messages are automatically acknowledged after MessageReceived event,
        /// if they are not acknowledged/rejected before by application.
        /// Default: true.
        /// </summary>
        protected bool AutoAcknowledgeMessages
        {
            get { return _autoAcknowledgeMessages; }
            set { _autoAcknowledgeMessages = value; }
        }
        private bool _autoAcknowledgeMessages = true;

        /// <summary>
        /// This method is called by NGRID server to process the message, when a message is sent to this application.
        /// </summary>
        /// <param name="message">Message to process</param>
        public abstract void ProcessMessage(IIncomingMessage message);
    }
}
