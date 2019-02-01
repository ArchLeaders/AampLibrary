using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AampLibrary;

namespace AampTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var probeAamp = "course.bglpbd";

            AampFile file = new AampFile(probeAamp);

            var root = file.RootNode;
            GetChildNodes(root);

            file.Save("New.aamp");

            Console.Read();
        }
        static void GetChildNodes(ParamList node)
        {
            foreach (var param in node.paramList)
            {
                GetChildNodes(param);
            }

            foreach (var paramObj in node.paramObjects)
            {
                Console.WriteLine("Object " + paramObj.HashString);
                Console.WriteLine("Group " + paramObj.GroupHashString);

                foreach (var entry in paramObj.paramEntries)
                {
                    Console.WriteLine("Entry " + entry.HashString + " " + entry.Value);
                }
            }
        }
    }
}
