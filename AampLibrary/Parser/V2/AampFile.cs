#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).

using AampLibrary.IO;
using System.Text;
using Syroot.Maths;

namespace AampLibrary
{
    internal class AampFileV2 : AampFile
    {
        public AampFileV2()
        {
            Version = 2;
            Endianness = 0;
        }
        public AampFileV2(string FileName) => Read(new FileReader(new FileStream(FileName, FileMode.Open)));
        public AampFileV2(Stream Stream) => Read(new FileReader(Stream));
        public override void Save(string FileName) => Write(new FileWriter(new FileStream(FileName, FileMode.Create, FileAccess.Write)));
        public override void Save(Stream Stream) => Write(new FileWriter(Stream));

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
            long pos = reader.Position;
            ParameterIOType = reader.ReadString(Syroot.BinaryData.BinaryStringFormat.ZeroTerminated, Encoding.UTF8);
            reader.Seek(pos + ParameterIOOffset, SeekOrigin.Begin);

            RootNode = ParamListV2.Read(reader);
        }

        private static bool IsBuffer(ParamType type)
        {
            return type switch
            {
                ParamType.BufferUint or ParamType.BufferInt or ParamType.BufferFloat or ParamType.BufferBinary => true,
                _ => false,
            };
        }

        private uint TotalListCount = 0;
        private uint TotalObjCount = 0;
        private uint TotalParamCount = 0;

        private static int AlignUp(int n, int align) => (n + align - 1) & -align;
        internal void Write(FileWriter writer)
        {
            _savedParamObjects.Clear();
            _savedParamLists.Clear();
            DataValues.Clear();
            StringValues.Clear();
            ObjListOffsets.Clear();
            ListOffsets.Clear();

            writer.ByteOrder = Syroot.BinaryData.ByteOrder.LittleEndian;

            writer.Write(Encoding.ASCII.GetBytes("AAMP"));
            writer.Write(Version);
            writer.Write(Endianness);

            long sizeOffset = writer.Position;
            writer.Write(0); //Write the file size later
            writer.Write(ParameterIOVersion);

            uint DataSectionSize = 0;
            uint StringSectionSize = 0;
            uint UnkUintCount = 0;

            writer.Write(AlignUp(ParameterIOType.Length + 1, 4));
            long totalCountOffset = writer.Position;
            writer.Write(TotalListCount);
            writer.Write(TotalObjCount);
            writer.Write(TotalParamCount);
            writer.Write(DataSectionSize);
            writer.Write(StringSectionSize);
            writer.Write(UnkUintCount);
            writer.Write(ParameterIOType, Syroot.BinaryData.BinaryStringFormat.ZeroTerminated);
            writer.Align(4);

            //Write the lists
            WriteList(writer, RootNode);

            //Save the data and offsets for lists
            for (int index = 0; index < _savedParamLists.Count; index++)
            {
                WriteListData(writer, _savedParamLists[index], ListOffsets[index]);
            }

            //Save objects from lists
            for (int index = 0; index < _savedParamLists.Count; index++)
            {
                ListOffsets[index][1].WriteOffsetU16(writer, (uint)writer.Position);
                foreach (var obj in _savedParamLists[index].ParamObjects)
                    WriteObject(writer, obj);
            }

            while (_savedParamObjects.Count != 0)
            {
                WriteObjectData(writer, PopAt(_savedParamObjects, 0));
            }

            uint DataStart = (uint)writer.Position;
            foreach (var entry in DataValues)
            {
                foreach (var offset in entry.Value)
                {
                    if (IsBuffer(((ParamEntry)offset.data).ParamType))
                    {
                        writer.Write(0); //Save offset after the size of buffer
                        offset.WriteOffsetU24(writer, (uint)writer.Position, (ParamEntry)offset.data);
                        writer.Seek(-4, SeekOrigin.Current);
                    }
                    else
                        offset.WriteOffsetU24(writer, (uint)writer.Position, (ParamEntry)offset.data);
                }

                writer.Write(entry.Key);
                writer.Align(4);
            }

            uint DataEnd = (uint)writer.Position;
            uint StringDataStart = (uint)writer.Position;

            int stringCount = 0;
            foreach (var entry in StringValues)
            {
                foreach (var offset in entry.Value)
                {
                    offset.WriteOffsetU24(writer, (uint)writer.Position, (ParamEntry)offset.data);
                    stringCount++;
                }

                writer.Write(entry.Key);

                do
                    writer.Write((byte)0);
                while (writer.Position % 4 != 0);
            }
            uint StringDataEnd = (uint)writer.Position;

            //Write data size
            writer.Seek(totalCountOffset, System.IO.SeekOrigin.Begin);

            writer.Write(TotalListCount);
            writer.Write(TotalObjCount);
            writer.Write(TotalParamCount);
            writer.Write(DataEnd - DataStart);
            writer.Write(StringDataEnd - StringDataStart);

            //Write end of file
            writer.Seek(sizeOffset, System.IO.SeekOrigin.Begin);
            uint FileSize = (uint)writer.BaseStream.Length;
            writer.Write(FileSize);

            writer.Close();
            writer.Dispose();
        }

        public static List<T> Rotate<T>(List<T> list, int offset) => list.Skip(offset).Concat(list.Take(offset)).ToList();
        public static T PopAt<T>(List<T> list, int index)
        {
            T r = list[index];
            list.RemoveAt(index);
            return r;
        }

        private class ObjectContext
        {
            public PlaceholderOffet? PlaceholderOffet;
            public ParamObject? ParamObject;
        }

        private List<ParamList> _savedParamLists = new List<ParamList>();
        private List<ObjectContext> _savedParamObjects = new List<ObjectContext>();
        private List<ParamEntry> _savedParamEntries = new List<ParamEntry>();

        private List<PlaceholderOffet[]> ListOffsets = new List<PlaceholderOffet[]>();
        private List<PlaceholderOffet> ObjListOffsets = new List<PlaceholderOffet>();
        private List<PlaceholderOffet> ParamEntryOffsets = new List<PlaceholderOffet>();

        public class PlaceholderOffet
        {
            public object data;
            public long BasePosition;
            public long OffsetPosition;

            public void WritePlaceholderU16(FileWriter writer)
            {
                writer.Write(ushort.MaxValue);
            }

            public void WritePlaceholderU24(FileWriter writer)
            {
                writer.Write(byte.MaxValue);
                writer.Write(byte.MaxValue);
                writer.Write(byte.MaxValue);
            }

            public void WritePlaceholderU32(FileWriter writer)
            {
                writer.Write(uint.MaxValue);
            }

            public void WritePlaceholderU64(FileWriter writer)
            {
                writer.Write(ulong.MaxValue);
            }

            public void WriteOffsetU24(FileWriter writer, uint Offset, ParamEntry entry)
            {
                using (writer.TemporarySeek(OffsetPosition, SeekOrigin.Begin))
                {
                    uint ValuePacked = 0;
                    writer.Write((uint)((ValuePacked << 24) | ((Offset - BasePosition) >> 2)));

                    writer.Seek(OffsetPosition + 3, SeekOrigin.Begin);
                    writer.Write((byte)entry.ParamType);
                }
            }

            public void WriteOffsetU16(FileWriter writer, uint Offset)
            {
                using (writer.TemporarySeek(OffsetPosition, SeekOrigin.Begin))
                {
                    writer.Write((ushort)((Offset - BasePosition) >> 2));
                }
            }
        }

        private PlaceholderOffet WritePlaceholderOffsetU24(FileWriter writer, long BasePosition)
        {
            PlaceholderOffet offset = new PlaceholderOffet();
            offset.OffsetPosition = writer.Position;
            offset.BasePosition = BasePosition;
            offset.WritePlaceholderU24(writer);
            return offset;
        }

        private PlaceholderOffet WritePlaceholderOffsetU16(FileWriter writer, long BasePosition)
        {
            PlaceholderOffet offset = new PlaceholderOffet();
            offset.OffsetPosition = writer.Position;
            offset.BasePosition = BasePosition;
            offset.WritePlaceholderU16(writer);
            return offset;
        }

        private void WriteList(FileWriter writer, ParamList paramList)
        {
            TotalListCount += 1;
            _savedParamLists.Add(paramList);

            ushort ChildListCount = paramList.ChildParams == null ? (ushort)0 : (ushort)paramList.ChildParams.Length;
            ushort ParamObjectCount = paramList.ParamObjects == null ? (ushort)0 : (ushort)paramList.ParamObjects.Length;

            long pos = writer.Position;
            writer.Write(paramList.Hash);
            var listEntry = WritePlaceholderOffsetU16(writer, pos);
            writer.Write(ChildListCount);
            var objectEntry = WritePlaceholderOffsetU16(writer, pos);
            writer.Write(ParamObjectCount);

            ListOffsets.Add(new PlaceholderOffet[] { listEntry, objectEntry });
        }

        private void WriteListData(FileWriter writer, ParamList paramList, PlaceholderOffet[] offsets)
        {
            offsets[0].WriteOffsetU16(writer, (uint)writer.Position);
            foreach (var child in paramList.ChildParams)
                WriteList(writer, child);
        }

        private void WriteObject(FileWriter writer, ParamObject paramObj)
        {
            TotalObjCount += 1;
            int EntryCount = paramObj.ParamEntries == null ? 0 : paramObj.ParamEntries.Length;

            long startPosition = writer.Position;

            writer.Write(paramObj.Hash);
            var paramEntry = WritePlaceholderOffsetU16(writer, startPosition);
            writer.Write((ushort)EntryCount);

            _savedParamObjects.Add(new ObjectContext()
            {
                ParamObject = paramObj,
                PlaceholderOffet = paramEntry,
            });
        }

        private Dictionary<byte[], List<PlaceholderOffet>> DataValues = new Dictionary<byte[], List<PlaceholderOffet>>(new ByteArrayComparer());
        private Dictionary<byte[], List<PlaceholderOffet>> StringValues = new Dictionary<byte[], List<PlaceholderOffet>>(new ByteArrayComparer());

        public class ByteArrayComparer : IEqualityComparer<byte[]>
        {
            public bool Equals(byte[] left, byte[] right)
            {
                if (left == null || right == null)
                    return left == right;

                return left.SequenceEqual(right);
            }

            public int GetHashCode(byte[] key)
            {
                if (key == null)
                    throw new ArgumentNullException("key");
                return key.Sum(b => b);
            }
        }

        private void WriteObjectData(FileWriter writer, ObjectContext context)
        {
            context.PlaceholderOffet.WriteOffsetU16(writer, (uint)writer.Position);

            foreach (ParamEntry entry in context.ParamObject.ParamEntries)
            {
                _savedParamEntries.Add(entry);

                long startOffset = writer.Position;
                TotalParamCount += 1;

                writer.Write(entry.Hash);
                var paramData = WritePlaceholderOffsetU24(writer, startOffset);
                paramData.data = entry;
                writer.Write((byte)entry.ParamType);

                byte[] data = GetParamData(entry);

                if (IsString(entry.ParamType))
                {
                    //Only write string entires once if any are the same
                    //DataValues store byte arrays for the data as the key then a list of offsets pointing to it

                    if (!StringValues.ContainsKey(data))
                        StringValues.Add(data, new List<PlaceholderOffet>() { paramData, });
                    else
                        StringValues[data].Add(paramData);
                }
                else
                {
                    //Only write data entires once if any are the same
                    //DataValues store byte arrays for the data as the key then a list of offsets pointing to it
                    if (DataValues.ContainsKey(data))
                        DataValues[data].Add(paramData); //Add additional offsets
                    else
                        DataValues.Add(data, new List<PlaceholderOffet> { paramData });
                }
            }
        }

        private bool IsString(ParamType ParamType)
        {
            switch (ParamType)
            {
                case ParamType.String64:
                case ParamType.String32:
                case ParamType.String256:
                case ParamType.StringRef:
                    return true;
                default:
                    return false;
            }
        }

        private byte[] GetParamData(ParamEntry entry)
        {
            MemoryStream mem = new MemoryStream();
            var writer = new FileWriter(mem);
            writer.ByteOrder = Syroot.BinaryData.ByteOrder.LittleEndian;

            switch (entry.ParamType)
            {
                case ParamType.Boolean: writer.Write((bool)entry.Value == false ? (uint)0 : (uint)1); break;
                case ParamType.Float: writer.Write((float)entry.Value); break;
                case ParamType.Int: writer.Write((int)entry.Value); break;
                case ParamType.Vector2F: writer.WriteVector2F((Vector2F)entry.Value); break;
                case ParamType.Vector3F: writer.WriteVector3F((Vector3F)entry.Value); break;
                case ParamType.Vector4F: writer.WriteVector4F((Vector4F)entry.Value); break;
                case ParamType.Color4F: writer.WriteVector4F((Vector4F)entry.Value); break;
                case ParamType.Quat: writer.Write((float[])entry.Value); break;
                case ParamType.Uint: writer.Write((uint)entry.Value); break;
                case ParamType.BufferUint:
                    writer.Write(((uint[])entry.Value).Length);
                    writer.Write((uint[])entry.Value); break;
                case ParamType.BufferInt:
                    writer.Write(((int[])entry.Value).Length);
                    writer.Write((int[])entry.Value); break;
                case ParamType.BufferFloat:
                    writer.Write(((float[])entry.Value).Length);
                    writer.Write((float[])entry.Value); break;
                case ParamType.BufferBinary:
                    writer.Write(((byte[])entry.Value).Length);
                    writer.Write((byte[])entry.Value); break;
                case ParamType.String64:
                case ParamType.String32:
                case ParamType.String256:
                case ParamType.StringRef:
                    writer.Write(((AampLibrary.StringEntry)entry.Value).Data);
                    break;
                case ParamType.Curve1:
                case ParamType.Curve2:
                case ParamType.Curve3:
                case ParamType.Curve4:
                    int curveAmount = entry.ParamType - ParamType.Curve1 + 1;

                    var curves = (Curve[])entry.Value;
                    for (int i = 0; i < curveAmount; i++)
                    {
                        writer.Write(curves[i].ValueUints);
                        writer.Write(curves[i].ValueFloats);
                    }
                    break;
                default:
                    throw new Exception("Unsupported param type! " + entry.ParamType);
            }

            return mem.ToArray();
        }

        uint GetListCount(ParamList paramList, uint total = 0)
        {
            total += 1;
            foreach (var child in paramList.ChildParams)
                total += GetListCount(child, total);

            return total;
        }

        uint GetObjectCount(ParamList paramList, uint total = 0)
        {
            total += 1;
            foreach (var child in paramList.ChildParams)
                total += GetObjectCount(child, total);

            return total;
        }

        uint GetParamCount(ParamList paramList, uint total = 0)
        {
            foreach (var obj in paramList.ParamObjects)
                total += (uint)obj.ParamEntries.Length;

            foreach (var child in paramList.ChildParams)
                total += GetObjectCount(child, total);

            return total;
        }
    }
}
