using System.Text.RegularExpressions;

namespace CouchDB.Driver.Extensions
{
    public static class StringExtensions
    {
        public static bool IsMatch(this string str, string pattern)
        {
            return new Regex(pattern).IsMatch(str);
        }
    }
}
