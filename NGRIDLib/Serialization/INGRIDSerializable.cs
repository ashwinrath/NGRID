namespace NGRID.Serialization
{
    /// <summary>
    /// This interface is implemented by all classes that can be serialized/deserialized by NGRID Serialization.
    /// </summary>
    public interface INGRIDSerializable
    {
        /// <summary>
        /// This method serializes the object.
        /// </summary>
        /// <param name="serializer">Used to serialize object</param>
        void Serialize(INGRIDSerializer serializer);

        /// <summary>
        /// This method deserializes the object.
        /// </summary>
        /// <param name="deserializer">Used to deserialize object</param>
        void Deserialize(INGRIDDeserializer deserializer);
    }
}
