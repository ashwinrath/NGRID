using NGRID.Communication.Messages;
using NGRID.Exceptions;
using NGRID.Serialization;

namespace NGRID.Communication.Protocols
{
    /// <summary>
    /// This class is the Default Protocol that is used by NGRID to communicate with other applications.
    /// A message frame is sent and received by NGRIDDefaultWireProtocol:
    /// 
    /// - Protocol type: 4 bytes unsigned integer. 
    ///   Must be NGRIDDefaultProtocolType for NGRIDDefaultWireProtocol.
    /// - Message type: 4 bytes integer.
    ///   Must be defined in NGRIDMessageFactory class.
    /// - Serialized bytes of a NGRIDMessage object.
    /// </summary>
    public class NGRIDDefaultWireProtocol : INGRIDWireProtocol
    {
        /// <summary>
        /// Specific number that a message must start with.
        /// </summary>
        public const uint NGRIDDefaultProtocolType = 19180685;

        /// <summary>
        /// Serializes and writes a NGRIDMessage according to the protocol rules.
        /// </summary>
        /// <param name="serializer">Serializer to serialize message</param>
        /// <param name="message">Message to be serialized</param>
        public void WriteMessage(INGRIDSerializer serializer, NGRIDMessage message)
        {
            //Write protocol type
            serializer.WriteUInt32(NGRIDDefaultProtocolType);
            
            //Write the message type
            serializer.WriteInt32(message.MessageTypeId);
            
            //Write message
            serializer.WriteObject(message);
        }

        /// <summary>
        /// Reads and constructs a NGRIDMessage according to the protocol rules.
        /// </summary>
        /// <param name="deserializer">Deserializer to read message</param>
        /// <returns>NGRIDMessage object that is read</returns>
        public NGRIDMessage ReadMessage(INGRIDDeserializer deserializer)
        {
            //Read protocol type
            var protocolType = deserializer.ReadUInt32();
            if (protocolType != NGRIDDefaultProtocolType)
            {
                throw new NGRIDException("Wrong protocol type: " + protocolType + ".");
            }

            //Read message type
            var messageTypeId = deserializer.ReadInt32();

            //Read and return message
            return deserializer.ReadObject(() => NGRIDMessageFactory.CreateMessageByTypeId(messageTypeId));
        }
    }
}
