using System;

namespace NGRID.Communication.Events
{
    /// <summary>
    /// A delegate to create events for changing state of a communicator.
    /// </summary>
    /// <param name="sender">The object which raises event</param>
    /// <param name="e">Event arguments</param>
    public delegate void CommunicatorStateChangedHandler(object sender, CommunicatorStateChangedEventArgs e);

    /// <summary>
    /// Stores informations about communicator and it's state.
    /// </summary>
    public class CommunicatorStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Communicator.
        /// </summary>
        public CommunicatorBase Communicator { get; set; }

        /// <summary>
        /// The state of the communicator before change.
        /// </summary>
        public CommunicationStates OldState { get; set; }
    }
}
