using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Xml;
using NGRID.Exceptions;

namespace NGRID.Settings
{
    /// <summary>
    /// This class is used to get/set all settings for this server from/to an XML file.
    /// </summary>
    public class NGRIDSettings
    {
        #region Static properties

        /// <summary>
        /// Singleton instance of NGRIDSettings.
        /// </summary>
        public static NGRIDSettings Instance
        {
            get
            {
                if(_instance == null)
                {
                    lock(_synObj)
                    {
                        if (_instance == null)
                        {
                            _instance = new NGRIDSettings(Path.Combine(GeneralHelper.GetCurrentDirectory(), "NGRIDSettings.xml"));
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

        private static NGRIDSettings _instance;

        private static readonly object _synObj = new object();

        #endregion

        #region Public properties

        /// <summary>
        /// All defined NGRID servers in server graph.
        /// </summary>
        public List<ServerInfoItem> Servers { get; private set; }

        /// <summary>
        /// All defined Client applications.
        /// </summary>
        public List<ApplicationInfoItem> Applications { get; private set; }

        /// <summary>
        /// All defined Route informations.
        /// </summary>
        public List<RouteInfoItem> Routes { get; private set; }

        /// <summary>
        /// Name of this server.
        /// </summary>
        public string ThisServerName
        {
            get
            {
                return _thisServerName;
            }

            set 
            {
                _thisServerName = value;
                _settings["ThisServerName"] = _thisServerName;
            }
        }
        private string _thisServerName;

        /// <summary>
        /// This value indicates timeout value to wait for ACK/Reject message from a remote application for a data transfer message.
        /// </summary>
        public int MessageResponseTimeout { get; set; }

        /// <summary>
        /// Gets/Sets a setting.
        /// </summary>
        /// <param name="fieldName">Name of setting</param>
        /// <returns>Value of setting</returns>
        public string this[string fieldName]
        {
            get { return _settings[fieldName]; }
            set { _settings[fieldName] = value; }
        }
        private readonly NameValueCollection _settings;

        /// <summary>
        /// Path of XML settings file.
        /// </summary>
        public string FilePath { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new NGRIDSettings from XML file.
        /// </summary>
        /// <param name="settingsFilePath">Path of xml file.</param>
        public NGRIDSettings(string settingsFilePath)
        {
            FilePath = settingsFilePath;
            MessageResponseTimeout = 300000; //5 minutes
            Servers = new List<ServerInfoItem>();
            Applications = new List<ApplicationInfoItem>();
            Routes = new List<RouteInfoItem>();
            _settings = new NameValueCollection();
            LoadFromXml();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Saves current settings to XML file.
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

            //Settings node
            var settingsRootNode = xmlDoc.CreateElement("Settings");
            rootNode.AppendChild(settingsRootNode);

            //Setting nodes
            foreach (var key in _settings.AllKeys)
            {
                if (key.StartsWith("__"))
                {
                    continue;
                }

                var settingNode = xmlDoc.CreateElement("Setting");
                settingNode.SetAttribute("Key", key);
                settingNode.SetAttribute("Value", _settings[key]);
                settingsRootNode.AppendChild(settingNode);
            }

            //Servers node
            var serversRootNode = xmlDoc.CreateElement("Servers");
            rootNode.AppendChild(serversRootNode);

            //Server nodes
            foreach (var server in Servers)
            {
                var serverNode = xmlDoc.CreateElement("Server");
                serverNode.SetAttribute("Name", server.Name);
                serverNode.SetAttribute("IpAddress", server.IpAddress);
                serverNode.SetAttribute("Port", server.Port.ToString());
                serverNode.SetAttribute("Adjacents", server.Adjacents);
                serversRootNode.AppendChild(serverNode);
            }

            //Applications node
            var applicationRootNode = xmlDoc.CreateElement("Applications");
            rootNode.AppendChild(applicationRootNode);

            //Application nodes
            foreach (var application in Applications)
            {
                var applicationNode = xmlDoc.CreateElement("Application");
                applicationNode.SetAttribute("Name", application.Name);
                applicationRootNode.AppendChild(applicationNode);

                foreach (var channel in application.CommunicationChannels)
                {
                    var channelNode = xmlDoc.CreateElement("Communication");
                    channelNode.SetAttribute("Type", channel.CommunicationType);
                    switch (channel.CommunicationType)
                    {
                        case "WebService":
                            channelNode.SetAttribute("Url", channel.CommunicationSettings["Url"]);
                            break;
                    }

                    applicationNode.AppendChild(channelNode);
                }
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

            //Get Settings section
            var resultNodes = settingsXmlDoc.SelectNodes("/NGRIDConfiguration/Settings/Setting");
            if (resultNodes == null)
            {
                return;
            }

            foreach (XmlNode node in resultNodes)
            {
                _settings[node.Attributes["Key"].Value] = node.Attributes["Value"].Value;
            }

            if (string.IsNullOrEmpty(_settings["ThisServerName"]))
            {
                throw new NGRIDException("ThisServerName is not defined.");
            }

            _thisServerName = _settings["ThisServerName"];

            //Get Servers section
            resultNodes = settingsXmlDoc.SelectNodes("/NGRIDConfiguration/Servers/Server");
            if (resultNodes == null)
            {
                throw new NGRIDException("No server defined.");
            }

            foreach (XmlNode node in resultNodes)
            {
                Servers.Add(new ServerInfoItem
                                {
                                    Name = node.Attributes["Name"].Value.Trim(),
                                    IpAddress = node.Attributes["IpAddress"].Value,
                                    Port = Convert.ToInt32(node.Attributes["Port"].Value),
                                    Adjacents = node.Attributes["Adjacents"].Value
                                });
            }

            //Get Applications section
            resultNodes = settingsXmlDoc.SelectNodes("/NGRIDConfiguration/Applications/Application");
            if (resultNodes != null)
            {
                //Read all application entries from xml file
                foreach (XmlNode node in resultNodes)
                {
                    var application = new ApplicationInfoItem
                    {
                        Name = node.Attributes["Name"].Value
                    };

                    //Add predefined communication channels
                    foreach (XmlNode childNode in node.ChildNodes)
                    {
                        switch (childNode.Name)
                        {
                            case "Communication":
                                switch (childNode.Attributes["Type"].Value)
                                {
                                    case "WebService":
                                        var webServiceComm = new ApplicationInfoItem.CommunicationChannelInfoItem
                                        {
                                            CommunicationType = "WebService"
                                        };
                                        webServiceComm.CommunicationSettings.Add("Url", childNode.Attributes["Url"].Value);
                                        application.CommunicationChannels.Add(webServiceComm);
                                        break;
                                }

                                break;
                        }
                    }

                    Applications.Add(application);
                }
            }

            //Get Routes section
            resultNodes = settingsXmlDoc.SelectNodes("/NGRIDConfiguration/Routes/Route");
            if (resultNodes != null)
            {
                //Read all route entries from xml file
                foreach (XmlNode node in resultNodes)
                {
                    var route = new RouteInfoItem
                                {
                                    Name = node.Attributes["Name"].Value,
                                    DistributionType = GetAttribute(node, "DistributionType")
                                };

                    //Read all filter entries of route from xml file
                    var filterNodes = node.SelectNodes("/NGRIDConfiguration/Routes/Route/Filters/Filter");
                    if (filterNodes != null)
                    {
                        foreach (XmlNode filterNode in filterNodes)
                        {
                            var filter = new RouteInfoItem.FilterInfoItem
                                             {
                                                 SourceServer = GetAttribute(filterNode, "SourceServer"),
                                                 SourceApplication = GetAttribute(filterNode, "SourceApplication"),
                                                 DestinationServer = GetAttribute(filterNode, "DestinationServer"),
                                                 DestinationApplication =
                                                     GetAttribute(filterNode, "DestinationApplication"),
                                                 TransmitRule = GetAttribute(filterNode, "TransmitRule")
                                             };
                            route.Filters.Add(filter);
                        }
                    }

                    //Read all destination entries of route from xml file
                    var destinationNodes = node.SelectNodes("/NGRIDConfiguration/Routes/Route/Destinations/Destination");
                    if (destinationNodes != null)
                    {
                        foreach (XmlNode destinationNode in destinationNodes)
                        {
                            var destination = new RouteInfoItem.DestinationInfoItem
                                           {
                                               Application = GetAttribute(destinationNode, "Application"),
                                               Server = GetAttribute(destinationNode, "Server"),
                                               RouteFactor = Convert.ToInt32(GetAttribute(destinationNode, "RouteFactor") ?? "1")
                                           };
                            if (destination.RouteFactor <= 0)
                            {
                                destination.RouteFactor = 1;
                            }

                            route.Destinations.Add(destination);
                        }
                    }

                    Routes.Add(route);
                }
            }
        }

        private static string GetAttribute(XmlNode node, string attributeName)
        {
            foreach (XmlAttribute attribute in node.Attributes)
            {
                if(attribute.Name == attributeName)
                {
                    return attribute.Value;
                }
            }

            return null;
        }
     
        #endregion
    }
}
