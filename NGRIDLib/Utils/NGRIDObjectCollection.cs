using System.Collections.Generic;

namespace NGRID.Utils
{
    /// <summary>
    /// Represents a thread-safe string-key based object collection.
    /// </summary>
    public class NGRIDObjectCollection
    {
        /// <summary>
        /// All objects are stored in this collection.
        /// </summary>
        private readonly SortedDictionary<string, object> _objects;

        /// <summary>
        /// Gets the synchronization object that is used by 
        /// </summary>
        public object SyncObj { get; private set; }

        /// <summary>
        /// Contructor.
        /// </summary>
        public NGRIDObjectCollection()
        {
            SyncObj = new object();
            _objects = new SortedDictionary<string, object>();
        }

        /// <summary>
        /// Gets/sets an object with given key.
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Object with given key, or null if key does not exists</returns>
        public object this[string key]
        {
            get 
            {
                lock (SyncObj)
                {
                    return _objects.ContainsKey(key) ? _objects[key] : null;
                }
            }

            set
            {
                lock (SyncObj)
                {
                    _objects[key] = value;
                }
            }
        }

        /// <summary>
        /// Removes an object from collection.
        /// </summary>
        /// <param name="key">Key of object to remove</param>
        public void Remove(string key)
        {
            lock (SyncObj)
            {
                _objects.Remove(key);
            }
        }

        /// <summary>
        /// Rewmoves all keys/values from collection.
        /// </summary>
        public void Clear()
        {
            lock (SyncObj)
            {
                _objects.Clear();
            }
        }
    }
}
