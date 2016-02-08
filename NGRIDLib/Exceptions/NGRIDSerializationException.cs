using System;

namespace NGRID.Exceptions
{
    /// <summary>
    /// Represents an Serialization / Deserialization exception.
    /// </summary>
    [Serializable]
    public class NGRIDSerializationException : NGRIDException
    {
        /// <summary>
        /// Contstructor.
        /// </summary>
        public NGRIDSerializationException()
        {

        }

        /// <summary>
        /// Contstructor.
        /// </summary>
        /// <param name="message">Exception message</param>
        public NGRIDSerializationException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Contstructor.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public NGRIDSerializationException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
