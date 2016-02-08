using System;
using NGRID.Exceptions;

namespace NGRID.Client.NGRIDServices
{
    /// <summary>
    /// This message is sent as return message of a NGRIDRemoteInvokeMessage.
    /// It is used to send return value of method invocation.
    /// It is sent by NGRIDServiceApplication class and received by NGRIDServiceProxyBase class.
    /// </summary>
    [Serializable]
    public class NGRIDRemoteInvokeReturnMessage
    {
        /// <summary>
        /// Return value of remote method invocation.
        /// </summary>
        public object ReturnValue { get; set; }

        /// <summary>
        /// If any exception occured during method invocation, this field contains Exception object.
        /// If no exception occured, this field is null.
        /// </summary>
        public NGRIDRemoteException RemoteException { get; set; }
    }
}
