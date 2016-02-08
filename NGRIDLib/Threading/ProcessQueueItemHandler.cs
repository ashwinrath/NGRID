using System;

namespace NGRID.Threading
{
    /// <summary>
    /// A delegate to used by QueueProcessorThread to raise processing event
    /// </summary>
    /// <typeparam name="T">Type of the item to process</typeparam>
    /// <param name="sender">The object which raises event</param>
    /// <param name="e">Event arguments</param>
    public delegate void ProcessQueueItemHandler<T>(object sender, ProcessQueueItemEventArgs<T> e);

    /// <summary>
    /// Stores processing item and some informations about queue.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ProcessQueueItemEventArgs<T> : EventArgs
    {
        /// <summary>
        /// The item to process.
        /// </summary>
        public T ProcessItem { get; set; }

        /// <summary>
        /// The item count waiting for processing on queue (after this one).
        /// </summary>
        public int QueuedItemCount { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="processItem">The item to process</param>
        /// <param name="queuedItemCount">The item count waiting for processing on queue (after this one)</param>
        public ProcessQueueItemEventArgs(T processItem, int queuedItemCount)
        {
            ProcessItem = processItem;
            QueuedItemCount = queuedItemCount;
        }
    }
}