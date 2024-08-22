using System;
using System.IO;

namespace WalkerSim
{
    internal static class Serialization
    {
        public static void WriteUInt32(BinaryWriter writer, UInt32 value)
        {
            // TODO: Variable length integer encoding.
            writer.Write(value);
        }

        public static UInt32 ReadUInt32(BinaryReader reader)
        {
            // TODO: Variable length integer encoding.
            return reader.ReadUInt32();
        }

        public static void WriteInt32(BinaryWriter writer, Int32 value)
        {
            // TODO: Variable length integer encoding.
            writer.Write(value);
        }

        public static Int32 ReadInt32(BinaryReader reader)
        {
            // TODO: Variable length integer encoding.
            return reader.ReadInt32();
        }

        public static void WriteSingle(BinaryWriter writer, Single value)
        {
            writer.Write(value);
        }

        public static Single ReadSingle(BinaryReader reader)
        {
            return reader.ReadSingle();
        }

        public static void WriteInt32Array(BinaryWriter writer, Int32[] values)
        {
            WriteInt32(writer, values.Length);
            foreach (var value in values)
            {
                WriteInt32(writer, value);
            }
        }

        public static Int32[] ReadInt32Array(BinaryReader reader)
        {
            var count = ReadInt32(reader);
            var arr = new Int32[count];
            for (var i = 0; i < count; i++)
            {
                arr[i] = ReadInt32(reader);
            }
            return arr;
        }

        public static void WriteStringUTF8(BinaryWriter writer, string value)
        {
            var data = System.Text.Encoding.UTF8.GetBytes(value);
            WriteInt32(writer, data.Length);
            writer.Write(data);
        }

        public static string ReadStringUTF8(BinaryReader reader)
        {
            var length = ReadInt32(reader);
            var utf8Bytes = reader.ReadBytes(length);
            return System.Text.Encoding.UTF8.GetString(utf8Bytes);
        }

        public static void WriteVector3(BinaryWriter writer, Vector3 value)
        {
            WriteSingle(writer, value.X);
            WriteSingle(writer, value.Y);
            WriteSingle(writer, value.Z);
        }

        public static Vector3 ReadVector3(BinaryReader reader)
        {
            return new Vector3
            {
                X = ReadSingle(reader),
                Y = ReadSingle(reader),
                Z = ReadSingle(reader)
            };
        }
    }
}
