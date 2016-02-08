namespace NGRID.Organization.Routing
{
    /// <summary>
    /// Represents a Destination of a Routing.
    /// </summary>
    public class RoutingDestination
    {
        /// <summary>
        /// Destination server name. Must be one of following values:
        /// Empty string or null: Don't change destination server.
        /// this: Change to this/current server.
        /// A server name: Change to a specified server name.
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// Destination application name. Must be one of following values:
        /// Empty string or null: Don't change destination application.
        /// A application name: Change to a specified application name.
        /// </summary>
        public string Application { get; set; }

        /// <summary>
        /// Route factor.
        /// Must be 1 or greater.
        /// </summary>
        public int RouteFactor { get; set; }

        /// <summary>
        /// Creates a new RoutingDestination object.
        /// </summary>
        public RoutingDestination()
        {
            Server = "";
            Application = "";
            RouteFactor = 1;
        }
        
        /// <summary>
        /// Returns a string that presents a brief information about RoutingFilter object.
        /// </summary>
        /// <returns>Brief information about RoutingFilter object</returns>
        public override string ToString()
        {
            return string.Format("Server: {0}, Application: {1}, RouteFactor: {2}", Server, Application, RouteFactor);
        }
    }
}
