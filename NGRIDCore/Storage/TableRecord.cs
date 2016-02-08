using System.Collections.Generic;

namespace NGRID.Storage
{
    /// <summary>
    /// Represents a single row in a table in a database.
    /// </summary>
    public class TableRecord
    {
        /// <summary>
        /// Gets/Sets a field value.
        /// </summary>
        /// <param name="fieldName">Name of field</param>
        /// <returns>Value of field</returns>
        public object this[string fieldName]
        {
            get
            {
                return _fields[fieldName];
            }

            set
            {
                _fields[fieldName] = value;
            }
        }

        /// <summary>
        /// Fields and their values in this record.
        /// </summary>
        private readonly SortedList<string, object> _fields;

        /// <summary>
        /// Creates a new TableRecord object.
        /// </summary>
        public TableRecord()
        {
            _fields = new SortedList<string, object>();
        }
    }
}
