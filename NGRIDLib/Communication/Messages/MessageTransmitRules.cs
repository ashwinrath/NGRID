namespace NGRID.Communication.Messages
{
    /// <summary>
    /// Message transmit rules.
    /// All messages are persistent except 'DirectlySend'.
    /// If a server doesn't stores message and transmiting it directly,
    /// it transmits this message before than a stored (persistent) message.
    /// </summary>
    public enum MessageTransmitRules : byte
    {
        /// <summary>
        /// Not persistent message.
        /// Message may be lost in an error.
        /// Message is not stored on any server. 
        /// Message is not guarantied to be delivered.
        /// This rule may be used if both of source and destination applications must be run at the same time.
        /// If no exception received while sending message,
        /// that means message delivered to and acknowledged by destination application correctly.
        /// This rule blocks sender application until destination application sends ACK for message.
        /// </summary>
        DirectlySend = 0,
        
        /// <summary>
        /// Persistent Message.
        /// Message can not be lost and it is being stored in all passing servers.
        /// Message is guarantied to be delivered and it will be delivered as ordered (FIFO).
        /// This is the slowest but most reliable rule.
        /// This rule blocks sender application until source (first) NGRID server stores message.
        /// </summary>
        StoreAndForward,

        /// <summary>
        /// Non-persistent message.
        /// Message will be lost if NGRID server which has message shuts down.
        /// Message is not guarantied to be delivered.
        /// </summary>
        NonPersistent
    }
}
