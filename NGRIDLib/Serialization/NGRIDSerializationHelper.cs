using System.IO;

namespace NGRID.Serialization
{
    /// <summary>
    /// This class is used to simplify serialization/deserialization with NGRID serialization classes.
    /// </summary>
    public static class NGRIDSerializationHelper
    {
        /// <summary>
        /// Serializes an object that implements INGRIDSerializable and returns serialized byte array.
        /// </summary>
        /// <param name="serializableObject">Object to serialize</param>
        /// <returns>Serialized object as byte array</returns>
        public static byte[] SerializeToByteArray(INGRIDSerializable serializableObject)
        {
            var stream = new MemoryStream();
            new NGRIDDefaultSerializer(stream).WriteObject(serializableObject);
            return stream.ToArray();
        }

        /// <summary>
        /// Serializes an object that implements INGRIDSerializable to a Stream.
        /// </summary>
        /// <param name="stream">Stream to write serialized object</param>
        /// <param name="serializableObject">Object to serialize</param>
        public static void SerializeToStream(Stream stream, INGRIDSerializable serializableObject)
        {
            new NGRIDDefaultSerializer(stream).WriteObject(serializableObject);
        }

        /// <summary>
        /// Deserializes an object from a byte array.
        /// </summary>
        /// <typeparam name="T">Type of object. This type must implement INGRIDSerializable interface</typeparam>
        /// <param name="createObjectHandler">A function that creates an instance of that object (T)</param>
        /// <param name="bytesOfObject">Byte array</param>
        /// <returns>Deserialized object</returns>
        public static T DeserializeFromByteArray<T>(CreateSerializableObjectHandler<T> createObjectHandler, byte[] bytesOfObject) where T : INGRIDSerializable
        {
            return new NGRIDDefaultDeserializer(new MemoryStream(bytesOfObject) {Position = 0}).ReadObject(createObjectHandler);
        }

        /// <summary>
        /// Deserializes an object via reading from a stream.
        /// </summary>
        /// <typeparam name="T">Type of object. This type must implement INGRIDSerializable interface</typeparam>
        /// <param name="createObjectHandler">A function that creates an instance of that object (T)</param>
        /// <param name="stream">Deserialized object</param>
        /// <returns>Deserialized object</returns>
        public static T DeserializeFromStream<T>(CreateSerializableObjectHandler<T> createObjectHandler, Stream stream) where T : INGRIDSerializable
        {
            return new NGRIDDefaultDeserializer(stream).ReadObject(createObjectHandler);
        }
    }
}
