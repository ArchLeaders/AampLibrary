using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AampLibraryCSharp.IO
{
    public static class StringExtensions
    {
        public static string Indent(this string value, int size)
        {
            var strArray = value.Split('\n');
            var sb = new StringBuilder();
            foreach (var s in strArray)
                sb.Append(new string(' ', size)).Append(s);
            return sb.ToString();
        }
    }
}
