using System;
using System.Runtime.Serialization;

namespace NGRID.Exceptions
{
    /// <summary>
    /// Represents a NGRID Exception.
    /// This is the base class for exceptions that are thrown by NGRID system.
    /// </summary>
    [Serializable]
    public class NGRIDException : Exception
    {
        /// <summary>
        /// Contstructor.
        /// </summary>
        public NGRIDException()
        {

        }

        /// <summary>
        /// Contstructor.
        /// </summary>
        public NGRIDException(SerializationInfo serializationInfo, StreamingContext context)
            : base(serializationInfo, context)
        {

        }

        /// <summary>
        /// Contstructor.
        /// </summary>
        /// <param name="message">Exception message</param>
        public NGRIDException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Contstructor.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public NGRIDException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
