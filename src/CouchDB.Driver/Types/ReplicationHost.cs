namespace CouchDB.Driver.Types;

[Serializable]
public sealed record ReplicationHost(
    [property: JsonPropertyName("url")]
    string Url,
    [property: JsonPropertyName("auth")]
    ReplicationAuth? Auth
);