using System;
using System.Reflection;
using System.ServiceProcess;
using log4net;
using NGRID;

namespace NGRID
{
    /// <summary>
    /// This service is used to run NGRID as a Windows Service.
    /// </summary>
    public partial class NGRIDService : ServiceBase
    {
        /// <summary>
        /// Reference to logger.
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Reference to NGRID server instance.
        /// </summary>
        private readonly NGRIDServer _ngridServer;

        /// <summary>
        /// Constructor.
        /// </summary>
        public NGRIDService()
        {
            InitializeComponent();
            _ngridServer = new NGRIDServer();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                _ngridServer.Start();
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                Stop();
            }
        }

        protected override void OnStop()
        {
            try
            {
                _ngridServer.Stop(true);
            }
            catch (Exception ex)
            {
                Logger.Warn(ex.Message, ex);
            }
        }
    }
}
