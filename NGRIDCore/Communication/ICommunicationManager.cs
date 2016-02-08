using NGRID.Communication.Events;
using NGRID.Threading;

namespace NGRID.Communication
{
    /// <summary>
    /// This interface is implemented by communcation managers.
    /// </summary>
    public interface ICommunicationManager : IRunnable
    {
        /// <summary>
        /// This event is raised when a communicator is connected succesfully.
        /// </summary>
        event CommunicatorConnectedHandler CommunicatorConnected;
    }
}
