namespace NGRID.Communication
{
    /// <summary>
    /// This class stores some consts used in NGRID.
    /// </summary>
    public sealed class CommunicationConsts
    {
        /// <summary>
        /// Default IP address to connect to NGRID server.
        /// </summary>
        public const string DefaultIpAddress = "127.0.0.1";

        /// <summary>
        /// Default listening port of NGRID server.
        /// </summary>
        public const int DefaultNGRIDPort = 10905;

        /// <summary>
        /// Maximum message data length.
        /// </summary>
        public const uint MaxMessageSize = 52428800; //50 MegaBytes
    }
}
