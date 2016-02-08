using System;
using NGRID.Communication;

namespace NGRID.Client.NGRIDServices
{
    public class NGRIDServiceConsumer : IDisposable
    {
        #region Public fields

        /// <summary>
        /// Underlying NGRIDClient object to send/receive NGRID messages.
        /// </summary>
        internal NGRIDClient NgridClient { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new NGRIDServiceApplication object with default values to connect to NGRID server.
        /// </summary>
        /// <param name="applicationName">Name of the application</param>
        public NGRIDServiceConsumer(string applicationName)
        {
            NgridClient = new NGRIDClient(applicationName, CommunicationConsts.DefaultIpAddress, CommunicationConsts.DefaultNGRIDPort);
            Initialize();
        }

        /// <summary>
        /// Creates a new NGRIDServiceApplication object with default port to connect to NGRID server.
        /// </summary>
        /// <param name="applicationName">Name of the application</param>
        /// <param name="ipAddress">IP address of NGRID server</param>
        public NGRIDServiceConsumer(string applicationName, string ipAddress)
        {
            NgridClient = new NGRIDClient(applicationName, ipAddress, CommunicationConsts.DefaultNGRIDPort);
        }

        /// <summary>
        /// Creates a new NGRIDServiceApplication object.
        /// </summary>
        /// <param name="applicationName">Name of the application</param>
        /// <param name="ipAddress">IP address of NGRID server</param>
        /// <param name="port">TCP port of NGRID server</param>
        public NGRIDServiceConsumer(string applicationName, string ipAddress, int port) 
        {
            NgridClient = new NGRIDClient(applicationName, ipAddress, port);
        }

        #endregion

        #region Public methods

        /// <summary>
        /// This method connects to NGRID server using underlying NGRIDClient object.
        /// </summary>
        public void Connect()
        {
            NgridClient.Connect();
        }

        /// <summary>
        /// This method disconnects from NGRID server using underlying NGRIDClient object.
        /// </summary>
        public void Disconnect()
        {
            NgridClient.Disconnect();
        }

        /// <summary>
        /// Disposes this object, disposes/closes underlying NGRIDClient object.
        /// </summary>
        public void Dispose()
        {
            NgridClient.Dispose();
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Initializes this object.
        /// </summary>
        private void Initialize()
        {
            NgridClient.CommunicationWay = CommunicationWays.Send;
        }
        
        #endregion
    }
}
