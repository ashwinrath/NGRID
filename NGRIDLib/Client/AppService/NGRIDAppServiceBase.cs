namespace NGRID.Client.AppService
{
    /// <summary>
    /// Base class for NGRIDMessageProcessor/NGRIDClientApplicationBase.
    /// </summary>
    public abstract class NGRIDAppServiceBase
    {
        /// <summary>
        /// Reference to the NGRID Server.
        /// </summary>
        public INGRIDServer Server { get; set; }

        /// <summary>
        /// Reference to this Application.
        /// </summary>
        public INGRIDApplication Application { get; set; }
    }
}
