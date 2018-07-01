using System;
using System.Text.RegularExpressions;

namespace Gensearch.Helpers
{
    public static class ConvertHelp
    {
        static Regex intsOnly = new Regex(@"[a-zA-Z]+([-+](?!\d)|\s)");
        public static int ToInt(this string str) {
            return Convert.ToInt32(intsOnly.Replace(str, ""));
        }
    }
}