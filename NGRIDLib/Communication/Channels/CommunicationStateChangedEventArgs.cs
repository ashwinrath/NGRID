using System;

namespace NGRID.Communication.Channels
{
    /// <summary>
    /// A delegate to create events for changing state of a communication channel.
    /// Old state is passed with event arguments, new state can be get from communication channel object (sender).
    /// </summary>
    /// <param name="sender">The communication channel object which raises event</param>
    /// <param name="e">Event arguments</param>
    public delegate void CommunicationStateChangedHandler(ICommunicationChannel sender, CommunicationStateChangedEventArgs e);

    /// <summary>
    /// Stores informations about communication channel's state.
    /// </summary>
    public class CommunicationStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The state of the client before change.
        /// </summary>
        public CommunicationStates OldState { get; set; }
    }
}
