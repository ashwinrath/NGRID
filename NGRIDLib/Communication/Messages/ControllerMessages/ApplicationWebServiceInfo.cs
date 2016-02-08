using NGRID.Serialization;

namespace NGRID.Communication.Messages.ControllerMessages
{
    /// <summary>
    /// Represents a web service communicator information for an application.
    /// </summary>
    public class ApplicationWebServiceInfo : INGRIDSerializable
    {
        /// <summary>
        /// Url of the web service.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Serializes this message.
        /// </summary>
        /// <param name="serializer">Serializer used to serialize objects</param>
        public void Serialize(INGRIDSerializer serializer)
        {
            serializer.WriteStringUTF8(Url);
        }

        /// <summary>
        /// Deserializes this message.
        /// </summary>
        /// <param name="deserializer">Deserializer used to deserialize objects</param>
        public void Deserialize(INGRIDDeserializer deserializer)
        {
            Url = deserializer.ReadStringUTF8();
        }
    }
}
