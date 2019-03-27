namespace CouchDB.Driver.Types
{
    /// <summary>
    /// Represents a document field type.
    /// </summary>
    public class CouchType
    {
        /// <summary>
        /// Represents the CouchDB type value.
        /// </summary>
        public string Value { get; }

        private CouchType(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Represents the "null" type.
        /// </summary>
        public static readonly CouchType Null = new CouchType("null");
        /// <summary>
        /// Represents the "boolean" type.
        /// </summary>
        public static readonly CouchType Boolean = new CouchType("boolean");
        /// <summary>
        /// Represents the "number" type.
        /// </summary>
        public static readonly CouchType Number = new CouchType("number");
        /// <summary>
        /// Represents the "string" type.
        /// </summary>
        public static readonly CouchType String = new CouchType("string");
        /// <summary>
        /// Represents the "array" type.
        /// </summary>
        public static readonly CouchType Array = new CouchType("array");
        /// <summary>
        /// Represents the "object" type.
        /// </summary>
        public static readonly CouchType Object = new CouchType("object");
    }
}
