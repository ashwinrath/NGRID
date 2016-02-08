using System;
using System.Runtime.Serialization;

namespace NGRID.Exceptions
{
    /// <summary>
    /// Represents a NGRID Remote Exception.
    /// This exception is used to send an exception from an application to another application.
    /// </summary>
    [Serializable]
    public class NGRIDRemoteException : NGRIDException
    {
        /// <summary>
        /// Contstructor.
        /// </summary>
        public NGRIDRemoteException()
        {

        }

        /// <summary>
        /// Contstructor.
        /// </summary>
        public NGRIDRemoteException(SerializationInfo serializationInfo, StreamingContext context)
            : base(serializationInfo, context)
        {
            
        }

        /// <summary>
        /// Contstructor.
        /// </summary>
        /// <param name="message">Exception message</param>
        public NGRIDRemoteException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Contstructor.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public NGRIDRemoteException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
