namespace CouchDB.Driver.Types;

/// <summary>
/// Represents a CouchDB document info.
/// </summary>
/// <param name="Id">The document ID.</param>
/// <param name="Key">The document key.</param>
/// <param name="Value">The document value containing revision information.</param>
[Serializable]
public sealed record CouchDocumentInfo(
    [property: JsonPropertyName("id")]
    string Id,
    [property: JsonPropertyName("key")]
    string Key,
    [property: JsonPropertyName("value")]
    CouchDocumentInfoValue Value
)
{
    /// <summary>
    /// Gets the document revision.
    /// </summary>
    public string Rev => Value.Rev;
}

/// <summary>
/// Represents the value portion of a CouchDB document info.
/// </summary>
/// <param name="Rev">The document revision identifier.</param>
[Serializable]
public sealed record CouchDocumentInfoValue(
    [property: JsonPropertyName("rev")]
    string Rev
);