using System.Collections.Generic;
using System.Collections.Specialized;

namespace NGRID.Settings
{
    /// <summary>
    /// Represents a Client Application's informations in settings.
    /// </summary>
    public class ApplicationInfoItem
    {
        /// <summary>
        /// Name of this server.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Predefined communication channels.
        /// </summary>
        public List<CommunicationChannelInfoItem> CommunicationChannels { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ApplicationInfoItem()
        {
            CommunicationChannels = new List<CommunicationChannelInfoItem>();
        }

        /// <summary>
        /// Represents a predefined Communication channel for an Application.
        /// </summary>
        public class CommunicationChannelInfoItem
        {
            /// <summary>
            /// Type of communicaton. Can be WebService.
            /// </summary>
            public string CommunicationType { get; set; }

            /// <summary>
            /// Settings for communication. For example, includes Url info if CommunicationType is WebService.
            /// </summary>
            public NameValueCollection CommunicationSettings { get; set; }

            /// <summary>
            /// Constructor.
            /// </summary>
            public CommunicationChannelInfoItem()
            {
                CommunicationSettings = new NameValueCollection();
            }
        }
    }
}
