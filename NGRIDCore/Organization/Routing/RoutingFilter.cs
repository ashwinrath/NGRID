using NGRID.Communication.Messages;

namespace NGRID.Organization.Routing
{
    /// <summary>
    /// Represents a filter of a routing.
    /// </summary>
    public class RoutingFilter
    {
        /// <summary>
        /// Source server name for the filter. Must be one of following values:
        /// Empty string or null: No filter on this property.
        /// this: For this/current server.
        /// A server name: For a specified server name.
        /// </summary>
        public string SourceServer { get; set; }

        /// <summary>
        /// Source application name for the filter. Must be one of following values:
        /// Empty string or null: No filter on this property.
        /// An application name: For a specified application name.
        /// </summary>
        public string SourceApplication { get; set; }

        /// <summary>
        /// Destination server name for the filter. Must be one of following values:
        /// Empty string or null: No filter on this property.
        /// this: For this/current server.
        /// A server name: For a specified server name.
        /// </summary>
        public string DestinationServer { get; set; }

        /// <summary>
        /// Destination application name for the filter. Must be one of following values:
        /// Empty string or null: No filter on this property.
        /// An application name: For a specified application name.
        /// </summary>
        public string DestinationApplication { get; set; }

        /// <summary>
        /// Transmit rule for the filter. Must be one of following values:
        /// Empty string or null: No filter on this property.
        /// An element of MessageTransmitRules enum.
        /// </summary>
        public string TransmitRule { get; set; }

        /// <summary>
        /// Creates a new RoutingFilter object.
        /// </summary>
        public RoutingFilter()
        {
            SourceServer = "";
            SourceApplication = "";
            DestinationServer = "";
            DestinationApplication = "";
            TransmitRule = "";
        }

        /// <summary>
        /// Checks if a message matches with this filter.
        /// </summary>
        /// <param name="message">Message to check</param>
        /// <returns>True, if message matches with this rule</returns>
        public bool Matches(NGRIDDataTransferMessage message)
        {
            if ((string.IsNullOrEmpty(SourceServer) || SourceServer == message.SourceServerName) &&
                (string.IsNullOrEmpty(SourceApplication) || SourceApplication == message.SourceApplicationName) &&
                (string.IsNullOrEmpty(DestinationServer) || DestinationServer == message.DestinationServerName) &&
                (string.IsNullOrEmpty(DestinationApplication) || DestinationApplication == message.DestinationApplicationName) &&
                (string.IsNullOrEmpty(TransmitRule) || TransmitRule == message.TransmitRule.ToString()))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns a string that presents a brief information about RoutingFilter object.
        /// </summary>
        /// <returns>Brief information about RoutingFilter object</returns>
        public override string ToString()
        {
            return string.Format("SourceServer: {0}, SourceApplication: {1}, DestinationServer: {2}, DestinationApplication: {3}, TransmitRule: {4}", SourceServer, SourceApplication, DestinationServer, DestinationApplication, TransmitRule);
        }
    }
}
