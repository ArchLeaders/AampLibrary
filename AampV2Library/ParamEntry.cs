using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Syroot.Maths;
using Syroot.BinaryData;

namespace AampV2Library
{
    public class ParamEntry
    {
        /// <summary>
        /// Gets the hash of the object name>
        /// </summary>
        public uint Hash { get; set; }

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
        public ParamType ParamType { get; set; }

        /// <summary>
        /// Gets the param object list>
        /// </summary>
        public ParamEntry[] paramEntries { get; set; }

        /// <summary>
        /// Gets or sets the value of the data>
        /// </summary>
        public object Value { get; set; }

        internal void Read(FileReader reader)
        {
            long CurrentPosition = reader.Position;

            Hash = reader.ReadUInt32();
            int field4 = reader.ReadInt32();
            int DataOffset = (field4 & 0xffffff);
            var type = (field4 >> 24);

            ParamType = (ParamType)type;

            int DataSize = 12;
            if (DataOffset != 0)
            {
                using (reader.TemporarySeek(DataOffset * 4 + CurrentPosition, SeekOrigin.Begin))
                {
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
                        case ParamType.BufferUint:
                            reader.Seek(-4, SeekOrigin.Current);
                            uint countUInt = reader.ReadUInt32();
                            Console.WriteLine($"countUInt {countUInt}");
                            Value = reader.ReadUInt32s((int)countUInt);
                            break;
                        case ParamType.BufferInt:
                            reader.Seek(-4, SeekOrigin.Current);
                            uint countInt = reader.ReadUInt32();
                            Console.WriteLine($"countInt {countInt}");
                            Value = reader.ReadInt32s((int)countInt);
                            break;
                        case ParamType.BufferFloat:
                            reader.Seek(-4, SeekOrigin.Current);
                            uint countF = reader.ReadUInt32();
                            Console.WriteLine($"countF {countF}");
                            Value = reader.ReadSingles((int)countF);
                            break;
                        case ParamType.BufferBinary:
                            reader.Seek(-4, SeekOrigin.Current);
                            uint countBin = reader.ReadUInt32();
                            Value = reader.ReadBytes((int)countBin);
                            break;
                        case ParamType.String64: Value = reader.ReadString(64); break;
                        case ParamType.String32: Value = reader.ReadString(32); break;
                        case ParamType.String256: Value = reader.ReadString(256); break;
                        case ParamType.StringRef: Value = reader.ReadString(8); break;
                        case ParamType.Curve1:
                        case ParamType.Curve2:
                        case ParamType.Curve3:
                        case ParamType.Curve4:
                            int curveAmount = ParamType - ParamType.Curve1 + 1;

                            var curves = new Curve[curveAmount];
                            Value = curves;

                            for (int i = 0; i < curveAmount; i++)
                            {
                                curves[i] = new Curve();
                                curves[i].valueUints = reader.ReadUInt32s(2);
                                curves[i].valueFloats = reader.ReadSingles(30);
                            }
                            break;
                        default:
                            throw new Exception("Unsupported param type! " + ParamType);
                    }
                }
            }
        }
    }
}
