using NGRID.Serialization;

namespace NGRID.Communication.Messages.ControllerMessages
{
    /// <summary>
    /// This message is response to NGRID Manager for GetApplicationWebServicesMessage.
    /// </summary>
    public class GetApplicationWebServicesResponseMessage : ControlMessage
    {
        /// <summary>
        /// Gets MessageTypeId for GetApplicationWebServicesMessage.
        /// </summary>
        public override int MessageTypeId
        {
            get { return ControlMessageFactory.MessageTypeIdGetApplicationWebServicesResponseMessage; }
        }

        /// <summary>
        /// True, if operation is success and no error occured.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Detailed information about operation result. Error text, if any error occured.
        /// </summary>
        public string ResultText { get; set; }

        /// <summary>
        /// Web service communicators of application.
        /// </summary>
        public ApplicationWebServiceInfo[] WebServices { get; set; }

        /// <summary>
        /// Serializes this message.
        /// </summary>
        /// <param name="serializer">Serializer used to serialize objects</param>
        public override void Serialize(INGRIDSerializer serializer)
        {
            base.Serialize(serializer);
            serializer.WriteBoolean(Success);
            serializer.WriteStringUTF8(ResultText);
            serializer.WriteObjectArray(WebServices);
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
            WebServices = deserializer.ReadObjectArray(() => new ApplicationWebServiceInfo());
        }
    }
}
