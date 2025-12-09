using System.Text.Json.Serialization;
using System;

namespace CouchDB.Driver.Types;

[Serializable]
public class CouchReplicationBasicCredentials
{
    public CouchReplicationBasicCredentials(string username, string password)
    {
        ArgumentNullException.ThrowIfNull(username);
        ArgumentNullException.ThrowIfNull(password);

        Username = username;
        Password = password;
    }

    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("password")]
    public string Password { get; set; }
}