using NGRID.Serialization;

namespace NGRID.Communication.Messages.ControllerMessages
{
    /// <summary>
    /// This message is sent by NGRID Server to NGRID Manager as a response to a RemoveApplicationMessage.
    /// </summary>
    public class RemoveApplicationResponseMessage : ControlMessage
    {
        /// <summary>
        /// Gets MessageTypeId for RemoveApplicationResponseMessage.
        /// </summary>
        public override int MessageTypeId
        {
            get { return ControlMessageFactory.MessageTypeIdRemoveApplicationResponseMessage; }
        }

        /// <summary>
        /// Name of the new application.
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// True, if application is successfully removed.
        /// </summary>
        public bool Removed { get; set; }

        /// <summary>
        /// If Removed = True then "Success", else error message.
        /// </summary>
        public string ResultMessage { get; set; }

        /// <summary>
        /// Serializes this message.
        /// </summary>
        /// <param name="serializer">Serializer used to serialize objects</param>
        public override void Serialize(INGRIDSerializer serializer)
        {
            base.Serialize(serializer);
            serializer.WriteStringUTF8(ApplicationName);
            serializer.WriteBoolean(Removed);
            serializer.WriteStringUTF8(ResultMessage);
        }

        /// <summary>
        /// Deserializes this message.
        /// </summary>
        /// <param name="deserializer">Deserializer used to deserialize objects</param>
        public override void Deserialize(INGRIDDeserializer deserializer)
        {
            base.Deserialize(deserializer);
            ApplicationName = deserializer.ReadStringUTF8();
            Removed = deserializer.ReadBoolean();
            ResultMessage = deserializer.ReadStringUTF8();
        }
    }
}
