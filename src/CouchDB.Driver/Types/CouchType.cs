namespace CouchDB.Driver.Types
{
    /// <summary>
    /// Represents a document field type.
    /// </summary>
    public class CouchType
    {
        public string Value { get; }

        private CouchType(string value)
        {
            Value = value;
        }

        public static readonly CouchType Null = new CouchType("null");
        public static readonly CouchType Boolean = new CouchType("boolean");
        public static readonly CouchType Number = new CouchType("number");
        public static readonly CouchType String = new CouchType("string");
        public static readonly CouchType Array = new CouchType("array");
        public static readonly CouchType Object = new CouchType("object");
    }
}
