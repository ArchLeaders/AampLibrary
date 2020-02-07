using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AampLibraryCSharp.IO;
using System.IO;
using Syroot.Maths;
using SharpYaml.Serialization;

namespace AampLibraryCSharp
{
    public class YamlConverter
    {
        #region Read
        public static AampFile FromYaml(string text)
        {
            AampFile file = new AampFile();
            var yaml = new YamlStream();
            yaml.Load(new StringReader(text));
            var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
        
            foreach (var entry in mapping.Children)
            {
                var key = ((YamlScalarNode)entry.Key).Value;
                var value = entry.Value.ToString();
                if (key == "aamp_version")
                {
                    file.Version = (uint)ParseValueInt(value);
                    if (file.Version == 2)
                        file = new AampFileV2();
                    else
                        file = new AampFileV1();
                }
                if (key == "type")
                    file.ParameterIOType = value;

                if (entry.Value.Tag == "!list")
                    file.RootNode = ParseParamList(key, (YamlMappingNode)entry.Value);
            }

            return file;
        }

        private static ParamList ParseParamList(YamlScalarNode node, YamlNode valueNode)
        {
            ParamList paramList = new ParamList();
            paramList.Hash = ParseHash(node.Value);

            List<ParamList> children = new List<ParamList>();
            List<ParamObject> objects = new List<ParamObject>();

            if (valueNode is YamlMappingNode) {
                foreach (YamlMappingNode child in ((YamlMappingNode)valueNode).Children.Values)
                {
                    foreach (var subChild in child.Children)
                    {
                        var scalar = ((YamlScalarNode)subChild.Key);

                        if (subChild.Value.Tag == "!list")
                            children.Add(ParseParamList(scalar, subChild.Value));
                        if (subChild.Value.Tag == "!obj")
                            objects.Add(ParseParamObject(scalar, subChild.Value));
                    }
                }
            }

            paramList.childParams = children.ToArray();
            paramList.paramObjects = objects.ToArray();

            return paramList;
        }

        private static ParamObject ParseParamObject(YamlScalarNode node, YamlNode valueNode)
        {
            ParamObject paramObject = new ParamObject();
            paramObject.Hash = ParseHash(node.Value);

            List<ParamEntry> entries = new List<ParamEntry>();
            if (valueNode is YamlMappingNode) {
                foreach (var child in ((YamlMappingNode)valueNode).Children)
                    entries.Add(ParseParamEntry(((YamlScalarNode)child.Key).Value, child.Value));

            }

            paramObject.paramEntries = entries.ToArray();
            return paramObject;
        }

        private static ParamEntry ParseParamEntry(string key, YamlNode valueNode)
        {
            ParamEntry entry = new ParamEntry();
            entry.Hash = ParseHash(key);

            if (valueNode is YamlSequenceNode)
            {
                var values = ((YamlSequenceNode)valueNode);

                switch (valueNode.Tag)
                {
                    case "!BufferBinary":
                        entry.Value = ToByteArray(values);
                        entry.ParamType = ParamType.BufferBinary;
                        break;
                    case "!BufferFloat":
                        entry.Value = ToFloatArray(values);
                        entry.ParamType = ParamType.BufferFloat;
                        break;
                    case "!BufferUint":
                        entry.Value = ToUIntArray(values);
                        entry.ParamType = ParamType.BufferUint;
                        break;
                    case "!BufferInt":
                        entry.Value = ToIntArray(values);
                        entry.ParamType = ParamType.BufferInt;
                        break;
                    case "!vec2": {
                            float[] singles = ToFloatArray(values, 2);
                            entry.Value = new Vector2F(singles[0], singles[1]);
                            entry.ParamType = ParamType.Vector2F;
                        }
                        break;
                    case "!vec3": {
                            float[] singles = ToFloatArray(values, 3);
                            entry.Value = new Vector3F(singles[0], singles[1], singles[2]);
                            entry.ParamType = ParamType.Vector3F;
                        }
                        break;
                    case "!color": {
                            float[] singles = ToFloatArray(values, 4);
                            entry.Value = new Vector4F(
                                singles[0], singles[1],
                                singles[2], singles[3]);
                            entry.ParamType = ParamType.Color4F;
                        }
                        break;
                    case "!vec4": {
                            float[] singles = ToFloatArray(values, 4);
                            entry.Value = new Vector4F(
                                singles[0], singles[1],
                                singles[2], singles[3]);
                            entry.ParamType = ParamType.Vector4F;
                        }
                        break;
                    case "!curve1":
                        entry.Value = ParseCurves(values, 1);
                        entry.ParamType = ParamType.Curve1;
                        break;
                    case "!curve2":
                        entry.Value = ParseCurves(values, 2);
                        entry.ParamType = ParamType.Curve2;
                        break;
                    case "!curve3":
                        entry.Value = ParseCurves(values, 3);
                        entry.ParamType = ParamType.Curve3;
                        break;
                    case "!curve4":
                        entry.Value = ParseCurves(values, 4);
                        entry.ParamType = ParamType.Curve4;
                        break;
                    default:
                        throw new Exception($"Unknown tag type using a sequence! {valueNode.Tag}");
                }
            }
            else if (valueNode is YamlScalarNode)
            {
                var value = ((YamlScalarNode)valueNode).Value;
                switch (valueNode.Tag)
                {
                    case "!str256":
                        entry.Value = new StringEntry(Encoding.UTF8.GetBytes(value));
                        entry.ParamType = ParamType.String256;
                        break;
                    case "!str64":
                        entry.Value = new StringEntry(Encoding.UTF8.GetBytes(value));
                        entry.ParamType = ParamType.String64;
                        break;
                    case "!str32":
                        entry.Value = new StringEntry(Encoding.UTF8.GetBytes(value));
                        entry.ParamType = ParamType.String32;
                        break;
                    case "!strRef":
                        entry.Value = new StringEntry(Encoding.UTF8.GetBytes(value));
                        entry.ParamType = ParamType.StringRef;
                        break;
                    default:
                        bool booleanValue;
                        uint uintValue;
                        float floatValue;
                        int intValue;
                        bool isBoolean = bool.TryParse(value, out booleanValue);
                        bool isUint = uint.TryParse(value, out uintValue);
                        bool isFloat = float.TryParse(value, out floatValue);
                        bool isInt = int.TryParse(value, out intValue);
                        bool HasDecimal = value.Contains(".");
                        if (isBoolean)
                        {
                            entry.Value = booleanValue;
                            entry.ParamType = ParamType.Boolean;
                        }
                        else if (isUint && !HasDecimal)
                        {
                            entry.Value = uintValue;
                            entry.ParamType = ParamType.Uint;
                        }
                        else if (isInt && !HasDecimal)
                        {
                            entry.Value = intValue;
                            entry.ParamType = ParamType.Int;
                        }
                        else if (isFloat)
                        {
                            entry.Value = floatValue;
                            entry.ParamType = ParamType.Float;
                        }
                        else
                            throw new Exception($"Failed to parse value for param {key} value {value}!");
                        break;
                }
            }

            return entry;
        }

        private static byte[] ToByteArray(YamlSequenceNode nodes)
        {
            List<byte> values = new List<byte>();
            foreach (var val in nodes)
                values.Add(byte.Parse(val.ToString()));
            return values.ToArray();
        }

        private static uint[] ToUIntArray(YamlSequenceNode nodes)
        {
            List<uint> values = new List<uint>();
            foreach (var val in nodes)
                values.Add(uint.Parse(val.ToString()));
            return values.ToArray();
        }

        private static int[] ToIntArray(YamlSequenceNode nodes)
        {
            List<int> values = new List<int>();
            foreach (var val in nodes)
                values.Add(int.Parse(val.ToString()));
            return values.ToArray();
        }

        private static float[] ToFloatArray(YamlSequenceNode nodes, int expectedLength = -1)
        {
            List<float> values = new List<float>();
            foreach (var val in nodes)
                values.Add(float.Parse(val.ToString()));

            if (expectedLength != -1 && values.Count != expectedLength)
                throw new System.Exception($"Invalid value length. " +
                    $"Expected {expectedLength}, got {values.Count}");

            return values.ToArray();
        }

        private static Curve[] ParseCurves(YamlSequenceNode nodes, int numCurves)
        {
            var values = nodes.ToList();

            Curve[] curves = new Curve[numCurves];
            int numValues = values.Count / numCurves; //Should be 32
            for (int i = 0; i < numCurves; i++) {
                List<uint> valueUints = new List<uint>();
                List<float> valueFloats = new List<float>();

                //2 ints
                //30 floats
                for (int j = 0; j < numValues; j++)
                {
                    var val = values[i * numValues + j].ToString();
                    if (j < 2)
                        valueUints.Add(ParseValueUnit(val));
                    else 
                        valueFloats.Add(ParseValueFloat(val));
                }

                curves[i] = new Curve()
                {
                    valueUints = valueUints.ToArray(),
                    valueFloats = valueFloats.ToArray(),
                };
            }
            return curves;
        }

        private static uint ParseHash(string name)
        {
            uint hash = 0;

            bool isHash = uint.TryParse(name, out hash);
            if (!isHash || hash == 0 || Hashes.HasString(name))
                return System.Security.Cryptography.Crc32.Compute(name);
            else
                return hash;
        }

        private static float ParseValueFloat(string value) {
            return float.Parse(ParseValueString(value));
        }

        private static uint ParseValueUnit(string value) {
            return uint.Parse(ParseValueString(value));
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
                writer.WriteLine($"aamp_version: {aampFile.Version}");
                writer.WriteLine($"io_version: {aampFile.ParameterIOVersion}");
                writer.WriteLine($"type: {aampFile.ParameterIOType}");
                WriteParamList(writer, aampFile.RootNode, 0);
            }

            return sb.ToString();
        }

        private static void WriteParamList(TextWriter writer, ParamList paramList, int IndentAmount)
        {
            writer.WriteLine($"{YamlHashStr(paramList.HashString)}: !list".Indent(IndentAmount));
           // Console.WriteLine($"HashString {paramList.HashString}");

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
            writer.WriteLine($"{YamlHashStr(paramObj.HashString)} : !obj".Indent(IndentAmount));
            foreach (var entry in paramObj.paramEntries)
            {
                writer.WriteLine($"{WriteParamData(entry)}".Indent(IndentAmount + 2));
            }
        }

        private static string WriteParamData(ParamEntry entry)
        {
            string value = "";
            switch (entry.ParamType)
            {
                case ParamType.Boolean: value = $"{(bool)entry.Value}"; break;
                case ParamType.BufferBinary: value = $"!BufferBinary [ {WriteBytes((byte[])entry.Value)} ]"; break;
                case ParamType.BufferFloat: value = $"!BufferFloat [ {WriteFloats((float[])entry.Value)} ]"; break;
                case ParamType.BufferInt: value = $"!BufferInt [ {WriteInts((int[])entry.Value)} ]"; break;
                case ParamType.BufferUint: value = $"!BufferUint [ {WriteUints((uint[])entry.Value)} ]"; break;
                case ParamType.Quat: value = $"!BufferUint [ {WriteFloats((float[])entry.Value)} ]"; break;
                case ParamType.Color4F: value = $"{WriteColor4F((Vector4F)entry.Value)}"; break;
                case ParamType.Vector2F: value = $"{WriteVec2F((Vector2F)entry.Value)}"; break;
                case ParamType.Vector3F: value = $"{WriteVec3F((Vector3F)entry.Value)}"; break;
                case ParamType.Vector4F: value = $"{WriteVec4F((Vector4F)entry.Value)}"; break;
                case ParamType.Uint: value = $"{(uint)entry.Value}"; break;
                case ParamType.Int: value = $"{(int)entry.Value}"; break;
                case ParamType.Float: value = string.Format("{0:0.0######}", (float)entry.Value); break;
                case ParamType.String256: value = $"!str256 {((AampLibraryCSharp.StringEntry)entry.Value).ToString()}"; break;
                case ParamType.String32: value = $"!str32 {((AampLibraryCSharp.StringEntry)entry.Value).ToString()}"; break;
                case ParamType.String64: value = $"!str64 {((AampLibraryCSharp.StringEntry)entry.Value).ToString()}"; break;
                case ParamType.StringRef: value = $"!strRef {((AampLibraryCSharp.StringEntry)entry.Value).ToString()}"; break;
                case ParamType.Curve1: value = $"{WriteCurve((Curve[])entry.Value, 1)}"; break;
                case ParamType.Curve2: value = $"{WriteCurve((Curve[])entry.Value, 2)}"; break;
                case ParamType.Curve3: value = $"{WriteCurve((Curve[])entry.Value, 3)}"; break;
                case ParamType.Curve4: value = $"{WriteCurve((Curve[])entry.Value, 4)}"; break;
                default:
                    throw new Exception("Unsupported type! " + entry.ParamType);
            }
            return $"{YamlHashStr(entry.HashString)}: {value}";
        }

        private static string YamlHashStr(string hash)
        {
            if (hash.Contains(":"))
                return '"' + hash + '"';
            return hash;
        }

        private static string WriteCurve(Curve[] curves, int Num)
        {
            string[] values = new string[Num];
            for (int i = 0; i < values.Length; i++)
                values[i] = $"{WriteUints(curves[i].valueUints)},{WriteFloats(curves[i].valueFloats)}";

            return $"!curve{Num} [{string.Join(",", values)}]";
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

        private static string WriteVec2F(Vector2F vec2) { return $"!vec2 [{vec2.X}, {vec2.Y}]"; }
        private static string WriteVec3F(Vector3F vec3) { return $"!vec3 [{vec3.X}, {vec3.Y}, {vec3.Z}]"; }
        private static string WriteVec4F(Vector4F vec4) { return $"!vec4 [{vec4.X}, {vec4.Y}, {vec4.Z}, {vec4.W}]"; }
        private static string WriteColor4F(Vector4F vec4) { return $"!color [{vec4.X}, {vec4.Y}, {vec4.Z}, {vec4.W}]"; }

        #endregion
    }
}
