using System;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Runtime.Serialization;
using CouchDB.Driver.Helpers;

namespace CouchDB.Driver.Types;

/// <summary>
/// Represents a CouchDB user.
/// </summary>
[Serializable]
[DatabaseName("_users")]
public class CouchUser : CouchDocument
{
    internal const string Prefix = "org.couchdb.user:";

    public CouchUser(string name, string password, List<string>? roles = null, string type = "user")
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(type);

        Id = Prefix + name;
        Name = name;
        Password = password;
        Roles = roles ?? [];
        Type = type;
    }

    [DataMember]
    [JsonPropertyName("password")]
    internal string Password { get; set; }

    /// <summary>
    /// User’s name aka login. Immutable e.g. you cannot rename an existing user - you have to create new one.
    /// </summary>
    [DataMember]
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// List of user roles. CouchDB doesn't provide any built-in roles, so you’re free to define your own depending on your needs. 
    /// However, you cannot set system roles like _admin there. 
    /// Also, only administrators may assign roles to users - by default all users have no roles
    /// </summary>
    [DataMember]
    [JsonPropertyName("roles")]
    public List<string> Roles { get; set; }

    /// <summary>
    /// Document type. Constantly has the value user.
    /// </summary>
    [DataMember]
    [JsonPropertyName("type")]
    public string Type { get; set; }
}