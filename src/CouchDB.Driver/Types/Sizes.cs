namespace CouchDB.Driver.Types;

/// <summary>
/// Represents size information.
/// </summary>
/// <param name="File">The size of the database file on disk in bytes. Views indexes are not included in the calculation.</param>
/// <param name="External">The uncompressed size of database contents in bytes.</param>
/// <param name="Active">The size of live data inside the database, in bytes.</param>
[Serializable]
public sealed record Sizes(
    [property: JsonPropertyName("file")]
    long File,
    [property: JsonPropertyName("external")]
    long External,
    [property: JsonPropertyName("active")]
    long Active
);