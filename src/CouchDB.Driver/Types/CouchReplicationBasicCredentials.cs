namespace CouchDB.Driver.Types;

// TODO Review
[Serializable]
public sealed record CouchReplicationBasicCredentials(
    [property: JsonPropertyName("username")]
    string Username,
    [property: JsonPropertyName("password")]
    string Password
);