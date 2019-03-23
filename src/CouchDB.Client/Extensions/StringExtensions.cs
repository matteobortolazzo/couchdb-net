using Humanizer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CouchDB.Client.Extensions
{
    public static class StringExtensions
    {
        public static string ToCamelCase(this string str)
        {
            if (!string.IsNullOrEmpty(str) && str.Length > 1)
            {
                return str.Transform(To.LowerCase);
            }
            return str;
        }
        public static bool IsMatch(this string str, string pattern)
        {
            return new Regex(pattern).IsMatch(str);
        }
    }
}
