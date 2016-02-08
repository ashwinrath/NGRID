namespace NGRID.Settings
{
    /// <summary>
    /// Represents a Server's informations in settings.
    /// </summary>
    public class ServerInfoItem
    {
        /// <summary>
        /// Name of this server.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// IP address of this server.
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// TCP Port number that is listened by this server.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// List of adjacent servers of this server that are splitted by , or ;
        /// </summary>
        public string Adjacents { get; set; }
    }
}
