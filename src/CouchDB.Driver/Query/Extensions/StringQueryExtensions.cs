using System.Text.RegularExpressions;

namespace CouchDB.Driver.Query.Extensions
{
    public static class StringQueryExtensions
    {
        /// <summary>
        /// Indicates whether the regular expression finds a match in the input string.
        /// </summary>
        /// <param name="input">The string to search for a match.</param>
        /// <param name="pattern">The regular expression pattern to match.</param>
        /// <returns>true if the regular expression finds a match; otherwise, false.</returns>
        public static bool IsMatch(this string input, string pattern)
        {
            return new Regex(pattern).IsMatch(input);
        }
    }
}
