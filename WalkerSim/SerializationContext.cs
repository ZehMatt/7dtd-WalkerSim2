using System;
using System.IO;

namespace WalkerSim
{
    internal class SerializationContext
    {
        private readonly BinaryWriter _writer;
        private readonly BinaryReader _reader;
        private readonly bool _isReading;

        private SerializationContext(BinaryWriter writer)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _isReading = false;
        }

        private SerializationContext(BinaryReader reader)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _isReading = true;
        }

        public static SerializationContext CreateWriter(BinaryWriter writer) => new SerializationContext(writer);
        public static SerializationContext CreateReader(BinaryReader reader) => new SerializationContext(reader);

        public bool IsReading => _isReading;
        public bool IsWriting => !_isReading;

        // Primitive types
        public void Serialize(ref byte value)
        {
            if (_isReading)
                value = _reader.ReadByte();
            else
                _writer.Write(value);
        }

        public void Serialize(ref int value, bool compressed = true)
        {
            if (_isReading)
                value = Serialization.ReadInt32(_reader, compressed);
            else
                Serialization.WriteInt32(_writer, value, compressed);
        }

        public void Serialize(ref uint value, bool compressed = true)
        {
            if (_isReading)
                value = Serialization.ReadUInt32(_reader, compressed);
            else
                Serialization.WriteUInt32(_writer, value, compressed);
        }

        public void Serialize(ref ulong value, bool compressed = true)
        {
            if (_isReading)
                value = Serialization.ReadUInt64(_reader, compressed);
            else
                Serialization.WriteUInt64(_writer, value, compressed);
        }

        public void Serialize(ref float value, bool compressed = true)
        {
            if (_isReading)
                value = Serialization.ReadSingle(_reader, compressed);
            else
                Serialization.WriteSingle(_writer, value, compressed);
        }

        public void Serialize(ref string value, bool compressed = true)
        {
            if (_isReading)
                value = Serialization.ReadStringUTF8(_reader, compressed);
            else
                Serialization.WriteStringUTF8(_writer, value, compressed);
        }

        public void Serialize(ref Vector3 value, bool compressed = true)
        {
            if (_isReading)
                value = Serialization.ReadVector3(_reader, compressed);
            else
                Serialization.WriteVector3(_writer, value, compressed);
        }

        public void SerializeEnum<T>(ref T value) where T : struct, Enum
        {
            int intValue = _isReading ? 0 : Convert.ToInt32(value);
            Serialize(ref intValue);
            if (_isReading)
                value = (T)Enum.ToObject(typeof(T), intValue);
        }

        public void SerializeEnumByte<T>(ref T value) where T : struct, Enum
        {
            byte byteValue = _isReading ? (byte)0 : Convert.ToByte(value);
            Serialize(ref byteValue);
            if (_isReading)
                value = (T)Enum.ToObject(typeof(T), byteValue);
        }
    }
}
