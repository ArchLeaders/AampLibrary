using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace AampV2Library
{
    public class ParamObject
    {
        /// <summary>
        /// Gets the hash of the object name>
        /// </summary>
        public uint Hash;

        /// <summary>
        /// Gets the hash of the group name>
        /// </summary>
        public uint GroupHash;

        /// <summary>
        /// Gets the hash converted string of the object name>
        /// </summary>
        public string HashString
        {
            get
            {
                return Hashes.GetName(Hash);
            }
        }

        /// <summary>
        /// Gets the param object list>
        /// </summary>
        public ParamEntry[] paramEntries;

        internal void Read(FileReader reader)
        {
            long CurrentPosition = reader.Position;

            Hash = reader.ReadUInt32();
            ushort ChildOffset = reader.ReadUInt16();
            ushort ChildCount = reader.ReadUInt16();

            if (ChildOffset != 0)
            {
                using (reader.TemporarySeek(ChildOffset * 4 + CurrentPosition, SeekOrigin.Begin))
                {
                    paramEntries = new ParamEntry[ChildCount];
                    for (int i = 0; i < ChildCount; i++)
                    {
                        paramEntries[i] = new ParamEntry();
                        paramEntries[i].Read(reader);
                    }
                }
            }
        }
    }
}
