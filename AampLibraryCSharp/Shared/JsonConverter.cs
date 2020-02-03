using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Syroot.Maths;
using Newtonsoft.Json;

namespace AampLibraryCSharp
{
    internal class JsonConverter
    {
        internal string ToJson(AampFile aampFile)
        {
            return JsonConvert.SerializeObject(aampFile, Formatting.Indented);
        }
    }
}
