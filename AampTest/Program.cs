using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AampLibraryCSharp;

namespace AampTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var aamp = "g3ds_packunslider.bgenv";

            AampFile file = AampFile.LoadFile(aamp);
            GetChildNodes(file.RootNode);
            ToYaml(file);
            file.Save("New.aamp");

            Console.Read();
        }

        static void ToYaml(AampFile file)
        {
            string yaml = YamlConverter.ToYaml(file);
            System.IO.File.WriteAllText("test.yaml", yaml);
        }

        static void GetChildNodes(ParamList node)
        {
            foreach (var param in node.childParams)
            {
                GetChildNodes(param);
            }

            foreach (var paramObj in node.paramObjects)
            {
                Console.WriteLine("Object " + paramObj.HashString);

                foreach (var entry in paramObj.paramEntries)
                {
                    if (entry.Value is Curve[])
                    {
                        int c = 0;
                        Console.WriteLine($"Entry {entry.HashString}");
                        foreach (var curve in (Curve[])entry.Value)
                        {
                            Console.WriteLine($"Curve {c++}");

                            foreach (float va in curve.valueUints)
                                Console.Write($" {va}");
                            foreach (float va in curve.valueFloats)
                                Console.Write($" {va}");
                            Console.WriteLine($"");
                        }
                    }
                    else
                        Console.WriteLine("Entry " + entry.HashString + " " + entry.Value);
                }
            }
        }
    }
}
