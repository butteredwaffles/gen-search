using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Gensearch.Helpers
{
    public static class ConvertHelp
    {
        static Regex numbersOnly = new Regex(@"-?\+?([^\d\.\+\-]\.?)");
        public static int ToInt(this string str) {
            return Convert.ToInt32(numbersOnly.Replace(str, ""));
        }
    }
}