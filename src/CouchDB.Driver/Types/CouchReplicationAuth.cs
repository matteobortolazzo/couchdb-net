namespace CouchDB.Driver.Types;

[Serializable]
public sealed record CouchReplicationAuth(
    [property:JsonPropertyName("basic")]
    CouchReplicationBasicCredentials BasicCredentials);