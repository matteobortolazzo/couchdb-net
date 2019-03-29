using Humanizer;
using System;

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
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }
        internal virtual string Convert(string str)
        {
            throw new NotImplementedException();
        }
        public override bool Equals(object obj)
        {
            if (!(obj is CaseType item))
            {
                return false;
            }
            return Value == item.Value;
        }
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
    /// <summary>
    /// A helper class for specify a case format for entities. 
    /// Every string will be at least convert to lowercase.
    /// </summary>
    public class EntityCaseType : CaseType
    {
        private EntityCaseType(string value) : base(value) { }

        /// <summary>
        /// Represents no format.
        /// </summary>
        public static readonly EntityCaseType None = new EntityCaseType("None");
        /// <summary>
        /// Represents underscore_case or snake_case.
        /// </summary>
        public static readonly EntityCaseType UnderscoreCase = new EntityCaseType("UnderscoreCase");        
        /// <summary>
        /// Represents kebab-case.
        /// </summary>
        public static readonly EntityCaseType KebabCase = new EntityCaseType("KebabCase");

        internal override string Convert(string str)
        {
            if (this == EntityCaseType.None)
                return str.ToLowerInvariant();
            if (this == EntityCaseType.UnderscoreCase)
                return str.ToLowerInvariant().Underscore();            
            if (this == EntityCaseType.KebabCase)
                return str.ToLowerInvariant().Kebaberize();
            throw new NotSupportedException($"Value {Value} not supported.");
        }
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
            if (this == PropertyCaseType.None)
                return str;
            if (this == PropertyCaseType.UnderscoreCase)
                return str.Underscore();
            if (this == PropertyCaseType.DashCase)
                return str.Dasherize();
            if (this == PropertyCaseType.KebabCase)
                return str.Kebaberize();
            if (this == PropertyCaseType.PascalCase)
                return str.Pascalize();
            if (this == PropertyCaseType.CamelCase)
                return str.Camelize();
            throw new NotSupportedException($"Value {Value} not supported.");
        }
    }
}
