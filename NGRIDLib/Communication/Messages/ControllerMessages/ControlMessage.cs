using NGRID.Serialization;

namespace NGRID.Communication.Messages.ControllerMessages
{
    /// <summary>
    /// This is the base class for all messages that are being transmited between NGRID server and Management (Controller) application.
    /// </summary>
    public abstract class ControlMessage : INGRIDSerializable
    {
        /// <summary>
        /// MessageTypeIf of message.
        /// It is used to serialize/deserialize message.
        /// </summary>
        public abstract int MessageTypeId { get; }

        /// <summary>
        /// Serializes this message.
        /// </summary>
        /// <param name="serializer">Serializer used to serialize objects</param>
        public virtual void Serialize(INGRIDSerializer serializer)
        {
            //No data to serialize
        }

        /// <summary>
        /// Deserializes this message.
        /// </summary>
        /// <param name="deserializer">Deserializer used to deserialize objects</param>
        public virtual void Deserialize(INGRIDDeserializer deserializer)
        {
            //No data to deserialize
        }
    }
}
