using NGRID.Serialization;

namespace NGRID.Communication.Messages
{
    /// <summary>
    /// This class represents a message that is being transmitted between NGRID server and a Controller (NGRID Manager).
    /// </summary>
    public class NGRIDControllerMessage : NGRIDMessage
    {
        /// <summary>
        /// MessageTypeId for NGRIDControllerMessage.
        /// </summary>
        public override int MessageTypeId
        {
            get { return NGRIDMessageFactory.MessageTypeIdNGRIDControllerMessage; }
        }

        /// <summary>
        /// MessageTypeId of ControllerMessage.
        /// This field is used to deserialize MessageData.
        /// All types defined in ControlMessageFactory class.
        /// </summary>
        public int ControllerMessageTypeId { get; set; }

        /// <summary>
        /// Essential message data.
        /// This is a serialized object of a class in NGRID.Communication.Messages.ControllerMessages namespace.
        /// </summary>
        public byte[] MessageData { get; set; }

        public override void Serialize(INGRIDSerializer serializer)
        {
            base.Serialize(serializer);
            serializer.WriteInt32(ControllerMessageTypeId);
            serializer.WriteByteArray(MessageData);
        }

        public override void Deserialize(INGRIDDeserializer deserializer)
        {
            base.Deserialize(deserializer);
            ControllerMessageTypeId = deserializer.ReadInt32();
            MessageData = deserializer.ReadByteArray();
        }
    }
}
