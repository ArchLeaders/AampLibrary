using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AampLibrary
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

        /// <summary>
        /// Gets or sets the effect name of the aamp/>
        /// </summary>
        public string EffectName { get; set; }

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
        /// Gets or sets the type of the aamp effect/>
        /// </summary>
        public string EffectType
        {
            get
            {
                if (EffectName.Contains("aglenv")) return "Environment";
                else if (EffectName.Contains("glpbd")) return "Probe Data";
                else if (EffectName.Contains("genv")) return "Environment";
                else if (EffectName.Contains("gsdw")) return "Shadow";
                else if (EffectName.Contains("agllmap")) return "Light Map";
                else if (EffectName.Contains("agldof")) return "Depth of Field";
                else if (EffectName.Contains("aglfila")) return "AA Filter";
                else if (EffectName.Contains("aglblm")) return "Bloom";
                else if (EffectName.Contains("aglccr")) return "Color Correction";
                else if (EffectName.Contains("aglcube")) return "Cube Map";
                else if (EffectName.Contains("aglatex")) return "Auto Dxposure";
                else if (EffectName.Contains("aglflr")) return "Flare Filter";
                else if (EffectName.Contains("aglmf")) return "Multi Filter";
                else if (EffectName.Contains("aglsdw")) return "Depth Shadow";
                else if (EffectName.Contains("aglshpp")) return "Shadow Pre Pass";
                else if (EffectName.Contains("aglofx")) return "Occluded Effect";
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
            reader.CheckSignature("AAMP");
            unknownValue = reader.ReadUInt32();
            Endianness = reader.ReadUInt32();
            reader.CheckByteOrderMark(Endianness);
            uint FileSize = reader.ReadUInt32();
            Version = reader.ReadUInt32();
            uint NameLength = reader.ReadUInt32();
            EffectName = reader.ReadString((int)NameLength, Encoding.Default);

            RootNode = new ParamList();
            RootNode.Read(reader);
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
            writer.Write(EffectName.Length);
            writer.Write(Encoding.Default.GetBytes(EffectName));

            RootNode.Write(writer);


            //Write end of file
            writer.Seek(sizeOffset, System.IO.SeekOrigin.Begin);
            uint FileSize = (uint)writer.BaseStream.Length;
            writer.Write(FileSize);

            writer.Close();
            writer.Dispose();
        }
    }
}
