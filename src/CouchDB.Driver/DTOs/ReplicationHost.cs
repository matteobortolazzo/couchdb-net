using CouchDB.Driver.Types;

namespace CouchDB.Driver.DTOs;

[Serializable]
internal sealed record ReplicationHost(
    [property: JsonPropertyName("url")]
    string Url,
    [property: JsonPropertyName("auth")]
    ReplicationAuth? Auth
);