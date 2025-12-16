namespace CouchDB.Driver.Types;

[Serializable]
public sealed record CouchReplicationHost(
    [property: JsonPropertyName("url")]
    string Url,
    [property: JsonPropertyName("auth")]
    CouchReplicationAuth? Auth
);