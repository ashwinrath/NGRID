namespace NGRID.Client.AppService
{
    /// <summary>
    /// Plug-In applications may derive this class to perform some operations on starting/stopping of NGRID.
    /// </summary>
    public abstract class NGRIDClientApplicationBase : NGRIDAppServiceBase
    {
        /// <summary>
        /// This method is called on startup of NGRID.
        /// </summary>
        public virtual void OnStart()
        {
            //No action
        }

        /// <summary>
        /// This method is called on stopping of NGRID.
        /// </summary>
        public virtual void OnStop()
        {
            //No action            
        }
    }
}
