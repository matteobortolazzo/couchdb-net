using Humanizer;
using System;

namespace CouchDB.Driver.Types
{
    public class CaseType
    {
        public string Value { get; }

        protected CaseType(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }
        public virtual string Convert(string str)
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
    public class EntityCaseType : CaseType
    {
        private EntityCaseType(string value) : base(value) { }

        public static EntityCaseType None = new EntityCaseType("None");
        public static EntityCaseType UnderscoreCase = new EntityCaseType("UnderscoreCase");
        public static EntityCaseType DashCase = new EntityCaseType("DashCase");
        public static EntityCaseType KebabCase = new EntityCaseType("KebabCase");

        public override string Convert(string str)
        {
            if (this == EntityCaseType.None)
                return str.ToLowerInvariant();
            if (this == EntityCaseType.UnderscoreCase)
                return str.ToLowerInvariant().Underscore();
            if (this == EntityCaseType.DashCase)
                return str.ToLowerInvariant().Dasherize();
            if (this == EntityCaseType.KebabCase)
                return str.ToLowerInvariant().Kebaberize();
            throw new NotSupportedException($"Value {Value} not supported.");
        }
    }
    public class PropertyCaseType : CaseType
    {
        private PropertyCaseType(string value) : base(value) { }

        public static PropertyCaseType None = new PropertyCaseType("None");
        public static PropertyCaseType UnderscoreCase = new PropertyCaseType("UnderscoreCase");
        public static PropertyCaseType DashCase = new PropertyCaseType("DashCase");
        public static PropertyCaseType KebabCase = new PropertyCaseType("KebabCase");
        public static PropertyCaseType PascalCase = new PropertyCaseType("PascalCase");
        public static PropertyCaseType CamelCase = new PropertyCaseType("CamelCase");

        public override string Convert(string str)
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
