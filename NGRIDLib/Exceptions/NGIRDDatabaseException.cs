using System;

namespace NGRID.Exceptions
{
    /// <summary>
    /// Represents a Database exception.
    /// </summary>
    [Serializable]
    public class NGRIDDatabaseException : NGRIDException
    {
        /// <summary>
        /// Executed query text
        /// </summary>
        public string QueryText { set;  get; }

        /// <summary>
        /// Contstructor.
        /// </summary>
        public NGRIDDatabaseException()
        {

        }

        /// <summary>
        /// Contstructor.
        /// </summary>
        /// <param name="message">Exception message</param>
        public NGRIDDatabaseException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Contstructor.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public NGRIDDatabaseException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
