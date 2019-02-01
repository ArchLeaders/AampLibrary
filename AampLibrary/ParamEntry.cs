using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Syroot.Maths;
using Syroot.BinaryData;

namespace AampLibrary
{
    public class ParamEntry
    {
        /// <summary>
        /// Gets the hash of the object name>
        /// </summary>
        public uint Hash;

        /// <summary>
        /// Gets the hash converted string of the object name>
        /// </summary>
        public string HashString
        {
            get
            {
                return Hashes.GetName(Hash);
            }
        }

        /// <summary>
        /// Gets the ParamType of the entry>
        /// </summary>
        public ParamType ParamType;

        /// <summary>
        /// Gets the param object list>
        /// </summary>
        public ParamEntry[] paramEntries;

        /// <summary>
        /// Gets or sets the value of the data>
        /// </summary>
        public object Value = null;

        public void Read(FileReader reader)
        {
            long CurrentPosition = reader.Position;

            uint Size = reader.ReadUInt32();
            ParamType = reader.ReadEnum<ParamType>(true);
            Hash = reader.ReadUInt32();

            int DataSize = (int)Size - 12;

            switch (ParamType)
            {
                case ParamType.Boolean: Value = reader.ReadBoolean(); break;
                case ParamType.Float: Value = reader.ReadSingle(); break;
                case ParamType.Int: Value = reader.ReadInt32(); break;
                case ParamType.Vector2F: Value = reader.ReadVector2F(); break;
                case ParamType.Vector3F: Value = reader.ReadVector3F(); break;
                case ParamType.Vector4F: Value = reader.ReadVector4F(); break;
                case ParamType.Color4F: Value = reader.ReadVector4F(); break;
                case ParamType.Uint: Value = reader.ReadUInt32(); break;
                case ParamType.BufferUint: Value = reader.ReadUInt32s(DataSize / sizeof(uint)); break;
                case ParamType.BufferInt: Value = reader.ReadInt32s(DataSize / sizeof(int)); break;
                case ParamType.BufferFloat: Value = reader.ReadSingles(DataSize / sizeof(float)); break;
                case ParamType.BufferBinary: Value = reader.ReadBytes(DataSize); break;
                case ParamType.String64: Value = reader.ReadString(64); break;
                case ParamType.String32: Value = reader.ReadString(32); break;
                case ParamType.String256: Value = reader.ReadString(256); break;
                case ParamType.StringRef: Value = reader.ReadString(8); break;
                default:
                    Value = reader.ReadBytes(DataSize);
                    break;
            }

            reader.Seek(CurrentPosition + Size, SeekOrigin.Begin);
        }

        public void Write(FileWriter writer)
        {
            long startPosition = writer.Position;
            writer.Write(uint.MaxValue); //Write the size after
            writer.Write(ParamType, true);
            writer.Write(Hash);

            switch (ParamType)
            {
                case ParamType.Boolean: writer.WriteBoolean((bool)Value); break;
                case ParamType.Float: writer.Write((float)Value); break;
                case ParamType.Int: writer.Write((int)Value); break;
                case ParamType.Vector2F: writer.WriteVector2F((Vector2F)Value); break;
                case ParamType.Vector3F: writer.WriteVector3F((Vector3F)Value); break;
                case ParamType.Vector4F: writer.WriteVector4F((Vector4F)Value); break;
                case ParamType.Color4F: writer.WriteVector4F((Vector4F)Value); break;
                case ParamType.Uint: writer.Write((uint)Value); break;
                case ParamType.BufferUint: writer.Write((uint[])Value); break;
                case ParamType.BufferInt: writer.Write((int[])Value); break;
                case ParamType.BufferFloat: writer.Write((float[])Value); break;
                case ParamType.BufferBinary: writer.Write((byte[])Value); break;
                case ParamType.String64:
                case ParamType.String32:
                case ParamType.String256:
                case ParamType.StringRef:
                    writer.Write((string)Value); break;
                default:
                    writer.Write((byte[])Value);
                    break;
            }

            writer.WriteSize(writer.Position, startPosition);
        }
    }
}
