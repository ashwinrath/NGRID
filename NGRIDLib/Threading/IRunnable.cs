namespace NGRID.Threading
{
    /// <summary>
    /// This interface is used for a class that can startable and stoppable.
    /// </summary>
    public interface IRunnable
    {
        /// <summary>
        /// This method is used to start running.
        /// </summary>
        void Start();

        /// <summary>
        /// This method is used to stop running.
        /// </summary>
        /// <param name="waitToStop">Indicates that caller thread waits stopping of module</param>
        void Stop(bool waitToStop);

        /// <summary>
        /// Joins module's thread until it stops.
        /// </summary>
        void WaitToStop();
    }
}
