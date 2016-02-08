using NGRID.Serialization;

namespace NGRID.Communication.Messages.ControllerMessages
{
    /// <summary>
    /// This message is used to get an operation result message.
    /// </summary>
    public class OperationResultMessage : ControlMessage
    {        
        /// <summary>
        /// Gets MessageTypeId for OperationResultMessage.
        /// </summary>
        public override int MessageTypeId
        {
            get { return ControlMessageFactory.MessageTypeIdOperationResultMessage; }
        }

        /// <summary>
        /// True, if operation is successfully executed.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// If Success = True then "Success.", else error message.
        /// </summary>
        public string ResultMessage { get; set; }

        /// <summary>
        /// Serializes this message.
        /// </summary>
        /// <param name="serializer">Serializer used to serialize objects</param>
        public override void Serialize(INGRIDSerializer serializer)
        {
            base.Serialize(serializer);
            serializer.WriteBoolean(Success);
            serializer.WriteStringUTF8(ResultMessage);
        }

        /// <summary>
        /// Deserializes this message.
        /// </summary>
        /// <param name="deserializer">Deserializer used to deserialize objects</param>
        public override void Deserialize(INGRIDDeserializer deserializer)
        {
            base.Deserialize(deserializer);
            Success = deserializer.ReadBoolean();
            ResultMessage = deserializer.ReadStringUTF8();
        }
    }
}
