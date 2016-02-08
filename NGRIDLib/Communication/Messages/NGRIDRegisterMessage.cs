using NGRID.Serialization;

namespace NGRID.Communication.Messages
{
    /// <summary>
    /// Register Message. A NGRIDRegisterMessage object is used to register a NGRID server as an Application or NGRID server.
    /// </summary>
    public class NGRIDRegisterMessage : NGRIDMessage
    {
        /// <summary>
        /// MessageTypeId of message.
        /// It is used to serialize/deserialize message.
        /// </summary>
        public override int MessageTypeId
        {
            get { return NGRIDMessageFactory.MessageTypeIdNGRIDRegisterMessage; }
        }

        /// <summary>
        /// Communicator type (NGRID server, Application or Controller). 
        /// </summary>
        public CommunicatorTypes CommunicatorType { get; set; }

        /// <summary>
        /// Communication way for this communicator (SEND, RECEIVE or BOTH)
        /// </summary>
        public CommunicationWays CommunicationWay { get; set; }

        /// <summary>
        /// Name of the communicator. 
        /// If CommunicatorType is a NGRID, than this is server's name,
        /// if CommunicatorType is an Application, than this is application's name,
        /// if CommunicatorType is a Controller, than this is an arbitrary string represents controller.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Password to connect to NGRID associated with Name and CommunicatorType.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Creates a new NGRIDRegisterMessage object.
        /// </summary>
        public NGRIDRegisterMessage()
        {
            CommunicationWay = CommunicationWays.Send;
        }

        /// <summary>
        /// Serializes this message.
        /// </summary>
        /// <param name="serializer">Serializer used to serialize objects</param>
        public override void Serialize(INGRIDSerializer serializer)
        {
            base.Serialize(serializer);
            serializer.WriteByte((byte)CommunicatorType);
            serializer.WriteByte((byte)CommunicationWay);
            serializer.WriteStringUTF8(Name);
            serializer.WriteStringUTF8(Password);
        }

        /// <summary>
        /// Deserializes this message.
        /// </summary>
        /// <param name="deserializer">Deserializer used to deserialize objects</param>
        public override void Deserialize(INGRIDDeserializer deserializer)
        {
            base.Deserialize(deserializer);
            CommunicatorType = (CommunicatorTypes) deserializer.ReadByte();
            CommunicationWay = (CommunicationWays) deserializer.ReadByte();
            Name = deserializer.ReadStringUTF8();
            Password = deserializer.ReadStringUTF8();
        }
    }
}
