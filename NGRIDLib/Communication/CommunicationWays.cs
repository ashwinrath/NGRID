namespace NGRID.Communication
{
    /// <summary>
    /// Communication ways.
    /// A client application may just send messages from communication channel or it can send and receive messages.
    /// </summary>
    public enum CommunicationWays: byte
    {
        /// <summary>
        /// Application can only send messages to NGRID server.
        /// </summary>
        Send = 1,

        /// <summary>
        /// Application can send and receive messages to/from NGRID server.
        /// </summary>
        SendAndReceive = 2,
    }
}
