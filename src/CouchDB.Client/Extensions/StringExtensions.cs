using Humanizer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CouchDB.Client.Extensions
{
    public static class StringExtensions
    {
        public static bool IsMatch(this string str, string pattern)
        {
            return new Regex(pattern).IsMatch(str);
        }
    }
}
