using NGRID.Communication;
using NGRID.Organization;
using NGRID.Organization.Routing;
using NGRID.Settings;
using NGRID.Storage;
using NGRID.Threading;

namespace NGRID
{
    /// <summary>
    /// Represents a NGRID server. This is the main class to construct and run a NGRID server.
    /// </summary>
    public class NGRIDServer : IRunnable
    {
        /// <summary>
        /// Settings.
        /// </summary>
        private readonly NGRIDSettings _settings;

        /// <summary>
        /// Storage manager used for database operations.
        /// </summary>
        private readonly IStorageManager _storageManager;

        /// <summary>
        /// Routing table.
        /// </summary>
        private readonly RoutingTable _routingTable;

        /// <summary>
        /// A Graph consist of server nodes. It also holds references to NGRIDAdjacentServer objects.
        /// </summary>
        private readonly NGRIDServerGraph _serverGraph;

        /// <summary>
        /// Reference to all NGRID Managers. It contains communicators to all instances of NGRID manager.
        /// </summary>
        private readonly NGRIDController _ngridManager;

        /// <summary>
        /// List of applications
        /// </summary>
        private readonly NGRIDClientApplicationList _clientApplicationList;

        /// <summary>
        /// Communication layer.
        /// </summary>
        private readonly CommunicationLayer _communicationLayer;

        /// <summary>
        /// Organization layer.
        /// </summary>
        private readonly OrganizationLayer _organizationLayer;

        /// <summary>
        /// Constructor.
        /// </summary>
        public NGRIDServer()
        {
            _settings = NGRIDSettings.Instance;
            _serverGraph = new NGRIDServerGraph();
            _clientApplicationList = new NGRIDClientApplicationList();
            _ngridManager = new NGRIDController("NGRIDController");
            _storageManager = StorageManagerFactory.CreateStorageManager();
            _routingTable = new RoutingTable();
            _communicationLayer = new CommunicationLayer();
            _organizationLayer = new OrganizationLayer(_communicationLayer, _storageManager, _routingTable, _serverGraph, _clientApplicationList, _ngridManager);
            _ngridManager.OrganizationLayer = _organizationLayer;
        }

        /// <summary>
        /// Starts the NGRID server.
        /// </summary>
        public void Start()
        {
            _storageManager.Start();
            CorrectDatabase();
            _communicationLayer.Start();
            _organizationLayer.Start();
        }

        /// <summary>
        /// Stops the NGRID server.
        /// </summary>
        /// <param name="waitToStop">True, if caller thread must be blocked until NGRID server stops.</param>
        public void Stop(bool waitToStop)
        {
            _communicationLayer.Stop(waitToStop);
            _organizationLayer.Stop(waitToStop);
            _storageManager.Stop(waitToStop);
        }

        /// <summary>
        /// Waits stopping of NGRID server.
        /// </summary>
        public void WaitToStop()
        {
            _communicationLayer.WaitToStop();
            _organizationLayer.WaitToStop();
            _storageManager.WaitToStop();
        }

        /// <summary>
        /// Checks and corrects database records if needed.
        /// </summary>
        private void CorrectDatabase()
        {
            if (_settings["CheckDatabaseOnStartup"] != "true")
            {
                return;
            }

            //If Server graph is changed, records in storage engine (database) may be wrong, therefore, they must be updated 
            var nextServersList = _serverGraph.GetNextServersForDestServers();
            foreach (var nextServerItem in nextServersList)
            {
                _storageManager.UpdateNextServer(nextServerItem.Key, nextServerItem.Value);
            }
        }
    }
}
