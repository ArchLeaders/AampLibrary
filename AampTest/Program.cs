using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AampV2Library;

namespace AampTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var probeAamp = "course_bglpbd.szs 0.rarc";

            AampFile file = new AampFile(probeAamp);
            GetChildNodes(file.RootNode);
            file.Save("New.aamp");

            Console.Read();
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
                    Console.WriteLine("Entry " + entry.HashString + " " + entry.Value);
                }
            }
        }
    }
}
