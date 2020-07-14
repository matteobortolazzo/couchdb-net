using System;
using CouchDB.Driver.Helpers;

namespace CouchDB.Driver.Options
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
}
