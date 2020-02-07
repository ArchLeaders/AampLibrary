using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AampLibraryCSharp
{
    public class ParamObject
    {
        /// <summary>
        /// Gets the hash of the object name>
        /// </summary>
        [JsonIgnore]
        public uint Hash;

        /// <summary>
        /// Gets the hash of the group name>
        /// </summary>
        public uint GroupHash = 0xCDCDCDCD;

        /// <summary>
        /// Gets the hash converted string of the object name>
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
        /// Gets the param object list>
        /// </summary>
        public ParamEntry[] paramEntries;

    }
}
