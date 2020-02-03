using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AampLibraryCSharp.IO;
using System.IO;
using Syroot.Maths;

namespace AampLibraryCSharp
{
    public class YamlConverter
    {
        #region Read
        public static AampFile FromYaml(string text)
        {
            AampFile file = new AampFile();
            using (TextReader reader = new StringReader(text))
            {
                while (true)
                {
                    string line = reader.ReadLine();
                    if (line.Contains("!aamp"))
                    {
                        line = reader.ReadLine();
                        if (line.Contains("version"))
                            file.Version = (uint)ParseValueInt(line);
                    }
                    if (line.Contains("!io"))
                    {
                        line = reader.ReadLine();
                        if (line.Contains("version"))
                            file.ParameterIOVersion = (uint)ParseValueInt(line);
                    }
                    if (line.Contains("type:") && file.ParameterIOType == null)
                        file.ParameterIOType = ParseValueString(line);

                    if (line.Contains("!list"))
                        file.RootNode = ParseParamList(reader, line);

                    if (file.RootNode != null)
                        break;
                }
            }
            return file;
        }

        private static ParamList ParseParamList(TextReader reader, string line)
        {
            ParamList paramList = new ParamList();
            paramList.Hash = ParseHash(line);

            string newLine = reader.ReadLine();

            List<ParamObject> objects = new List<ParamObject>();
            List<ParamList> children = new List<ParamList>();
            if (newLine.Contains("lists:"))
                children.Add(ParseParamList(reader, newLine));
            if (newLine.Contains("objects:"))
                objects.Add(ParseParamObject(reader));

            return paramList;
        }

        private static ParamObject ParseParamObject(TextReader reader)
        {
            string line = reader.ReadLine();

            ParamObject paramObject = new ParamObject();

            return paramObject;
        }

        private static uint ParseHash(string line)
        {
            uint hash = 0;

            var name = line.Split(':').FirstOrDefault();
            bool isHash = uint.TryParse(name, out hash);
            if (!isHash)
                return System.Security.Cryptography.Crc32.Compute(name);
            else
                return hash;
        }

        private static int ParseValueInt(string value) {
            return Int32.Parse(ParseValueString(value));
        }

        private static string ParseValueString(string value) {
            return value.Split(':').LastOrDefault();
        }

        #endregion

        #region Write

        public static string ToYaml(AampFile aampFile)
        {
            StringBuilder sb = new StringBuilder();

            using (TextWriter writer = new StringWriter(sb))
            {
                writer.WriteLine("!aamp");
                writer.WriteLine($"version: {aampFile.Version}");
                writer.WriteLine("!io");
                writer.WriteLine($"version: {aampFile.ParameterIOVersion}");
                writer.WriteLine($"type: {aampFile.ParameterIOType}");
                WriteParamList(writer, aampFile.RootNode, 0);
            }

            return sb.ToString();
        }

        private static void WriteParamList(TextWriter writer, ParamList paramList, int IndentAmount)
        {
            writer.WriteLine($"{paramList.HashString}: !list".Indent(IndentAmount));

            if (paramList.paramObjects.Length <= 0)
                writer.WriteLine("objects: {}".Indent(IndentAmount + 2));
            else
                writer.WriteLine("objects: ".Indent(IndentAmount + 2));

            foreach (var paramObj in paramList.paramObjects)
            {
                WriteParamObject(writer, paramObj, IndentAmount + 4);
            }

            if (paramList.childParams.Length <= 0)
                writer.WriteLine("lists: {}".Indent(IndentAmount + 2));
            else
                writer.WriteLine("lists: ".Indent(IndentAmount + 2));

            foreach (var child in paramList.childParams)
            {
                WriteParamList(writer, child, IndentAmount + 4);
            }
        }

        private static void WriteParamObject(TextWriter writer, ParamObject paramObj, int IndentAmount)
        {
            writer.WriteLine($"{paramObj.HashString} : !obj".Indent(IndentAmount));
            foreach (var entry in paramObj.paramEntries)
            {
                writer.WriteLine($"{WriteParamData(entry)}".Indent(IndentAmount + 2));
            }
        }

        private static string WriteParamData(ParamEntry entry)
        {
            switch (entry.ParamType)
            {
                case ParamType.Boolean: return $"{entry.HashString}: {(bool)entry.Value}";
                case ParamType.BufferBinary: return $"{entry.HashString}: !BufferBinary [ {WriteBytes((byte[])entry.Value)} ]";
                case ParamType.BufferFloat: return $"{entry.HashString}: !BufferFloat [ {WriteFloats((float[])entry.Value)} ]";
                case ParamType.BufferInt: return $"{entry.HashString}: !BufferInt [ {WriteInts((int[])entry.Value)} ]";
                case ParamType.BufferUint: return $"{entry.HashString}: !BufferUint [ {WriteUints((uint[])entry.Value)} ]";
                case ParamType.Quat: return $"{entry.HashString}: !BufferUint [ {WriteFloats((float[])entry.Value)} ]";
                case ParamType.Color4F: return $"{entry.HashString}: {WriteColor4F((Vector4F)entry.Value)}";
                case ParamType.Vector2F: return $"{entry.HashString}: {WriteVec2F((Vector2F)entry.Value)}";
                case ParamType.Vector3F: return $"{entry.HashString}: {WriteVec3F((Vector3F)entry.Value)}";
                case ParamType.Vector4F: return $"{entry.HashString}: {WriteVec4F((Vector4F)entry.Value)}";
                case ParamType.Uint: return $"{entry.HashString}: {(uint)entry.Value}";
                case ParamType.Int: return $"{entry.HashString}: {(int)entry.Value}";
                case ParamType.Float: return $"{entry.HashString}: {(float)entry.Value}";
                case ParamType.String256: return $"{entry.HashString}: !str256 {((AampLibraryCSharp.StringEntry)entry.Value).ToString()}";
                case ParamType.String32: return $"{entry.HashString}: !str32 {((AampLibraryCSharp.StringEntry)entry.Value).ToString()}";
                case ParamType.String64: return $"{entry.HashString}: !str64 {((AampLibraryCSharp.StringEntry)entry.Value).ToString()}";
                case ParamType.StringRef: return $"{entry.HashString}: !strRef {((AampLibraryCSharp.StringEntry)entry.Value).ToString()}";
                case ParamType.Curve1: return $"{entry.HashString}: {WriteCurve((Curve[])entry.Value, 1)}";
                case ParamType.Curve2: return $"{entry.HashString}: {WriteCurve((Curve[])entry.Value, 2)}";
                case ParamType.Curve3: return $"{entry.HashString}: {WriteCurve((Curve[])entry.Value, 3)}";
                case ParamType.Curve4: return $"{entry.HashString}: {WriteCurve((Curve[])entry.Value, 4)}";
                default:
                    throw new Exception("Unsupported type! " + entry.ParamType);
            }
        }

        private static string WriteCurve(Curve[] curves, int Num)
        {
            string curveStr = "";
            foreach (var curve in curves)
                curveStr += $"!curve{Num}[{WriteUints(curve.valueUints)}] [{WriteFloats(curve.valueFloats)}] \n";

            return curveStr;
        }

        public static AampFile ToAamp(string FileName)
        {
            var aampFile = new AampFile();

            return aampFile;
        }

        private static string WriteUints(uint[] arr)
        {
            return String.Join(",", arr.Select(p => p.ToString()).ToArray());
        }

        private static string WriteFloats(float[] arr)
        {
            return String.Join(",", arr.Select(p => p.ToString()).ToArray());
        }

        private static string WriteInts(int[] arr)
        {
            return String.Join(",", arr.Select(p => p.ToString()).ToArray());
        }

        private static string WriteBytes(byte[] arr)
        {
            return String.Join(",", arr.Select(p => p.ToString()).ToArray());
        }

        private static string WriteVec2F(Vector2F vec2) { return $"!vec2[{vec2.X}, {vec2.Y}]"; }
        private static string WriteVec3F(Vector3F vec3) { return $"!vec3[{vec3.X}, {vec3.Y}, {vec3.Z}]"; }
        private static string WriteVec4F(Vector4F vec4) { return $"!vec4[{vec4.X}, {vec4.Y}, {vec4.Z}, {vec4.W}]"; }
        private static string WriteColor4F(Vector4F vec4) { return $"!color[{vec4.X}, {vec4.Y}, {vec4.Z}, {vec4.W}]"; }

        #endregion
    }
}
