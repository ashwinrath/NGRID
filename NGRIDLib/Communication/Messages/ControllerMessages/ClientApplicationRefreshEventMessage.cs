using NGRID.Serialization;

namespace NGRID.Communication.Messages.ControllerMessages
{
    /// <summary>
    /// This message is sent to all connected NGRID managers/controllers by NGRID Server to inform about latest informations/state of a client application.
    /// </summary>
    public class ClientApplicationRefreshEventMessage : ControlMessage
    {
        /// <summary>
        /// Gets MessageTypeId for GetApplicationListResponseMessage.
        /// </summary>
        public override int MessageTypeId
        {
            get { return ControlMessageFactory.MessageTypeIdClientApplicationRefreshEventMessage; }
        }

        /// <summary>
        /// Name of the client application
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Currently connected (online) communicator count.
        /// </summary>
        public int CommunicatorCount { get; set; }

        /// <summary>
        /// Serializes this message.
        /// </summary>
        /// <param name="serializer">Serializer used to serialize objects</param>
        public override void Serialize(INGRIDSerializer serializer)
        {
            base.Serialize(serializer);
            serializer.WriteStringUTF8(Name);
            serializer.WriteInt32(CommunicatorCount);
        }

        /// <summary>
        /// Deserializes this message.
        /// </summary>
        /// <param name="deserializer">Deserializer used to deserialize objects</param>
        public override void Deserialize(INGRIDDeserializer deserializer)
        {
            base.Deserialize(deserializer);
            Name = deserializer.ReadStringUTF8();
            CommunicatorCount = deserializer.ReadInt32();
        }
    }
}
