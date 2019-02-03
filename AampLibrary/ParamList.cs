using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace AampV1Library
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

            uint Size = reader.ReadUInt32();
            Hash = reader.ReadUInt32();
            uint ChildListCount = reader.ReadUInt32();
            uint ParamObjectCount = reader.ReadUInt32();

            childParams = new ParamList[ChildListCount];
            paramObjects = new ParamObject[ParamObjectCount];

            for (int i = 0; i < ChildListCount; i++)
            {
                childParams[i] = new ParamList();
                childParams[i].Read(reader);
            }
            for (int i = 0; i < ParamObjectCount; i++)
            {
                paramObjects[i] = new ParamObject();
                paramObjects[i].Read(reader);
            }

            reader.Seek(CurrentPosition + Size, SeekOrigin.Begin);
        }

        internal void Write(FileWriter writer)
        {
            int ChildListCount = childParams == null ? 0 : childParams.Length;
            int ParamObjectCount = paramObjects == null ? 0 : paramObjects.Length;

            long startPosition = writer.Position;
            writer.Write(uint.MaxValue); //Write the size after
            writer.Write(Hash);
            writer.Write(ChildListCount);
            writer.Write(ParamObjectCount);

            for (int i = 0; i < ChildListCount; i++)
                childParams[i].Write(writer);
            
            for (int i = 0; i < ParamObjectCount; i++)
                paramObjects[i].Write(writer);
            
            writer.WriteSize(writer.Position, startPosition);
        }
    }
}
