namespace CouchDB.Driver.Types;

/// <summary>
/// Represents cluster configuration.
/// </summary>
/// <param name="Replicas">The number of copies of every document.</param>
/// <param name="Shards">The number of range partitions.</param>
/// <param name="ReadQuorum">The number of consistent copies of a document that need to be read before a successful reply.</param>
/// <param name="WriteQuorum">The number of copies of a document that need to be written before a successful reply.</param>
[Serializable]
public sealed record Cluster(
    [property: JsonPropertyName("n")]
    int Replicas,
    [property: JsonPropertyName("q")]
    int Shards,
    [property: JsonPropertyName("r")]
    int ReadQuorum,
    [property: JsonPropertyName("w")]
    int WriteQuorum
);