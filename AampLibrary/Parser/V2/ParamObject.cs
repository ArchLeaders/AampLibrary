﻿using AampLibrary.IO;

namespace AampLibrary
{
    internal class ParamObjectV2 
    {
        internal static ParamObject Read(FileReader reader)
        {
            ParamObject entry = new();
            entry.Hash = reader.ReadUInt32();

            long CurrentPosition = reader.Position;
            ushort ChildOffset = reader.ReadUInt16();
            ushort ChildCount = reader.ReadUInt16();

            if (ChildOffset != 0)
            {
                using (reader.TemporarySeek(ChildOffset * 4 + CurrentPosition, SeekOrigin.Begin))
                {
                    entry.ParamEntries = new ParamEntry[ChildCount];
                    for (int i = 0; i < ChildCount; i++)
                        entry.ParamEntries[i] = ParamEntryV2.Read(reader);
                }
            }
            return entry;
        }
    }
}
