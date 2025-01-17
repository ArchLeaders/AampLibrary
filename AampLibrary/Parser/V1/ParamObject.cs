﻿using AampLibrary.IO;

namespace AampLibrary
{
    internal class ParamObjectV1 
    {
        internal static ParamObject Read(FileReader reader)
        {
            ParamObject entry = new ParamObject();

            long CurrentPosition = reader.Position;

            uint Size = reader.ReadUInt32();
            uint EntryCount = reader.ReadUInt32();
            entry.Hash = reader.ReadUInt32();
            entry.GroupHash = reader.ReadUInt32();

            entry.ParamEntries = new ParamEntry[EntryCount];
            for (int i = 0; i < EntryCount; i++)
                entry.ParamEntries[i] = ParamEntryV1.Read(reader);

            reader.Seek(CurrentPosition + Size, SeekOrigin.Begin);
            return entry;
        }

        internal static void Write(ParamObject entry, FileWriter writer)
        {
            int EntryCount = entry.ParamEntries == null ? 0 : entry.ParamEntries.Length;

            long startPosition = writer.Position;

            writer.Write(uint.MaxValue); //Write the size after
            writer.Write(EntryCount);
            writer.Write(entry.Hash);
            writer.Write(entry.GroupHash);

            for (int i = 0; i < EntryCount; i++)
                if (entry.ParamEntries != null)
                    ParamEntryV1.Write(entry.ParamEntries[i], writer);

            writer.WriteSize(writer.Position, startPosition);
        }
    }
}
