using System;
using Humanizer;

namespace CouchDB.Driver.Options
{
    /// <summary>
    /// A helper class for specify a case format for documents. 
    /// Every string will be at least convert to lowercase.
    /// </summary>
    public class DocumentCaseType : CaseType
    {
        private DocumentCaseType(string value) : base(value) { }

        /// <summary>
        /// Represents no format.
        /// </summary>
        public static readonly DocumentCaseType None = new DocumentCaseType("None");
        /// <summary>
        /// Represents underscore_case or snake_case.
        /// </summary>
        public static readonly DocumentCaseType UnderscoreCase = new DocumentCaseType("UnderscoreCase");        
        /// <summary>
        /// Represents kebab-case.
        /// </summary>
        public static readonly DocumentCaseType KebabCase = new DocumentCaseType("KebabCase");

        internal override string Convert(string str)
        {
            if (Equals(this, None))
            {
                return str.ToLowerInvariant();
            }
            if (Equals(this, UnderscoreCase))
            {
                return str.ToLowerInvariant().Underscore();
            }
            if (Equals(this, KebabCase))
            {
                return str.ToLowerInvariant().Kebaberize();
            }
            throw new NotSupportedException($"Value {Value} not supported.");
        }
    }
}