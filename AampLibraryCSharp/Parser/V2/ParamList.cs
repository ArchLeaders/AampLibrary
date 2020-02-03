using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using AampLibraryCSharp.IO;

namespace AampLibraryCSharp
{
    public class ParamListV2 
    {
        internal static ParamList Read(FileReader reader)
        {
            ParamList entry = new ParamList();

            long CurrentPosition = reader.Position;

            entry.Hash = reader.ReadUInt32();
            ushort ChildListOffset = reader.ReadUInt16();
            ushort ChildListCount = reader.ReadUInt16();
            ushort ParamObjectOffset = reader.ReadUInt16();
            ushort ParamObjectCount = reader.ReadUInt16();

            entry.childParams = new ParamList[ChildListCount];
            entry.paramObjects = new ParamObject[ParamObjectCount];

            if (ChildListOffset != 0)
            {
                using (reader.TemporarySeek(ChildListOffset * 4 + CurrentPosition, SeekOrigin.Begin))
                {
                    for (int i = 0; i < ChildListCount; i++)
                        entry.childParams[i] = ParamListV2.Read(reader);
                }
            }
            if (ParamObjectOffset != 0)
            {
                using (reader.TemporarySeek(ParamObjectOffset * 4 + CurrentPosition, SeekOrigin.Begin))
                {
                    for (int i = 0; i < ParamObjectCount; i++)
                        entry.paramObjects[i] = ParamObjectV2.Read(reader);
                }
            }

            return entry;
        }
    }
}
