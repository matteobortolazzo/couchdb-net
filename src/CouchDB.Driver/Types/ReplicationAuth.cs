namespace CouchDB.Driver.Types;

[Serializable]
public sealed record ReplicationAuth(
    [property:JsonPropertyName("basic")]
    ReplicationBasicCredentials BasicCredentials);