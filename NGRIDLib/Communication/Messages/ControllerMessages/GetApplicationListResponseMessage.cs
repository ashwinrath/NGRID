using NGRID.Serialization;

namespace NGRID.Communication.Messages.ControllerMessages
{
    /// <summary>
    /// This message is sent from NGRID server to NGRID manager as a response to GetApplicationListMessage message.
    /// </summary>
    public class GetApplicationListResponseMessage : ControlMessage
    {
        /// <summary>
        /// Gets MessageTypeId for GetApplicationListResponseMessage.
        /// </summary>
        public override int MessageTypeId
        {
            get { return ControlMessageFactory.MessageTypeIdGetApplicationListResponseMessage; }
        }

        /// <summary>
        /// List of client applications.
        /// </summary>
        public ClientApplicationInfo[] ClientApplications { get; set; }

        /// <summary>
        /// Serializes this message.
        /// </summary>
        /// <param name="serializer">Serializer used to serialize objects</param>
        public override void Serialize(INGRIDSerializer serializer)
        {
            base.Serialize(serializer);
            serializer.WriteObjectArray(ClientApplications);
        }

        /// <summary>
        /// Deserializes this message.
        /// </summary>
        /// <param name="deserializer">Deserializer used to deserialize objects</param>
        public override void Deserialize(INGRIDDeserializer deserializer)
        {
            base.Deserialize(deserializer);
            ClientApplications = deserializer.ReadObjectArray(() => new ClientApplicationInfo());
        }

        /// <summary>
        /// This class is used to transfer simple information about a NGRID Client Application.
        /// </summary>
        public class ClientApplicationInfo : INGRIDSerializable
        {
            /// <summary>
            /// Name of the client application
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Currently connected (online) communicator count.
            /// </summary>
            public int CommunicatorCount { get; set; }

            /// <summary>
            /// Serializes this message.
            /// </summary>
            /// <param name="serializer">Serializer used to serialize objects</param>
            public void Serialize(INGRIDSerializer serializer)
            {
                serializer.WriteStringUTF8(Name);
                serializer.WriteInt32(CommunicatorCount);
            }

            /// <summary>
            /// Deserializes this message.
            /// </summary>
            /// <param name="deserializer">Deserializer used to deserialize objects</param>
            public void Deserialize(INGRIDDeserializer deserializer)
            {
                Name = deserializer.ReadStringUTF8();
                CommunicatorCount = deserializer.ReadInt32();
            }
        }
    }
}
