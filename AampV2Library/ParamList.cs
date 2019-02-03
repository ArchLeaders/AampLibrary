using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace AampV2Library
{
    public class ParamList
    {
        /// <summary>
        /// Gets the hash of the param list>
        /// </summary>
        public uint Hash;

        /// <summary>
        /// Gets the hash converted string of the param list>
        /// </summary>
        public string HashString
        {
            get
            {
                return Hashes.GetName(Hash);
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

        internal void Read(FileReader reader)
        {
            long CurrentPosition = reader.Position;

            Hash = reader.ReadUInt32();
            ushort ChildListOffset = reader.ReadUInt16();
            ushort ChildListCount = reader.ReadUInt16();
            ushort ParamObjectOffset = reader.ReadUInt16();
            ushort ParamObjectCount = reader.ReadUInt16();

            childParams = new ParamList[ChildListCount];
            paramObjects = new ParamObject[ParamObjectCount];

            if (ChildListOffset != 0)
            {
                using (reader.TemporarySeek(ChildListOffset * 4 + CurrentPosition, SeekOrigin.Begin))
                {
                    for (int i = 0; i < ChildListCount; i++)
                    {
                        childParams[i] = new ParamList();
                        childParams[i].Read(reader);
                    }
                }
            }
            if (ParamObjectOffset != 0)
            {
                using (reader.TemporarySeek(ParamObjectOffset * 4 + CurrentPosition, SeekOrigin.Begin))
                {
                    for (int i = 0; i < ParamObjectCount; i++)
                    {
                        paramObjects[i] = new ParamObject();
                        paramObjects[i].Read(reader);
                    }
                }
            }
        }
    }
}
