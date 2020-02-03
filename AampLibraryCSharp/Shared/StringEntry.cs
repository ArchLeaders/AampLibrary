using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AampLibraryCSharp
{
    public class StringEntry
    {
        int MaxValue = -1;

        public StringEntry(byte[] data, int maxValue)
        {
            Data = data;
            MaxValue = maxValue;
        }

        public StringEntry(byte[] data)
        {
            Data = data;
        }

        public StringEntry(string text)
        {
            Data = EncodeType.GetBytes(text);
        }

        public Encoding EncodeType = Encoding.UTF8;

        public override string ToString()
        {
            return EncodeType.GetString(Data);
        }

        public byte[] Data;
    }
}
