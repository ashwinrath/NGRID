using System;
using NGRID.Serialization;

namespace NGRID.Communication.Messages
{
    /// <summary>
    /// This class is used to store transmit informations of a message throught a server.
    /// </summary>
    public class ServerTransmitReport : INGRIDSerializable
    {
        /// <summary>
        /// Name of the server.
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// Message arriving time to server.
        /// </summary>
        public DateTime ArrivingTime { get; set; }

        /// <summary>
        /// Message leaving time from server.
        /// </summary>
        public DateTime LeavingTime { get; set; }

        /// <summary>
        /// Creates a new ServerTransmitReport.
        /// </summary>
        public ServerTransmitReport()
        {
            ArrivingTime = DateTime.MinValue;
            LeavingTime = DateTime.MinValue;
        }

        /// <summary>
        /// Serializes this object.
        /// </summary>
        /// <param name="serializer">Serializer used to serialize objects</param>
        public void Serialize(INGRIDSerializer serializer)
        {
            serializer.WriteStringUTF8(ServerName);
            serializer.WriteDateTime(ArrivingTime);
            serializer.WriteDateTime(LeavingTime);
        }

        /// <summary>
        /// Deserializes this object.
        /// </summary>
        /// <param name="deserializer">Deserializer used to deserialize objects</param>
        public void Deserialize(INGRIDDeserializer deserializer)
        {
            ServerName = deserializer.ReadStringUTF8();
            ArrivingTime = deserializer.ReadDateTime();
            LeavingTime = deserializer.ReadDateTime();
        }
    }
}
