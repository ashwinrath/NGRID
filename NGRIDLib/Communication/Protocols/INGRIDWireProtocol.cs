using NGRID.Communication.Messages;
using NGRID.Serialization;

namespace NGRID.Communication.Protocols
{
    /// <summary>
    /// This interface is used to Write/Read messages according to a Wire/Communication Protocol.
    /// </summary>
    public interface INGRIDWireProtocol
    {
        /// <summary>
        /// Serializes and writes a NGRIDMessage according to the protocol rules.
        /// </summary>
        /// <param name="serializer">Serializer to serialize message</param>
        /// <param name="message">Message to be serialized</param>
        void WriteMessage(INGRIDSerializer serializer, NGRIDMessage message);

        /// <summary>
        /// Reads and constructs a NGRIDMessage according to the protocol rules.
        /// </summary>
        /// <param name="deserializer">Deserializer to read message</param>
        /// <returns>NGRIDMessage object that is read</returns>
        NGRIDMessage ReadMessage(INGRIDDeserializer deserializer);
    }
}
