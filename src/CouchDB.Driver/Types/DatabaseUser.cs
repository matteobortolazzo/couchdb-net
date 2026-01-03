namespace CouchDB.Driver.Types;

/// <summary>
/// Represents a CouchDB user.
/// <param name="Name">User’s name aka login. Immutable e.g. you cannot rename an existing user - you have to create new one.</param>
/// <param name="Password">User’s name aka login. Immutable e.g. you cannot rename an existing user - you have to create new one.</param>
/// </summary>
[Serializable]
public record DatabaseUser(
    [property: JsonPropertyName("name")]
    string Name,
    [property: JsonPropertyName("password")]
    string Password)
{
    internal const string Prefix = "org.couchdb.user:";

    [property: JsonPropertyName("id")]
    public string Id => Prefix + Name;

    /// <summary>
    /// List of user roles. CouchDB doesn't provide any built-in roles, so you’re free to define your own depending on your needs. 
    /// However, you cannot set system roles like _admin there. 
    /// Also, only administrators may assign roles to users - by default all users have no roles
    /// </summary>
    [property: JsonPropertyName("roles")]
    public string[] Roles { get; init; } = [];

    /// <summary>
    /// Document type. Constantly has the value user.
    /// </summary>
    [property: JsonPropertyName("type")]
    public string Type { get; init; } = "user";
}