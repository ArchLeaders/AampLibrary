using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using AampLibraryCSharp.IO;

namespace AampLibraryCSharp
{
    public class AampFile
    {
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
        public uint Endianness { get; internal set; }

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

        public static AampFile LoadFile(string fileName) {
            return LoadFile(new FileStream(fileName, FileMode.Open));
        }

        public static AampFile LoadFile(Stream Stream)
        {
            AampFile file = null;
            uint version = CheckVersion(Stream);
            if (version == 2)
                file = new AampFileV2(Stream);
            else
                file = new AampFileV1(Stream);
            return file;
        }

        public virtual void Save(string FileName)
        {
        }

        public virtual void Save(Stream Stream)
        {

        }

        public void ToJson(string fileName)
        {
            File.WriteAllText(fileName, ToJson());
        }

        public string ToJson()
        {
            JsonConverter json = new JsonConverter();
            return json.ToJson(this);
        }

        private static uint CheckVersion(Stream stream)
        {
            using (FileReader reader = new FileReader(stream, true))
            {
                reader.ByteOrder = Syroot.BinaryData.ByteOrder.LittleEndian;
                reader.Position = 4;

                return reader.ReadUInt32();
            }
        }
    }
}
