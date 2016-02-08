using NGRID.Communication.Events;

namespace NGRID.Communication
{
    /// <summary>
    /// Base class for communicator managers.
    /// </summary>
    public abstract class CommunicationManagerBase : ICommunicationManager
    {
        /// <summary>
        /// This event is raised when a communicator connection established.
        /// </summary>
        public event CommunicatorConnectedHandler CommunicatorConnected;

        /// <summary>
        /// Starts communication manager.
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// Stops communication manager.
        /// </summary>
        /// <param name="waitToStop">Indicates that caller thread waits stopping of manager</param>
        public abstract void Stop(bool waitToStop);

        /// <summary>
        /// Waits stopping of communication manager.
        /// </summary>
        public abstract void WaitToStop();

        /// <summary>
        /// Raises CommunicatorConnected event.
        /// </summary>
        /// <param name="communicator">Communicator</param>
        protected void OnCommunicatorConnected(ICommunicator communicator)
        {
            if (CommunicatorConnected != null)
            {
                CommunicatorConnected(this, new CommunicatorConnectedEventArgs {Communicator = communicator});
            }
        }
    }
}
