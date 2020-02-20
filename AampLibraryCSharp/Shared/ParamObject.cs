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
        public T GetEntryValue<T>(string hashName) where T : new()
        {
            T instance = new T();
            foreach (var entry in paramEntries) {
                if (entry.HashString == hashName) {
                    if (entry.Value.GetType() == instance.GetType())
                        return (T)entry.Value;
                }
            }
            return instance;
        }

        public void SetEntryValue(string hashName, object value)
        {
            ParamEntry entry = paramEntries.FirstOrDefault(x => x.HashString == hashName);
            if (entry != null) {
                entry.Value = value;
            }
            else {
                entry = new ParamEntry();
                entry.HashString = hashName;
                entry.Value = value;
            }
        }

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
