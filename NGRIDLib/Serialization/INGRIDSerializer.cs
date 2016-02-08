using System;

namespace NGRID.Serialization
{
    /// <summary>
    /// This interface is used to serialize primitives and objects.
    /// Only needed Write methods designed for NGRID.
    /// </summary>
    public interface INGRIDSerializer
    {
        /// <summary>
        /// Serializes a byte.
        /// </summary>
        /// <param name="b">byte to serialize</param>
        void WriteByte(byte b);

        /// <summary>
        /// Writes a byte array to serialization stream.
        /// Byte array may be null or empty.
        /// </summary>
        /// <param name="bytes">byte array to write</param>
        void WriteByteArray(byte[] bytes);

        /// <summary>
        /// Serializes an integer.
        /// </summary>
        /// <param name="number">integer to serialize</param>
        void WriteInt32(int number);

        /// <summary>
        /// Serializes an unsigned integer.
        /// </summary>
        /// <param name="number">unsigned integer to serialize</param>
        void WriteUInt32(uint number);

        /// <summary>
        /// Serializes a long.
        /// </summary>
        /// <param name="number">long to serialize</param>
        void WriteInt64(long number);

        /// <summary>
        /// Serializes a boolean.
        /// </summary>
        /// <param name="b">boolean to serialize</param>
        void WriteBoolean(bool b);

        /// <summary>
        /// Serializes a DateTime object.
        /// </summary>
        /// <param name="dateTime">DateTime to serialize</param>
        void WriteDateTime(DateTime dateTime);

        /// <summary>
        /// Serializes a char according to UTF8.
        /// Char may be null or empty.
        /// </summary>
        /// <param name="c">char to serialize</param>
        void WriteCharUTF8(char c);

        /// <summary>
        /// Serializes a string according to UTF8.
        /// String may be null or empty.
        /// </summary>
        /// <param name="text">string to serialize</param>
        void WriteStringUTF8(string text);

        /// <summary>
        /// Serializes an object that implements INGRIDSerializable interface.
        /// Object may be null.
        /// </summary>
        /// <param name="serializableObject">object to serialize</param>
        void WriteObject(INGRIDSerializable serializableObject);

        /// <summary>
        /// Serializes an array that all items implements INGRIDSerializable interface.
        /// Object array may be null or empty.
        /// </summary>
        /// <param name="serializableObjects">objects to serialize</param>
        void WriteObjectArray(INGRIDSerializable[] serializableObjects);
    }
}
