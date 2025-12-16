namespace CouchDB.Driver.Types;

// TODO: Add docs
[Serializable]
public sealed record RevisionInfo(
    [property: JsonPropertyName("rev")]
    string Rev,
    [property: JsonPropertyName("status")]
    string Status);