namespace CouchDB.Driver.Types;

/// <summary>
/// Represents a document field type.
/// </summary>
public sealed class CouchType
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
    public static readonly CouchType CNull = new("null");

    /// <summary>
    /// Represents the "boolean" type.
    /// </summary>
    public static readonly CouchType CBoolean = new("boolean");

    /// <summary>
    /// Represents the "number" type.
    /// </summary>
    public static readonly CouchType CNumber = new("number");

    /// <summary>
    /// Represents the "string" type.
    /// </summary>
    public static readonly CouchType CString = new("string");

    /// <summary>
    /// Represents the "array" type.
    /// </summary>
    public static readonly CouchType CArray = new("array");

    /// <summary>
    /// Represents the "object" type.
    /// </summary>
    public static readonly CouchType CObject = new("object");
}