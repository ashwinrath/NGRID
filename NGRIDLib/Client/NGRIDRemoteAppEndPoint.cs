namespace NGRID.Client
{
    public class NGRIDRemoteAppEndPoint
    {
        public string ServerName { get; set; }

        public string ApplicationName { get; set; }

        public long CommunicatorId { get; set; }

        public NGRIDRemoteAppEndPoint()
        {
            
        }

        public NGRIDRemoteAppEndPoint(string applicationName)
        {
            ApplicationName = applicationName;
        }

        public NGRIDRemoteAppEndPoint(string serverName, string applicationName)
        {
            ServerName = serverName;
            ApplicationName = applicationName;
        }

        public NGRIDRemoteAppEndPoint(string serverName, string applicationName, long communicatorId)
        {
            ServerName = serverName;
            ApplicationName = applicationName;
            CommunicatorId = communicatorId;
        }
    }
}
