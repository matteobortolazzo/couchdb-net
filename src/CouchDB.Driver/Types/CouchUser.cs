using CouchDB.Driver.Attributes;

namespace CouchDB.Driver.Types;

/// <summary>
/// Represents a CouchDB user.
/// <param name="Name">User’s name aka login. Immutable e.g. you cannot rename an existing user - you have to create new one.</param>
/// <param name="Password">User’s name aka login. Immutable e.g. you cannot rename an existing user - you have to create new one.</param>
/// <param name="Roles">
/// List of user roles. CouchDB doesn't provide any built-in roles, so you’re free to define your own depending on your needs. 
/// However, you cannot set system roles like _admin there. 
/// Also, only administrators may assign roles to users - by default all users have no roles
/// </param>
/// <param name="Type">Document type. Constantly has the value user.</param>
/// </summary>
[Serializable]
[DatabaseName("_users")]
public record CouchUser(
    string Name,
    string Password,
    IList<string>? Roles = null,
    string Type = "user")
{
    internal const string Prefix = "org.couchdb.user:";

    [property: JsonPropertyName("id")]
    public string Id => Prefix + Name;
}