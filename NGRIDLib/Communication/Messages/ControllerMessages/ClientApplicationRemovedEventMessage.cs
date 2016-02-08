using NGRID.Serialization;

namespace NGRID.Communication.Messages.ControllerMessages
{
    /// <summary>
    /// This message is sent to all connected NGRID managers/controllers by NGRID Server to inform when a client application is removed.
    /// </summary>
    public class ClientApplicationRemovedEventMessage : ControlMessage
    {
        /// <summary>
        /// Gets MessageTypeId for ClientApplicationRemovedEventMessage.
        /// </summary>
        public override int MessageTypeId
        {
            get { return ControlMessageFactory.MessageTypeIdClientApplicationRemovedEventMessage; }
        }

        /// <summary>
        /// Name of the new application.
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// Serializes this message.
        /// </summary>
        /// <param name="serializer">Serializer used to serialize objects</param>
        public override void Serialize(INGRIDSerializer serializer)
        {
            base.Serialize(serializer);
            serializer.WriteStringUTF8(ApplicationName);
        }

        /// <summary>
        /// Deserializes this message.
        /// </summary>
        /// <param name="deserializer">Deserializer used to deserialize objects</param>
        public override void Deserialize(INGRIDDeserializer deserializer)
        {
            base.Deserialize(deserializer);
            ApplicationName = deserializer.ReadStringUTF8();
        }
    }
}
