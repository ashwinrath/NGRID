using NGRID.Communication.Messages;

namespace NGRID.Organization.Routing
{
    /// <summary>
    /// Interface of a distribution strategy.
    /// A distribution strategy is a way of redirecting a message to one of available destinations.
    /// </summary>
    internal interface IDistributionStrategy
    {
        /// <summary>
        /// Initializes and Resets distribution strategy.
        /// </summary>
        void Reset();

        /// <summary>
        /// Sets the destination of a message according to distribution strategy.
        /// </summary>
        /// <param name="message">Message to set it's destination</param>
        void SetDestination(NGRIDDataTransferMessage message);
    }
}
