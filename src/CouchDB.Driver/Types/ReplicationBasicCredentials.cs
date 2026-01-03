namespace CouchDB.Driver.Types;

[Serializable]
public sealed record ReplicationBasicCredentials(
    [property: JsonPropertyName("username")]
    string Username,
    [property: JsonPropertyName("password")]
    string Password
);