using System;

namespace NGRID.Exceptions
{
    /// <summary>
    /// This exception is thrown when there is not a communicator of a remote application.
    /// </summary>
    [Serializable]
    public class NGRIDNoCommunicatorException : NGRIDException
    {
        /// <summary>
        /// Contstructor.
        /// </summary>
        public NGRIDNoCommunicatorException()
        {

        }

        /// <summary>
        /// Contstructor.
        /// </summary>
        /// <param name="message">Exception message</param>
        public NGRIDNoCommunicatorException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Contstructor.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public NGRIDNoCommunicatorException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
