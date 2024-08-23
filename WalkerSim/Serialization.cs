using System;
using System.IO;

namespace WalkerSim
{
    internal static class Serialization
    {
        public static void WriteUInt32(BinaryWriter writer, UInt32 value, bool compressed = true)
        {
            // TODO: Variable length integer encoding.
            writer.Write(value);
        }

        public static UInt32 ReadUInt32(BinaryReader reader, bool compressed = true)
        {
            // TODO: Variable length integer encoding.
            return reader.ReadUInt32();
        }

        public static void WriteInt32(BinaryWriter writer, Int32 value, bool compressed = true)
        {
            // TODO: Variable length integer encoding.
            writer.Write(value);
        }

        public static Int32 ReadInt32(BinaryReader reader, bool compressed = true)
        {
            // TODO: Variable length integer encoding.
            return reader.ReadInt32();
        }

        public static void WriteSingle(BinaryWriter writer, Single value, bool compressed = true)
        {
            writer.Write(value);
        }

        public static Single ReadSingle(BinaryReader reader, bool compressed = true)
        {
            return reader.ReadSingle();
        }

        public static void WriteInt32Array(BinaryWriter writer, Int32[] values, bool compressed = true)
        {
            WriteInt32(writer, values.Length, compressed);
            foreach (var value in values)
            {
                WriteInt32(writer, value, compressed);
            }
        }

        public static Int32[] ReadInt32Array(BinaryReader reader, bool compressed = true)
        {
            var count = ReadInt32(reader, compressed);
            var arr = new Int32[count];
            for (var i = 0; i < count; i++)
            {
                arr[i] = ReadInt32(reader, compressed);
            }
            return arr;
        }

        public static void WriteStringUTF8(BinaryWriter writer, string value, bool compressed = true)
        {
            var data = System.Text.Encoding.UTF8.GetBytes(value);
            WriteInt32(writer, data.Length, compressed);
            writer.Write(data);
        }

        public static string ReadStringUTF8(BinaryReader reader, bool compressed = true)
        {
            var length = ReadInt32(reader, compressed);
            var utf8Bytes = reader.ReadBytes(length);
            return System.Text.Encoding.UTF8.GetString(utf8Bytes);
        }

        public static void WriteVector3(BinaryWriter writer, Vector3 value, bool compressed = true)
        {
            WriteSingle(writer, value.X, compressed);
            WriteSingle(writer, value.Y, compressed);
            WriteSingle(writer, value.Z, compressed);
        }

        public static Vector3 ReadVector3(BinaryReader reader, bool compressed = true)
        {
            return new Vector3
            {
                X = ReadSingle(reader, compressed),
                Y = ReadSingle(reader, compressed),
                Z = ReadSingle(reader, compressed)
            };
        }
    }
}
