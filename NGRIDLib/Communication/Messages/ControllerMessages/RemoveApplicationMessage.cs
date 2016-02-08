using NGRID.Serialization;

namespace NGRID.Communication.Messages.ControllerMessages
{
    /// <summary>
    /// This message is sent by NGRID Manager to NGRID Server to remove a Application from NGRID.
    /// </summary>
    public class RemoveApplicationMessage : ControlMessage
    {
        /// <summary>
        /// Gets MessageTypeId for RemoveApplicationMessage.
        /// </summary>
        public override int MessageTypeId
        {
            get { return ControlMessageFactory.MessageTypeIdRemoveApplicationMessage; }
        }

        /// <summary>
        /// Name of the removing application.
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
