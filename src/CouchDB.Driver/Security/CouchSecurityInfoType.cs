namespace CouchDB.Driver.Security;

/// <summary>
/// Represents list of users and/or roles that have rights to the database.
/// </summary>
[method: JsonConstructor]
public sealed class CouchSecurityInfoType(List<string> names, List<string> roles)
{
    public CouchSecurityInfoType() : this([], [])
    {
    }

    /// <summary>
    /// List of CouchDB users' names.
    /// </summary>
    [property: JsonPropertyName("names")]
    public List<string> Names { get; } = names;

    /// <summary>
    /// List of users roles.
    /// </summary>
    [property: JsonPropertyName("roles")]
    public List<string> Roles { get; } = roles;
}