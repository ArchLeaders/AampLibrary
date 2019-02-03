using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.IO;
using Syroot.Maths;

namespace AampV1Library
{
    internal class JsonConverter
    {
        internal void ToJson(AampFile aampFile, string FileName)
        {
            Write(aampFile, FileName);
        }

        void Read(AampFile aampFile)
        {
        }

        internal void Write(AampFile aampFile, string FileName)
        {
            var root = aampFile.RootNode;

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.Indented;

                JsonSerializer jsonSerializer = new JsonSerializer();
                jsonSerializer.Serialize(writer, aampFile);

            /*    writer.WriteStartObject();
                writer.WritePropertyName("version");
                writer.WriteValue(aampFile.Version.ToString("x"));
                writer.WritePropertyName("EffectName");
                writer.WriteValue(aampFile.EffectName);
                writer.WritePropertyName("EffectType");
                writer.WriteValue(aampFile.EffectType);*/

           //     WriteChildNodes(root, writer);
      //          writer.WriteEndObject();
            }

            Console.WriteLine(sw.ToString());

            File.WriteAllText(FileName, sw.ToString());

            sw.Close();
            sb.Clear();
        }

        void WriteChildNodes(ParamList node, JsonWriter writer)
        {
            writer.WritePropertyName(node.HashString == "" ? node.Hash.ToString() : node.HashString);
            writer.WriteValue("!list");


            if (node.childParams.Length <= 0)
            {
                writer.WritePropertyName("lists");
                writer.WriteValue("");
            }

            foreach (var param in node.childParams)
            {
                writer.WriteStartArray();
                WriteChildNodes(param, writer);
                writer.WriteEnd();
            }

            writer.WritePropertyName("objects");
            writer.WriteValue("");
            WriteEntries(node, writer);
        }
        void WriteEntries(ParamList node, JsonWriter writer)
        {
            foreach (var paramObj in node.paramObjects)
            {
                writer.WritePropertyName(paramObj.HashString == "" ? paramObj.Hash.ToString() : paramObj.HashString);
                writer.WriteValue("!!obj");

                foreach (var entry in paramObj.paramEntries)
                {
                    writer.WritePropertyName(entry.HashString == "" ? entry.Hash.ToString() : entry.HashString);

                    switch (entry.ParamType)
                    {
                        case ParamType.Boolean:
                        case ParamType.Int:
                        case ParamType.Uint:
                        case ParamType.Float:
                        case ParamType.String256:
                        case ParamType.String64:
                        case ParamType.String32:
                        case ParamType.StringRef:
                            string json = JsonConvert.SerializeObject(entry.Value, Formatting.Indented);
                            writer.WriteValue(json);
                            break;
                        case ParamType.Vector4F:
                            var Vec4 = (Vector4F)entry.Value;
                            writer.WriteValue($"!vec4 [{Vec4.X.ToString("0.0")}, {Vec4.Y.ToString("0.0")}, {Vec4.Z.ToString("0.0")}, {Vec4.W.ToString("0.0")}]");
                            break;
                        case ParamType.Color4F:
                            var col = (Vector4F)entry.Value;
                            writer.WriteValue($"!color [{col.X.ToString("0.0")}, {col.Y.ToString("0.0")}, {col.Z.ToString("0.0")}, {col.W.ToString("0.0")}]");
                            break;
                        case ParamType.Curve1:
                        case ParamType.Curve2:
                        case ParamType.Curve3:
                        case ParamType.Curve4:
                            int curveAmount = entry.ParamType - ParamType.Curve1 + 1;

                            writer.WriteStartArray();
                            var curves = (Curve[])entry.Value;
                            for (int i = 0; i < curveAmount; i++)
                            {
                                string valUint = JsonConvert.SerializeObject(curves[i].valueUints);
                                string valFloat = JsonConvert.SerializeObject(curves[i].valueFloats);
                                writer.WriteValue($"!curve{i}");
                                writer.WriteValue(valUint);
                                writer.WriteValue(valFloat);
                            }
                            writer.WriteEnd();
                            break;
                        case ParamType.BufferFloat:
                            writer.WriteValue($"!BufferFloat {JsonConvert.SerializeObject(entry.Value)}");
                            break;
                        case ParamType.BufferInt:
                            writer.WriteValue($"!BufferInt {JsonConvert.SerializeObject(entry.Value)}");
                            break;
                        case ParamType.BufferUint:
                            writer.WriteValue($"!BufferUint {JsonConvert.SerializeObject(entry.Value)}");
                            break;
                        default:
                            writer.WriteValue($"{JsonConvert.SerializeObject(entry.Value)}");
                            break;
                    }
                }
            }
        }
    }
}
