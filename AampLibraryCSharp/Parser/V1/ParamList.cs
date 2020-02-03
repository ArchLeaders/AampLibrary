using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using AampLibraryCSharp.IO;

namespace AampLibraryCSharp
{
    public class ParamListV1 
    {
        internal static ParamList Read(FileReader reader)
        {
            ParamList entry = new ParamList();

            long CurrentPosition = reader.Position;

            uint Size = reader.ReadUInt32();
            entry.Hash = reader.ReadUInt32();
            uint ChildListCount = reader.ReadUInt32();
            uint ParamObjectCount = reader.ReadUInt32();

            entry.childParams = new ParamList[ChildListCount];
            entry.paramObjects = new ParamObject[ParamObjectCount];

            for (int i = 0; i < ChildListCount; i++)
                entry.childParams[i] = ParamListV1.Read(reader);

            for (int i = 0; i < ParamObjectCount; i++)
                entry.paramObjects[i] = ParamObjectV1.Read(reader);

            reader.Seek(CurrentPosition + Size, SeekOrigin.Begin);
            return entry;
        }

        internal static void Write(ParamList entry, FileWriter writer)
        {
            int ChildListCount = entry.childParams == null ? 0 : entry.childParams.Length;
            int ParamObjectCount = entry.paramObjects == null ? 0 : entry.paramObjects.Length;

            long startPosition = writer.Position;
            writer.Write(uint.MaxValue); //Write the size after
            writer.Write(entry.Hash);
            writer.Write(ChildListCount);
            writer.Write(ParamObjectCount);

            for (int i = 0; i < ChildListCount; i++)
                ParamListV1.Write(entry.childParams[i], writer);
            
            for (int i = 0; i < ParamObjectCount; i++)
                ParamObjectV1.Write(entry.paramObjects[i], writer);
            
            writer.WriteSize(writer.Position, startPosition);
        }
    }
}
