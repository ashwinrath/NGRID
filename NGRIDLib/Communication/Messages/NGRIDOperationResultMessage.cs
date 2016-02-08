using NGRID.Serialization;

namespace NGRID.Communication.Messages
{
    /// <summary>
    /// This message is sent to clients as a response to an operation.
    /// It is generally used to send an ACK/Reject message for a message
    /// or a response to register message.
    /// </summary>
    public class NGRIDOperationResultMessage : NGRIDMessage
    {
        /// <summary>
        /// MessageTypeId of message.
        /// It is used to serialize/deserialize message.
        /// </summary>
        public override int MessageTypeId
        {
            get { return NGRIDMessageFactory.MessageTypeIdNGRIDOperationResultMessage; }
        }

        /// <summary>
        /// Operation result.
        /// True, if operation is successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// A text that may be used as a description for result of operation.
        /// </summary>
        public string ResultText { get; set; }

        /// <summary>
        /// Serializes this message.
        /// </summary>
        /// <param name="serializer">Serializer used to serialize objects</param>
        public override void Serialize(INGRIDSerializer serializer)
        {
            base.Serialize(serializer);
            serializer.WriteBoolean(Success);
            serializer.WriteStringUTF8(ResultText);
        }

        /// <summary>
        /// Deserializes this message.
        /// </summary>
        /// <param name="deserializer">Deserializer used to deserialize objects</param>
        public override void Deserialize(INGRIDDeserializer deserializer)
        {
            base.Deserialize(deserializer);
            Success = deserializer.ReadBoolean();
            ResultText = deserializer.ReadStringUTF8();
        }
    }
}
