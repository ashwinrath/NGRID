using NGRID.Serialization;

namespace NGRID.Communication.Messages.ControllerMessages
{
    /// <summary>
    /// This message is used to get web services informations of an application from NGRID server.
    /// </summary>
    public class GetApplicationWebServicesMessage : ControlMessage
    {
        /// <summary>
        /// Gets MessageTypeId for GetApplicationWebServicesMessage.
        /// </summary>
        public override int MessageTypeId
        {
            get { return ControlMessageFactory.MessageTypeIdGetApplicationWebServicesMessage; }
        }

        /// <summary>
        /// Name of the application to get web service information.
        /// </summary>
        public string ApplicationName { get; set; }
        
        /// <summary>
        /// Serializes this message.
        /// </summary>
        /// <param name="serializer">Serializer used to serialize objects</param>
        public override void Serialize(INGRIDSerializer serializer)
        {
            base.Serialize(serializer);
            serializer.WriteStringUTF8(ApplicationName);
        }

        /// <summary>
        /// Deserializes this message.
        /// </summary>
        /// <param name="deserializer">Deserializer used to deserialize objects</param>
        public override void Deserialize(INGRIDDeserializer deserializer)
        {
            base.Deserialize(deserializer);
            ApplicationName = deserializer.ReadStringUTF8();
        }
    }
}
