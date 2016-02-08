using System.Collections.Generic;

namespace NGRID.Organization
{
    /// <summary>
    /// Represents a NGRID server on network.
    /// </summary>
    public class NGRIDServerNode
    {
        /// <summary>
        /// Name of the remote application
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Adjacent server nodes of this node.
        /// </summary>
        public SortedList<string, NGRIDServerNode> Adjacents { get; set; }

        /// <summary>
        /// Stores best paths to the all server nodes from this node
        /// </summary>
        public SortedList<string, List<NGRIDServerNode>> BestPathsToServers { get; set; }

        /// <summary>
        /// Constructur.
        /// </summary>
        /// <param name="name">name of server</param>
        public NGRIDServerNode(string name)
        {
            Name = name;
        }
    }
}
