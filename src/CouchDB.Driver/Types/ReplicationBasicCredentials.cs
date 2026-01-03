namespace CouchDB.Driver.Types;

// TODO Review
[Serializable]
public sealed record ReplicationBasicCredentials(
    [property: JsonPropertyName("username")]
    string Username,
    [property: JsonPropertyName("password")]
    string Password
);