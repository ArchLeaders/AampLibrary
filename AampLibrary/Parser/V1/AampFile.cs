using AampLibrary.IO;
using System.Text;

namespace AampLibrary
{
    internal class AampFileV1 : AampFile
    {
        public AampFileV1() { }
        public AampFileV1(string FileName) => Read(new FileReader(new FileStream(FileName, FileMode.Open)));
        public AampFileV1(Stream Stream) => Read(new FileReader(Stream));
        public override void Save(string FileName) => Write(new FileWriter(new FileStream(FileName, FileMode.Create, FileAccess.Write)));
        public override void Save(Stream Stream) => Write(new FileWriter(Stream));

        internal byte[]? EffectName { get; set; }

        internal void Read(FileReader reader)
        {
            reader.ByteOrder = Syroot.BinaryData.ByteOrder.LittleEndian;
            reader.CheckSignature("AAMP");
            Version = reader.ReadUInt32();
            Endianness = reader.ReadUInt32();

            uint FileSize = reader.ReadUInt32();
            ParameterIOVersion = reader.ReadUInt32();
            uint NameLength = reader.ReadUInt32();
            long pos = reader.Position;
            EffectName = reader.ReadBytes((int)NameLength);

            //read the string as zero terminated for now
            reader.Seek(pos, SeekOrigin.Begin);
            ParameterIOType = reader.ReadString(Syroot.BinaryData.BinaryStringFormat.ZeroTerminated);

            reader.Seek(pos + NameLength, SeekOrigin.Begin);
            RootNode = ParamListV1.Read(reader);
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
            writer.Write(AlignUp(ParameterIOType.Length + 1, 4));
            writer.Write(ParameterIOType, Syroot.BinaryData.BinaryStringFormat.ZeroTerminated);
            writer.Align(4);

            ParamListV1.Write(RootNode, writer);

            //Write end of file
            writer.Seek(sizeOffset, System.IO.SeekOrigin.Begin);
            uint FileSize = (uint)writer.BaseStream.Length;
            writer.Write(FileSize);

            writer.Close();
            writer.Dispose();
        }

        private static int AlignUp(int n, int align) => (n + align - 1) & -align;
    }
}
