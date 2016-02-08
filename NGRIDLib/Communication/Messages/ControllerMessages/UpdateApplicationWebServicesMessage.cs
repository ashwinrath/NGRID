using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGRID.Serialization;

namespace NGRID.Communication.Messages.ControllerMessages
{
    public class UpdateApplicationWebServicesMessage : ControlMessage
    {
        /// <summary>
        /// Gets MessageTypeId for GetApplicationWebServicesMessage.
        /// </summary>
        public override int MessageTypeId
        {
            get { return ControlMessageFactory.MessageTypeIdUpdateApplicationWebServicesMessage; }
        }

        /// <summary>
        /// Name of the application to get web service information.
        /// </summary>
        public string ApplicationName { get; set; }

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
            serializer.WriteStringUTF8(ApplicationName);
            serializer.WriteObjectArray(WebServices);
        }

        /// <summary>
        /// Deserializes this message.
        /// </summary>
        /// <param name="deserializer">Deserializer used to deserialize objects</param>
        public override void Deserialize(INGRIDDeserializer deserializer)
        {
            base.Deserialize(deserializer);
            ApplicationName = deserializer.ReadStringUTF8();
            WebServices = deserializer.ReadObjectArray(() => new ApplicationWebServiceInfo());
        }
    }
}
