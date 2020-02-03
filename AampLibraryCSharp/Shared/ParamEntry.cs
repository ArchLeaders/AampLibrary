using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using AampLibraryCSharp.IO;

namespace AampLibraryCSharp
{
    public class ParamEntry
    {
        /// <summary>
        /// Gets the hash of the object name>
        /// </summary>
        [JsonIgnore]
        public uint Hash { get; set; }

        /// <summary>
        /// Gets the hash converted string of the object name>
        /// </summary>
        [JsonProperty]
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
        /// Gets the ParamType of the entry>
        /// </summary>
        [JsonIgnore]
        public ParamType ParamType { get; set; }

        /// <summary>
        /// Gets or sets the value of the data>
        /// </summary>
        public object Value { get; set; }
    }
}
