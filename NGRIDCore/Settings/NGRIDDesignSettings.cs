using System.Collections.Generic;
using System.IO;
using System.Xml;
using NGRID.Exceptions;

namespace NGRID.Settings
{
    /// <summary>
    /// This class is used to get/set all design settings for this server from/to an XML file.
    /// Design settings is used by NGRID Manager GUI.
    /// </summary>
    public class NGRIDDesignSettings
    {
        #region Static properties

        /// <summary>
        /// Singleton instance of NGRIDSettings.
        /// </summary>
        public static NGRIDDesignSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_synObj)
                    {
                        if (_instance == null)
                        {
                            _instance = new NGRIDDesignSettings(Path.Combine(GeneralHelper.GetCurrentDirectory(), "NGRIDSettings.design.xml"));
                        }
                    }
                }

                return _instance;
            }

            set
            {
                lock (_synObj)
                {
                    _instance = value;
                }
            }
        }

        private static NGRIDDesignSettings _instance;

        private static readonly object _synObj = new object();

        #endregion

        #region Public properties

        /// <summary>
        /// All defined NGRID servers in server graph.
        /// </summary>
        public List<ServerDesignItem> Servers { get; private set; }

        /// <summary>
        /// Path of XML design settings file.
        /// </summary>
        public string FilePath { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new NGRIDSettings from XML file.
        /// </summary>
        /// <param name="settingsFilePath">Path of xml file.</param>
        public NGRIDDesignSettings(string settingsFilePath)
        {
            FilePath = settingsFilePath;
            Servers = new List<ServerDesignItem>();
            LoadFromXml();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Saves current design settings to XML file.
        /// </summary>
        public void SaveToXml()
        {
            //Create directory if needed
            var saveDirectory = Path.GetDirectoryName(FilePath);
            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
            }

            //Create XmlDocument object to create XML file
            var xmlDoc = new XmlDocument();

            //XML declaration
            var xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
            xmlDoc.InsertBefore(xmlDeclaration, xmlDoc.DocumentElement);

            //Root node
            var rootNode = xmlDoc.CreateElement("NGRIDConfiguration");
            xmlDoc.AppendChild(rootNode);

            //Servers node
            var serversRootNode = xmlDoc.CreateElement("Servers");
            rootNode.AppendChild(serversRootNode);

            //Server nodes
            foreach (var server in Servers)
            {
                var serverNode = xmlDoc.CreateElement("Server");
                serverNode.SetAttribute("Name", server.Name);
                serverNode.SetAttribute("Location", server.Location);
                serversRootNode.AppendChild(serverNode);
            }

            //Save XML document
            xmlDoc.Save(FilePath);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Gets all settings from XML file.
        /// </summary>
        private void LoadFromXml()
        {
            //Create XmlDocument object to read XML settings file
            var settingsXmlDoc = new XmlDocument();
            settingsXmlDoc.Load(FilePath);

            //Get Servers section
            var resultNodes = settingsXmlDoc.SelectNodes("/NGRIDConfiguration/Servers/Server");
            if (resultNodes == null)
            {
                throw new NGRIDException("No server defined");
            }

            foreach (XmlNode node in resultNodes)
            {
                Servers.Add(new ServerDesignItem
                                {
                                    Name = node.Attributes["Name"].Value.Trim(),
                                    Location = node.Attributes["Location"].Value
                                });
            }
        }

        #endregion
    }
}
