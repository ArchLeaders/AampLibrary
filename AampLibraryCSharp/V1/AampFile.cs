using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AampLibraryCSharp.IO;

namespace AampLibraryCSharp
{
    public class AampFileV1 : AampFile
    {
        public AampFileV1(string FileName)
        {
            Read(new FileReader(new FileStream(FileName, FileMode.Open)));
        }

        public AampFileV1(Stream Stream)
        {
            Read(new FileReader(Stream));
        }

        public override void Save(string FileName)
        {
            Write(new FileWriter(new FileStream(FileName, FileMode.Create, FileAccess.Write)));
        }

        public override void Save(Stream Stream)
        {
            Write(new FileWriter(Stream));
        }

        private byte[] effectName { get; set; }

        internal void Read(FileReader reader)
        {
            reader.CheckSignature("AAMP");
            unknownValue = reader.ReadUInt32();
            Endianness = reader.ReadUInt32();
            reader.CheckByteOrderMark(Endianness);

            uint FileSize = reader.ReadUInt32();
            Version = reader.ReadUInt32();
            uint NameLength = reader.ReadUInt32();
            long pos = reader.Position;
            effectName = reader.ReadBytes((int)NameLength);

            //read the string as zero terminated for now
            reader.Seek(pos, SeekOrigin.Begin);
            ParameterIOType = reader.ReadString(Syroot.BinaryData.BinaryStringFormat.ZeroTerminated);

            reader.Seek(pos + NameLength, SeekOrigin.Begin);
            RootNode = ParamListV1.Read(reader);
        }

        internal void Write(FileWriter writer)
        {
            writer.Write(Encoding.ASCII.GetBytes("AAMP"));
            writer.Write(unknownValue);
            writer.Write(Endianness);

            writer.CheckByteOrderMark(Endianness);

            long sizeOffset = writer.Position;
            writer.Write(0); //Write the file size later
            writer.Write(Version);
            writer.Write(effectName.Length);
            writer.Write(effectName);

            ParamListV1.Write(RootNode, writer);

            //Write end of file
            writer.Seek(sizeOffset, System.IO.SeekOrigin.Begin);
            uint FileSize = (uint)writer.BaseStream.Length;
            writer.Write(FileSize);

            writer.Close();
            writer.Dispose();
        }
    }
}
