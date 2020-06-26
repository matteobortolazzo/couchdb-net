using Humanizer;
using System;
using CouchDB.Driver.Helpers;

namespace CouchDB.Driver.Settings
{
    /// <summary>
    /// A helper class for specify a case format for strings.
    /// </summary>
    public class CaseType
    {
        /// <summary>
        /// Name of the case format type.
        /// </summary>
        public string Value { get; }

        protected CaseType(string value)
        {
            Check.NotNull(value, nameof(value));
            Value = value;
        }
        internal virtual string Convert(string str)
        {
            throw new NotImplementedException();
        }
        public override bool Equals(object obj)
        {
            return obj is CaseType item && Value == item.Value;
        }
        public override int GetHashCode()
        {
            return Value.GetHashCode(StringComparison.InvariantCultureIgnoreCase);
        }
    }
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

#pragma warning disable CA1308 // Normalize strings to uppercase
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
#pragma warning restore CA1308 // Normalize strings to uppercase
    }

    /// <summary>
    /// A helper class for specify a case format for properties.
    /// </summary>
    public class PropertyCaseType : CaseType
    {
        private PropertyCaseType(string value) : base(value) { }

        /// <summary>
        /// Represents no format.
        /// </summary>
        public static readonly PropertyCaseType None = new PropertyCaseType("None");
        /// <summary>
        /// Represents underscore_case. 
        /// </summary>
        public static readonly PropertyCaseType UnderscoreCase = new PropertyCaseType("UnderscoreCase");
        /// <summary>
        /// Represents dash-case, allows uppercase characters. 
        /// </summary>
        public static readonly PropertyCaseType DashCase = new PropertyCaseType("DashCase");
        /// <summary>
        /// Represents kebab-case.
        /// </summary>
        public static readonly PropertyCaseType KebabCase = new PropertyCaseType("KebabCase");
        /// <summary>
        /// Represents PascalCase.
        /// </summary>
        public static readonly PropertyCaseType PascalCase = new PropertyCaseType("PascalCase");
        /// <summary>
        /// Represents camelCase.
        /// </summary>
        public static readonly PropertyCaseType CamelCase = new PropertyCaseType("CamelCase");

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
