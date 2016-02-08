using System;

namespace NGRID.Client.NGRIDServices
{
    /// <summary>
    /// This message is sent to invoke a method of an application that implements NGRIDService API.
    /// It is sent by NGRIDServiceProxyBase class and received by NGRIDServiceApplication class.
    /// </summary>
    [Serializable]
    public class NGRIDRemoteInvokeMessage
    {
        /// <summary>
        /// Name of the target service class.
        /// </summary>
        public string ServiceClassName { get; set; }

        /// <summary>
        /// Method of remote application to invoke.
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        /// Parameters of method.
        /// </summary>
        public object[] Parameters { get; set; }
    }
}
