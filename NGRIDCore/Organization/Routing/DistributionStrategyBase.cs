using System;
using NGRID.Communication.Messages;
using NGRID.Settings;

namespace NGRID.Organization.Routing
{
    /// <summary>
    /// Base class for distribution strategies.
    /// </summary>
    internal abstract class DistributionStrategyBase
    {
        /// <summary>
        /// Reference to RoutingRule object that uses this distribution strategy to route messages.
        /// </summary>
        protected readonly RoutingRule RoutingRule;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="routingRule">Reference to RoutingRule object that uses this distribution strategy to route messages</param>
        protected DistributionStrategyBase(RoutingRule routingRule)
        {
            RoutingRule = routingRule;
        }

        /// <summary>
        /// Sets the destination of a message.
        /// </summary>
        /// <param name="message">Message to set it's destination</param>
        /// <param name="destination">Destination to set to message</param>
        protected static void SetMessageDestination(NGRIDDataTransferMessage message, RoutingDestination destination)
        {
            //Sets destination server
            if (!string.IsNullOrEmpty(destination.Server))
            {
                message.DestinationServerName = destination.Server.Equals("this", StringComparison.OrdinalIgnoreCase)
                                                    ? NGRIDSettings.Instance.ThisServerName
                                                    : destination.Server;
            }

            //Sets destination application
            if (!string.IsNullOrEmpty(destination.Application))
            {
                message.DestinationApplicationName = destination.Application;
            }
        }
    }
}
