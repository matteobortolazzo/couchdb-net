using System.Text.Json.Serialization;
using System;
using System.Runtime.Serialization;

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

    [DataMember]
    [JsonPropertyName("username")]
    public string Username { get; set; }

    [DataMember]
    [JsonPropertyName("password")]
    public string Password { get; set; }
}