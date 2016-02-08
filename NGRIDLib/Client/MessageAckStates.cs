namespace NGRID.Client
{
    /// <summary>
    /// Respesents states of an incoming message.
    /// </summary>
    public enum MessageAckStates
    {
        /// <summary>
        /// Message is waiting for Ack.
        /// </summary>
        WaitingForAck,

        /// <summary>
        /// Message is acknowledged.
        /// </summary>
        Acknowledged,

        /// <summary>
        /// Message is rejected.
        /// </summary>
        Rejected
    }
}
