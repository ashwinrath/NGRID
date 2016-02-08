using NGRID.Serialization;

namespace NGRID.Communication.Messages.ControllerMessages
{
    /// <summary>
    /// This message is sent from NGRID manager to NGRID server to update server graph of NGRID.
    /// </summary>
    public class UpdateServerGraphMessage : ControlMessage
    {
        /// <summary>
        /// Gets MessageTypeId for UpdateServerGraphMessage.
        /// </summary>
        public override int MessageTypeId
        {
            get { return ControlMessageFactory.MessageTypeIdUpdateServerGraphMessage; }
        }

        /// <summary>
        /// The ServerGraphInfo object that stores all server and graph informations.
        /// </summary>
        public ServerGraphInfo ServerGraph { get; set; }

        /// <summary>
        /// Serializes this message.
        /// </summary>
        /// <param name="serializer">Serializer used to serialize objects</param>
        public override void Serialize(INGRIDSerializer serializer)
        {
            base.Serialize(serializer);
            serializer.WriteObject(ServerGraph);
        }

        /// <summary>
        /// Deserializes this message.
        /// </summary>
        /// <param name="deserializer">Deserializer used to deserialize objects</param>
        public override void Deserialize(INGRIDDeserializer deserializer)
        {
            base.Deserialize(deserializer);
            ServerGraph = deserializer.ReadObject(() => new ServerGraphInfo());
        }
    }
}
