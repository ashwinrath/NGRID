namespace NGRID.Communication.Messages.ControllerMessages
{
    /// <summary>
    /// This message is sent from NGRID manager to NGRID server to get graph of NGRID servers.
    /// </summary>
    public class GetServerGraphMessage : ControlMessage
    {
        /// <summary>
        /// Gets MessageTypeId for GetServerGraphMessage.
        /// </summary>
        public override int MessageTypeId
        {
            get { return ControlMessageFactory.MessageTypeIdGetServerGraphMessage; }
        }
    }
}
