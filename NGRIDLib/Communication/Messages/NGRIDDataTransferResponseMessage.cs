using NGRID.Serialization;

namespace NGRID.Communication.Messages
{
    /// <summary>
    /// This message is used to acknowledge/reject a message and to send a NGRIDDataTransferMessage in same message object.
    /// It is used in web services.
    /// </summary>
    public class NGRIDDataTransferResponseMessage : NGRIDMessage
    {
        /// <summary>
        /// MessageTypeId of message.
        /// It is used to serialize/deserialize message.
        /// </summary>
        public override int MessageTypeId
        {
            get { return NGRIDMessageFactory.MessageTypeIdNGRIDDataTransferResponseMessage; }
        }

        /// <summary>
        /// This field is used to acknowledge/reject to an incoming message.
        /// </summary>
        public NGRIDOperationResultMessage Result { get; set; }

        /// <summary>
        /// This field is used to send a new message.
        /// </summary>
        public NGRIDDataTransferMessage Message { get; set; }

        /// <summary>
        /// Serializes this message.
        /// </summary>
        /// <param name="serializer">Serializer used to serialize objects</param>
        public override void Serialize(INGRIDSerializer serializer)
        {
            base.Serialize(serializer);
            serializer.WriteObject(Result);
            serializer.WriteObject(Message);
        }

        /// <summary>
        /// Deserializes this message.
        /// </summary>
        /// <param name="deserializer">Deserializer used to deserialize objects</param>
        public override void Deserialize(INGRIDDeserializer deserializer)
        {
            base.Deserialize(deserializer);
            Result = deserializer.ReadObject(() => new NGRIDOperationResultMessage());
            Message = deserializer.ReadObject(() => new NGRIDDataTransferMessage());
        }
    }
}
