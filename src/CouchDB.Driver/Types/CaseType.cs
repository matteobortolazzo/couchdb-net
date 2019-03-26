using Humanizer;
using System;

namespace CouchDB.Driver.Types
{
    public class CaseType
    {
        public string Value { get; }

        private CaseType(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public static CaseType None = new CaseType("None");
        public static CaseType PascalCase = new CaseType("PascalCase");
        public static CaseType CamelCase = new CaseType("CamelCase");
        public static CaseType UnderscoreCase = new CaseType("UnderscoreCase");
        public static CaseType DashCase = new CaseType("DashCase");
        public static CaseType KebabCase = new CaseType("KebabCase");
        
        public string Convert(string str)
        {
            if (this == CaseType.None)
                return str;
            if (this == CaseType.PascalCase)
                return str.Pascalize();
            if (this == CaseType.CamelCase)
                return str.Camelize();
            if (this == CaseType.UnderscoreCase)
                return str.Underscore();
            if (this == CaseType.DashCase)
                return str.Dasherize();
            if (this == CaseType.KebabCase)
                return str.Kebaberize();
            throw new NotSupportedException($"Value {Value} not supported.");
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
}
