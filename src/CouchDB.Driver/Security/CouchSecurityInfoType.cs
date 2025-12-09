using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.Security;

/// <summary>
/// Represents list of users and/or roles that have rights to the database.
/// </summary>
public sealed class CouchSecurityInfoType
{
    /// <summary>
    /// List of CouchDB users' names.
    /// </summary>
    [JsonPropertyName("names")]
    public List<string> Names { get; } = [];

    /// <summary>
    /// List of users roles.
    /// </summary>
    [JsonPropertyName("roles")]
    public List<string> Roles { get; } = [];
}