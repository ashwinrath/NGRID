using NGRID.Communication.Messages;

namespace NGRID.Organization.Routing
{
    /// <summary>
    /// Sequential distribution strategy.
    /// According to this strategy, a message is routed one of available destinations sequentially according to destination's RouteFactor.
    /// For example, if,
    /// 
    /// - Destination-A has a RouteFactor of 4
    /// - Destination-B has a RouteFactor of 1
    /// 
    /// Then, 4 messages are sent to Destination-A, 1 message is sent to Destination-B sequentially.
    /// </summary>
    internal class SequentialDistribution : DistributionStrategyBase, IDistributionStrategy
    {
        /// <summary>
        /// Total count of all RouteFactors of Destinations and calculated by Reset method.
        /// </summary>
        private int _totalCount;

        /// <summary>
        /// Current routing number. It is used to determine the next routing destination.
        /// For example, if,
        /// 
        /// - Destination-A has a RouteFactor of 4
        /// - Destination-B has a RouteFactor of 3
        /// - Destination-C has a RouteFactor of 3
        /// 
        /// and _currentNumber is 5, then destination is Destination-B. 
        /// </summary>
        private int _currentNumber;

        /// <summary>
        /// Creates a new SequentialDistribution object.
        /// </summary>
        /// <param name="routingRule">Reference to RoutingRule object that uses this distribution strategy to route messages</param>
        public SequentialDistribution(RoutingRule routingRule)
            : base(routingRule)
        {
            Reset();
        }

        /// <summary>
        /// Initializes and Resets distribution strategy.
        /// </summary>
        public void Reset()
        {
            _totalCount = 0;
            foreach (var destination in RoutingRule.Destinations)
            {
                _totalCount += destination.RouteFactor;
            }
        }

        /// <summary>
        /// Sets the destination of a message according to distribution strategy.
        /// </summary>
        /// <param name="message">Message to set it's destination</param>
        public void SetDestination(NGRIDDataTransferMessage message)
        {
            //Return, if no destination exists
            if (_totalCount == 0 || RoutingRule.Destinations.Length <= 0)
            {
                return;
            }

            //Find next destination and set
            var currentTotal = 0;
            foreach (var destination in RoutingRule.Destinations)
            {
                currentTotal += destination.RouteFactor;
                if (_currentNumber < currentTotal)
                {
                    SetMessageDestination(message, destination);
                    break;
                }
            }

            //Increase _currentNumber
            if ((++_currentNumber) >= _totalCount)
            {
                _currentNumber = 0;
            }
        }
    }
}
