namespace NGRID.Communication
{
    /// <summary>
    /// Represents states of a communication object.
    /// </summary>
    public enum CommunicationStates
    {
        /// <summary>
        /// Connecting now..
        /// </summary>
        Connecting,

        /// <summary>
        /// Connection is established, communication can be made.
        /// </summary>
        Connected,

        /// <summary>
        /// Closing connection..
        /// </summary>
        Closing,

        /// <summary>
        /// Connection is closed, so communication can not be made.
        /// </summary>
        Closed
    }
}
