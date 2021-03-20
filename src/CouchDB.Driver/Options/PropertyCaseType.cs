using System;
using Humanizer;

namespace CouchDB.Driver.Options
{
    /// <summary>
    /// A helper class for specify a case format for properties.
    /// </summary>
    public class PropertyCaseType : CaseType
    {
        private PropertyCaseType(string value) : base(value) { }

        /// <summary>
        /// Represents no format.
        /// </summary>
        public static readonly PropertyCaseType None = new("None");
        /// <summary>
        /// Represents underscore_case. 
        /// </summary>
        public static readonly PropertyCaseType UnderscoreCase = new("UnderscoreCase");
        /// <summary>
        /// Represents dash-case, allows uppercase characters. 
        /// </summary>
        public static readonly PropertyCaseType DashCase = new("DashCase");
        /// <summary>
        /// Represents kebab-case.
        /// </summary>
        public static readonly PropertyCaseType KebabCase = new("KebabCase");
        /// <summary>
        /// Represents PascalCase.
        /// </summary>
        public static readonly PropertyCaseType PascalCase = new("PascalCase");
        /// <summary>
        /// Represents camelCase.
        /// </summary>
        public static readonly PropertyCaseType CamelCase = new("CamelCase");

        internal override string Convert(string str)
        {
            if (Equals(this, None))
            {
                return str;
            }
            if (Equals(this, UnderscoreCase))
            {
                return str.Underscore();
            }
            if (Equals(this, DashCase))
            {
                return str.Dasherize();
            }
            if (Equals(this, KebabCase))
            {
                return str.Kebaberize();
            }
            if (Equals(this, PascalCase))
            {
                return str.Pascalize();
            }
            if (Equals(this, CamelCase))
            {
                return str.Camelize();
            }

            throw new NotSupportedException($"Value {Value} not supported.");
        }
    }
}