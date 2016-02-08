using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using log4net;
using NGRID.Exceptions;
using NGRID.Settings;
using NGRID.Threading;

namespace NGRID.Organization
{
    /// <summary>
    /// Represents all servers on network as a graph.
    /// And also stores references to all communicating adjacent servers.
    /// </summary>
    public class NGRIDServerGraph : IRunnable
    {
        #region Public properties

        /// <summary>
        /// Gets this (current) server node.
        /// </summary>
        public NGRIDServerNode ThisServerNode { get; private set; }

        /// <summary>
        /// All server nodes on network.
        /// </summary>
        public SortedList<string, NGRIDServerNode> ServerNodes { get; private set; }

        /// <summary>
        /// A collection that stores communicating adjacent NGRID servers to this NGRID server.
        /// </summary>
        public SortedList<string, NGRIDAdjacentServer> AdjacentServers { get; private set; }

        #endregion

        #region Private fields

        /// <summary>
        /// Reference to logger.
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Reference to settings.
        /// </summary>
        private readonly NGRIDSettings _settings;

        /// <summary>
        /// This Timer is used to check ngrid server connections,
        /// send ping messages and reconnect if needed.
        /// </summary>
        private readonly Timer _pingTimer;
        
        /// <summary>
        /// This flag is used to start/stop _pingTimer.
        /// </summary>
        private volatile bool _running;

        #endregion

        #region Constructors

        /// <summary>
        /// Contructor.
        /// </summary>
        public NGRIDServerGraph()
        {
            _settings = NGRIDSettings.Instance;
            ServerNodes = new SortedList<string, NGRIDServerNode>();
            AdjacentServers = new SortedList<string, NGRIDAdjacentServer>();
            _pingTimer = new Timer(PingTimer_Elapsed, null, Timeout.Infinite, Timeout.Infinite);
            try
            {
                CreateGraph();
            }
            catch (NGRIDException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new NGRIDException("Can not read settings file. Detail: " + ex.Message, ex);
            }
        }

        #endregion

        #region Public methods

        public void Start()
        {
            foreach (var server in AdjacentServers.Values)
            {
                server.Start();
            }

            _running = true;
            _pingTimer.Change(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        }

        public void Stop(bool waitToStop)
        {
            lock (_pingTimer)
            {
                _running = false;
                _pingTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }

            foreach (var server in AdjacentServers.Values)
            {
                server.Stop(waitToStop);
            }
        }

        public void WaitToStop()
        {
            foreach (var server in AdjacentServers.Values)
            {
                server.WaitToStop();
            }
        }

        /// <summary>
        /// Calculates all next (adjacent) servers for all destination servers. 
        /// </summary>
        /// <returns>
        /// List of "Destination server name - Next server name" pairs.
        /// For example:
        /// If path is "ThisServer - ServerB - ServerC" than Destination server is ServerC and NextServer is ServerB.
        /// </returns>
        public List<KeyValuePair<string, string>> GetNextServersForDestServers()
        {
            var list = new List<KeyValuePair<string, string>>(ServerNodes.Count);
            list.Add(new KeyValuePair<string, string>(ThisServerNode.Name, ThisServerNode.Name));

            foreach (var destNode in ServerNodes.Values)
            {
                if (!ThisServerNode.BestPathsToServers.ContainsKey(destNode.Name))
                {
                    continue;
                }

                var bestPath = ThisServerNode.BestPathsToServers[destNode.Name];
                if (bestPath.Count > 1)
                {
                    list.Add(new KeyValuePair<string, string>(destNode.Name, bestPath[1].Name));
                }
            }

            return list;
        }

        #endregion

        #region Private methods

        #region Initializing methods

        /// <summary>
        /// Creates graph according to settings.
        /// </summary>
        private void CreateGraph()
        {
            CreateServerNodes();
            JoinNodes();
            SetCurrentServer();
            CreateAdjacentServers();
            CalculateShortestPaths();
        }

        /// <summary>
        /// Creates ServerNodes list from _settings.
        /// </summary>
        private void CreateServerNodes()
        {
            foreach (var server in _settings.Servers)
            {
                ServerNodes.Add(server.Name, new NGRIDServerNode(server.Name));
            }
        }

        /// <summary>
        /// Gets adjacent nodes from settings and joins servers in ServerNodes.
        /// </summary>
        private void JoinNodes()
        {
            //Gets adjacents of all nodes
            var adjacentsOfServers = new SortedList<string, string>();
            foreach (var server in _settings.Servers)
            {
                adjacentsOfServers.Add(server.Name, server.Adjacents);
            }

            //Join adjacents of all nodes
            foreach (var serverName in adjacentsOfServers.Keys)
            {
                //Create adjacent list
                ServerNodes[serverName].Adjacents = new SortedList<string, NGRIDServerNode>();
                //Get adjacent names
                var adjacents = adjacentsOfServers[serverName].Split(',');
                //Add nodes as adjacent
                foreach (var adjacent in adjacents)
                {
                    var trimmedAdjacentName = adjacent.Trim();
                    if (string.IsNullOrEmpty(trimmedAdjacentName))
                    {
                        continue;
                    }

                    if (!ServerNodes.ContainsKey(trimmedAdjacentName))
                    {
                        throw new NGRIDException("Adjacent server (" + trimmedAdjacentName + ") of server (" + serverName + ") can not be found in servers list.");
                    }

                    ServerNodes[serverName].Adjacents.Add(trimmedAdjacentName, ServerNodes[trimmedAdjacentName]);
                }
            }
        }
        
        /// <summary>
        /// Sets ThisServerNode field according to _settings and ServerNodes
        /// </summary>
        private void SetCurrentServer()
        {
            if (ServerNodes.ContainsKey(_settings.ThisServerName))
            {
                ThisServerNode = ServerNodes[_settings.ThisServerName];
            }
            else
            {
                throw new NGRIDException("Current server is not defined in settings file.");
            }
        }

        /// <summary>
        /// Fills AdjacentServers collection according to settings.
        /// </summary>
        private void CreateAdjacentServers()
        {
            //Create NGRIDAdjacentServer objects to communicate with adjacent servers of this Server
            foreach (var server in _settings.Servers)
            {
                //If the node is this server, get IP and Port informations
                if (server.Name == _settings.ThisServerName)
                {
                    _settings["__ThisServerTCPPort"] = server.Port.ToString();
                }
                //If the node is adjacent to this server, create a NGRIDAdjacentServer object to communicate
                else if (ThisServerNode.Adjacents.ContainsKey(server.Name))
                {
                    AdjacentServers.Add(server.Name, new NGRIDAdjacentServer(server.Name, server.IpAddress, server.Port));
                }
            }
        }

        /// <summary>
        /// Calculates all shorted paths from all nodes to other nodes.
        /// </summary>
        private void CalculateShortestPaths()
        {
            //Find shortest paths from all servers to all servers
            foreach (var sourceNodeName in ServerNodes.Keys)
            {
                ServerNodes[sourceNodeName].BestPathsToServers = new SortedList<string, List<NGRIDServerNode>>();
                foreach (var destinationNodeName in ServerNodes.Keys)
                {
                    //Do not search if source and destination nodes are same
                    if (sourceNodeName == destinationNodeName)
                    {
                        continue;
                    }

                    var shortestPath = FindShortestPath(ServerNodes[sourceNodeName], ServerNodes[destinationNodeName]);
                    if (shortestPath == null)
                    {
                        throw new NGRIDException("There is no path from server '" + sourceNodeName + "' to server '" + destinationNodeName + "'");
                    }

                    ServerNodes[sourceNodeName].BestPathsToServers[destinationNodeName] = shortestPath;
                }
            }
        }

        /// <summary>
        /// Find one of the shortest paths from given source node to destination node.
        /// </summary>
        /// <param name="sourceServerNode">Source node</param>
        /// <param name="destServerNode">Destination node</param>
        /// <returns>A path from source to destination</returns>
        private static List<NGRIDServerNode> FindShortestPath(NGRIDServerNode sourceServerNode, NGRIDServerNode destServerNode)
        {
            //Find all paths
            var allPaths = new List<List<NGRIDServerNode>>();
            FindPaths(sourceServerNode, destServerNode, allPaths, new List<NGRIDServerNode>());

            //Get shortest
            if (allPaths.Count > 0)
            {
                var bestPath = allPaths[0];
                for (var i = 1; i < allPaths.Count; i++)
                {
                    if (bestPath.Count > allPaths[i].Count)
                    {
                        bestPath = allPaths[i];
                    }
                }

                return bestPath;
            }

            //No path from sourceServerNode to destServerNode
            return null;
        }

        /// <summary>
        /// Finds all the paths from currentServerNode to destServerNode as a recursive method.
        /// Passes all nodes, if destination node found, it is added to paths
        /// </summary>
        /// <param name="currentServerNode">Current server node</param>
        /// <param name="destServerNode">Destination server node</param>
        /// <param name="paths">All possible paths are inserted to this list</param>
        /// <param name="passedNodes"></param>
        private static void FindPaths(NGRIDServerNode currentServerNode, NGRIDServerNode destServerNode, ICollection<List<NGRIDServerNode>> paths, ICollection<NGRIDServerNode> passedNodes)
        {
            //Add current node to passedNodes to prevent multi-pass over same node
            passedNodes.Add(currentServerNode);

            //If current node is destination, then add passed nodes to found paths.
            if (currentServerNode == destServerNode)
            {
                var foundPath = new List<NGRIDServerNode>();
                foundPath.AddRange(passedNodes);
                paths.Add(foundPath);
            }
            //Else, Jump to adjacents nodes of current node and conitnue searching
            else
            {
                foreach (var adjacentServerNode in currentServerNode.Adjacents.Values)
                {
                    //If passed over this adjacentServerNode before, skip it
                    if (passedNodes.Contains(adjacentServerNode))
                    {
                        continue;
                    }
                    
                    //Search path from this adjacent server to destination (recursive call)
                    FindPaths(adjacentServerNode, destServerNode, paths, passedNodes);
                    //Remove node from passed nodes, because we may pass over this node for searching another path
                    passedNodes.Remove(adjacentServerNode);
                }
            }
        }

        #endregion

        #region Checking server connections periodically

        /// <summary>
        /// This method is called by _pingTimer periodically.
        /// </summary>
        /// <param name="state">Not used argument</param>
        private void PingTimer_Elapsed(object state)
        {
            try
            {
                //Stop ping timer temporary
                _pingTimer.Change(Timeout.Infinite, Timeout.Infinite);

                //Check connection for all adjacent servers
                foreach (var server in AdjacentServers.Values)
                {
                    try
                    {
                        server.CheckConnection();
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex.Message, ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warn(ex.Message, ex);
            }
            finally
            {
                //Schedule ping timer for next running if NGRIDServerGraph is still running.
                lock (_pingTimer)
                {
                    if (_running)
                    {
                        _pingTimer.Change(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
                    }
                }
            }
        }

        #endregion

        #endregion
    }
}