#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

using AampLibrary.IO;
using System.Text;
using System.Text.Json;

namespace AampLibrary
{
    public class AampFile
    {
        internal AampFile() { }
        public AampFile(string fileName) => LoadStream(new FileStream(fileName, FileMode.Open));
        public AampFile(Stream stream) => LoadStream(stream);

        private void LoadStream(Stream stream)
        {
            uint version = CheckVersion(stream);

            if (version == 2)
                Setter(new AampFileV2(stream));
            else
                Setter(new AampFileV1(stream));
        }

        private void Setter(AampFile aamp)
        {
            ParameterIOVersion = aamp.ParameterIOVersion;
            ParameterIOType = aamp.ParameterIOType;
            UnknownValue = aamp.UnknownValue;
            Endianness = aamp.Endianness;
            RootNode = aamp.RootNode;
            Version = aamp.Version;
        }

        /// <summary>
        /// Gets or sets the effect name of the aamp
        /// </summary>
        public string ParameterIOType { get; set; }

        /// <summary>
        /// Gets or sets an unknown value
        /// </summary>
        public uint UnknownValue { get; set; } = 0x01000000;

        /// <summary>
        /// Gets the endianness of the file.
        /// </summary>
        public uint Endianness { get; internal set; } = 0x01000000;

        /// <summary>
        /// Gets the version of the file.
        /// </summary>
        public uint Version { get; internal set; }

        /// <summary>
        /// Gets the parameter io version of the file.
        /// </summary>
        public uint ParameterIOVersion { get; internal set; }


        /// <summary>
        /// Get the type of the aamp effect.
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
                else return "";
            }
        }

        /// <summary>
        /// Gets the root of the params in the file.
        /// </summary>
        public ParamList RootNode { get; set; }

        /// <summary>
        /// Saves the binary compiled AampFile object to a file.
        /// </summary>
        /// <param name="fileName"></param>
        public virtual void Save(string fileName) { }

        /// <summary>
        /// Writes the binary compiled AampFile object to a stream.
        /// </summary>
        /// <param name="stream"></param>
        public virtual void Save(Stream stream) { }

        /// <summary>
        /// Convert to AAMP version two.
        /// </summary>
        /// <returns></returns>
        public AampFile ConvertToVersion2()
        {
            return new AampFileV2()
            {
                Endianness = Endianness,
                ParameterIOType = ParameterIOType,
                ParameterIOVersion = 410,
                RootNode = RootNode,
                Version = 2,
                UnknownValue = 0,
            };
        }

        /// <summary>
        /// Convert to AAMP version one.
        /// </summary>
        /// <returns></returns>
        public AampFile ConvertToVersion1()
        {
            return new AampFileV1()
            {
                Endianness = Endianness,
                ParameterIOType = ParameterIOType,
                ParameterIOVersion = 0,
                RootNode = RootNode,
                Version = 1,
                UnknownValue = 0,
                EffectName = Encoding.UTF8.GetBytes(ParameterIOType),
            };
        }

        /// <summary>
        /// Converts the AampFile object to a JSON formatted string.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => JsonSerializer.Serialize(this, new JsonSerializerOptions() { WriteIndented = true });

        /// <summary>
        /// Writes the AampFile object as a JSON formatted file.
        /// </summary>
        /// <returns></returns>
        public void WriteJson(string fileName) => File.WriteAllText(fileName, ToString());

        /// <summary>
        /// Get the version of the AAMP <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private static uint CheckVersion(Stream stream)
        {
            using FileReader reader = new FileReader(stream, true);
            reader.ByteOrder = Syroot.BinaryData.ByteOrder.LittleEndian;
            reader.Position = 4;

            return reader.ReadUInt32();
        }
    }
}
