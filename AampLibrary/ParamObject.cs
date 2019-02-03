using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace AampV1Library
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
        /// Gets the hash converted string of the group name>
        /// </summary>
        public string GroupHashString
        {
            get
            {
                return Hashes.GetName(GroupHash);
            }
        }

        /// <summary>
        /// Gets the param object list>
        /// </summary>
        public ParamEntry[] paramEntries;

        internal void Read(FileReader reader)
        {
            long CurrentPosition = reader.Position;

            uint Size = reader.ReadUInt32();
            uint EntryCount = reader.ReadUInt32();
            Hash = reader.ReadUInt32();
            GroupHash = reader.ReadUInt32();

            paramEntries = new ParamEntry[EntryCount];
            for (int i = 0; i < EntryCount; i++)
            {
                paramEntries[i] = new ParamEntry();
                paramEntries[i].Read(reader);
            }

            reader.Seek(CurrentPosition + Size, SeekOrigin.Begin);
        }

        internal void Write(FileWriter writer)
        {
            int EntryCount = paramEntries == null ? 0 : paramEntries.Length;

            long startPosition = writer.Position;

            writer.Write(uint.MaxValue); //Write the size after
            writer.Write(EntryCount);
            writer.Write(Hash);
            writer.Write(GroupHash);

            for (int i = 0; i < EntryCount; i++)
            {
                paramEntries[i].Write(writer);
            }

            writer.WriteSize(writer.Position, startPosition);
        }
    }
}
