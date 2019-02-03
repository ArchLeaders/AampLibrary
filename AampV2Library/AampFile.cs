using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.Maths;

namespace AampV2Library
{
    public class AampFile
    {
        public AampFile(string FileName)
        {
            Read(new FileReader(new FileStream(FileName, FileMode.Open)));
        }

        public AampFile(Stream Stream)
        {
            Read(new FileReader(Stream));
        }

        public void Save(string FileName)
        {
            Write(new FileWriter(new FileStream(FileName, FileMode.Create, FileAccess.Write)));
        }

        public void Save(Stream Stream)
        {
            Write(new FileWriter(Stream));
        }

        public void ToJson(string FileName)
        {
            JsonConverter json = new JsonConverter();
            json.ToJson(this, FileName);
        }

        /// <summary>
        /// Gets or sets the effect name of the aamp/>
        /// </summary>
        public string ParameterIOType { get; set; }

        /// <summary>
        /// Gets or sets an unknown value/>
        /// </summary>
        public uint unknownValue { get; set; }

        /// <summary>
        /// Gets the endianness of the file>
        /// </summary>
        public uint Endianness { get; private set; }

        /// <summary>
        /// Gets the version of the file>
        /// </summary>
        public uint Version { get; set; }

        /// <summary>
        /// Gets the parameter io version of the file>
        /// </summary>
        public uint ParameterIOVersion { get; set; }

        /// <summary>
        /// Gets or sets the type of the aamp effect/>
        /// </summary>
        public string EffectType
        {
            get
            {
                if (ParameterIOType.Contains("aglenv")) return "Environment";
                else if (ParameterIOType.Contains("glpbd")) return "Probe Data";
                else if (ParameterIOType.Contains("genv")) return "Environment";
                else if (ParameterIOType.Contains("gsdw")) return "Shadow";
                else if (ParameterIOType.Contains("agllmap")) return "Light Map";
                else if (ParameterIOType.Contains("agldof")) return "Depth of Field";
                else if (ParameterIOType.Contains("aglfila")) return "AA Filter";
                else if (ParameterIOType.Contains("aglblm")) return "Bloom";
                else if (ParameterIOType.Contains("aglccr")) return "Color Correction";
                else if (ParameterIOType.Contains("aglcube")) return "Cube Map";
                else if (ParameterIOType.Contains("aglatex")) return "Auto Dxposure";
                else if (ParameterIOType.Contains("aglflr")) return "Flare Filter";
                else if (ParameterIOType.Contains("aglmf")) return "Multi Filter";
                else if (ParameterIOType.Contains("aglsdw")) return "Depth Shadow";
                else if (ParameterIOType.Contains("aglshpp")) return "Shadow Pre Pass";
                else if (ParameterIOType.Contains("aglofx")) return "Occluded Effect";
                else
                    return "";
            }
        }

        /// <summary>
        /// Gets the root of the params in the file>
        /// </summary>
        public ParamList RootNode { get; set; }

        internal void Read(FileReader reader)
        {
            reader.ByteOrder = Syroot.BinaryData.ByteOrder.LittleEndian;

            reader.CheckSignature("AAMP");
            Version = reader.ReadUInt32();
            Endianness = reader.ReadUInt32();
            //    reader.CheckByteOrderMark(Endianness);
            uint FileSize = reader.ReadUInt32();
            ParameterIOVersion = reader.ReadUInt32();
            uint ParameterIOOffset = reader.ReadUInt32();
            uint ListCount = reader.ReadUInt32();
            uint ObjectCount = reader.ReadUInt32();
            uint ParamCount = reader.ReadUInt32();
            uint DataSectionSize = reader.ReadUInt32();
            uint StringSectionSize = reader.ReadUInt32();
            uint UnkUintCount = reader.ReadUInt32();
            ParameterIOType = reader.ReadString((int)ParameterIOOffset, Encoding.Default);

            RootNode = new ParamList();
            RootNode.Read(reader);
        }

        internal void Write(FileWriter writer)
        {
            writer.ByteOrder = Syroot.BinaryData.ByteOrder.LittleEndian;

            writer.Write(Encoding.ASCII.GetBytes("AAMP"));
            writer.Write(Version);
            writer.Write(Endianness);

            long sizeOffset = writer.Position;
            writer.Write(0); //Write the file size later
            writer.Write(ParameterIOVersion);

            uint ListCount = GetListCount(RootNode);
            uint ObjectCount = GetObjectCount(RootNode);
            uint ParamCount = GetParamCount(RootNode);
            uint DataSectionSize = 0;
            uint StringSectionSize = 0;
            uint UnkUintCount = 0;

            writer.Write(ParameterIOType.Length);
            writer.Write(ListCount);
            writer.Write(ObjectCount);
            writer.Write(ParamCount);
            writer.Write(DataSectionSize);
            writer.Write(StringSectionSize);
            writer.Write(UnkUintCount);
            writer.Write(Encoding.Default.GetBytes(ParameterIOType));
            WriteLists(writer, RootNode);
            WriteObjects(writer, RootNode);
            WriteEntries(writer, RootNode);

            Console.WriteLine($"ListCount {ListCount}");
            Console.WriteLine($"ObjectCount {ObjectCount}");
            Console.WriteLine($"ParamCount {ParamCount}");
            Console.WriteLine($"ParameterIOType {ParameterIOType}");
            Console.WriteLine($"ParameterIOType Length {ParameterIOType.Length}");

            //Write end of file
            writer.Seek(sizeOffset, System.IO.SeekOrigin.Begin);
            uint FileSize = (uint)writer.BaseStream.Length;
            writer.Write(FileSize);

            writer.Close();
            writer.Dispose();
        }
        //Write the lists first before objects
        private void WriteLists(FileWriter writer, ParamList paramList)
        {
            int ChildListCount = paramList.childParams == null ? 0 : paramList.childParams.Length;
            int ParamObjectCount = paramList.paramObjects == null ? 0 : paramList.paramObjects.Length;

            long startPosition = writer.Position;
            writer.Write(paramList.Hash);
            writer.Write(ushort.MaxValue); //Write the offset after
            writer.Write((ushort)ChildListCount);
            writer.Write(ushort.MaxValue); //Write the offset after
            writer.Write((ushort)ParamObjectCount);

            for (int i = 0; i < ChildListCount; i++)
            {
                WriteLists(writer, paramList.childParams[i]);
            }
        }
        private void WriteObjects(FileWriter writer, ParamList paramList)
        {
            int ParamObjectCount = paramList.paramObjects == null ? 0 : paramList.paramObjects.Length;
            for (int i = 0; i < ParamObjectCount; i++)
            {
                WriteObject(writer, paramList.paramObjects[i]);
            }
        }
        private void WriteObject (FileWriter writer, ParamObject paramObj)
        {
            int EntryCount = paramObj.paramEntries == null ? 0 : paramObj.paramEntries.Length;

            long startPosition = writer.Position;

            writer.Write(paramObj.Hash);
            writer.Write(ushort.MaxValue); //Write the offset after
            writer.Write((ushort)EntryCount);
        }
        private void WriteEntries(FileWriter writer, ParamList paramList)
        {
            int ChildListCount = paramList.childParams == null ? 0 : paramList.childParams.Length;
            int ParamObjectCount = paramList.paramObjects == null ? 0 : paramList.paramObjects.Length;

            for (int i = 0; i < ChildListCount; i++)
            {
                WriteLists(writer, paramList.childParams[i]);
            }

            for (int i = 0; i < ParamObjectCount; i++)
            {
                int EntryCount = paramList.paramObjects[i].paramEntries == null ? 0 : paramList.paramObjects[i].paramEntries.Length;
                for (int obj = 0; obj < EntryCount; obj++)
                {
                    WriteParamEntry(writer, paramList.paramObjects[i].paramEntries[i]);
                }
            }
        }
        private void WriteParamEntry(FileWriter writer, ParamEntry entry)
        {
            long startPosition = writer.Position;
            writer.Write(entry.Hash);
            writer.Write(ushort.MaxValue); //Write the offset after
            writer.Write(byte.MaxValue);
            writer.Write(entry.ParamType, true);

            switch (entry.ParamType)
            {
                case ParamType.Boolean: writer.Write((bool)entry.Value == false ? (byte)0 : (byte)1); break;
                case ParamType.Float: writer.Write((float)entry.Value); break;
                case ParamType.Int: writer.Write((int)entry.Value); break;
                case ParamType.Vector2F: writer.WriteVector2F((Vector2F)entry.Value); break;
                case ParamType.Vector3F: writer.WriteVector3F((Vector3F)entry.Value); break;
                case ParamType.Vector4F: writer.WriteVector4F((Vector4F)entry.Value); break;
                case ParamType.Color4F: writer.WriteVector4F((Vector4F)entry.Value); break;
                case ParamType.Uint: writer.Write((uint)entry.Value); break;
                case ParamType.BufferUint: writer.Write((uint[])entry.Value); break;
                case ParamType.BufferInt: writer.Write((int[])entry.Value); break;
                case ParamType.BufferFloat: writer.Write((float[])entry.Value); break;
                case ParamType.BufferBinary: writer.Write((byte[])entry.Value); break;
                case ParamType.String64:
                case ParamType.String32:
                case ParamType.String256:
                case ParamType.StringRef:
                    writer.Write((string)entry.Value); break;
                case ParamType.Curve1:
                case ParamType.Curve2:
                case ParamType.Curve3:
                case ParamType.Curve4:
                    int curveAmount = entry.ParamType - ParamType.Curve1 + 1;

                    var curves = (Curve[])entry.Value;
                    for (int i = 0; i < curveAmount; i++)
                    {
                        writer.Write(curves[i].valueUints);
                        writer.Write(curves[i].valueFloats);
                    }
                    break;
                default:
                    throw new Exception("Unsupported param type! " + entry.ParamType);
            }
        }

        uint GetListCount(ParamList paramList, uint total = 0)
        {
            total += 1;
            foreach (var child in paramList.childParams)
                total += GetListCount(child, total);

            return total;
        }

        uint GetObjectCount(ParamList paramList, uint total = 0)
        {
            total += 1;
            foreach (var child in paramList.childParams)
                total += GetObjectCount(child, total);

            return total;
        }

        uint GetParamCount(ParamList paramList, uint total = 0)
        {
            foreach (var obj in paramList.paramObjects)
                total += (uint)obj.paramEntries.Length;

            foreach (var child in paramList.childParams)
                total += GetObjectCount(child, total);

            return total;
        }
    }
}
