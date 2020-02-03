using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using AampLibraryCSharp.IO;

namespace AampLibraryCSharp
{
    public class ParamList
    {
        /// <summary>
        /// Gets the hash of the param list>
        /// </summary>
        [JsonIgnore]
        public uint Hash;

        /// <summary>
        /// Gets the hash converted string of the param list>
        /// </summary>
        public string HashString
        {
            get
            {
                return AampLibraryCSharp.Hashes.GetName(Hash);
            }
            set
            {
                Hash = AampLibraryCSharp.Hashes.SetName(value);
            }
        }

        /// <summary>
        /// Gets the child param list>
        /// </summary>
        public ParamList[] childParams;

        /// <summary>
        /// Gets the param object list>
        /// </summary>
        public ParamObject[] paramObjects;

        public void Read(FileReader reader, uint version)
        {

        }

        public void Write(FileWriter writer, uint version)
        {

        }
    }
}
