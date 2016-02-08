using System;

namespace NGRID.Exceptions
{
    /// <summary>
    /// Represents an Timeout exception.
    /// </summary>
    [Serializable]
    public class NGRIDTimeoutException : NGRIDException
    {
        /// <summary>
        /// Contstructor.
        /// </summary>
        public NGRIDTimeoutException()
        {

        }

        /// <summary>
        /// Contstructor.
        /// </summary>
        /// <param name="message">Exception message</param>
        public NGRIDTimeoutException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Contstructor.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public NGRIDTimeoutException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
