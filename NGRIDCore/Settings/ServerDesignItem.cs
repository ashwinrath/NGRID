namespace NGRID.Settings
{
    /// <summary>
    /// Represents a Server's design informations in design settings.
    /// </summary>
    public class ServerDesignItem
    {
        /// <summary>
        /// Name of this server.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Location of server (Left (X) and Top (Y) properties in design area, seperated by comma (,)).
        /// </summary>
        public string Location { get; set; }
    }
}
