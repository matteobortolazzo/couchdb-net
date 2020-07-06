using System.Collections.Generic;
using Newtonsoft.Json;

namespace CouchDB.Driver.Security
{
    /// <summary>
    /// Represents list of users and/or roles that have rights to the database.
    /// </summary>
    public sealed class CouchSecurityInfoType
    {
        public CouchSecurityInfoType()
        {
            Names = new List<string>();
            Roles = new List<string>();
        }

        /// <summary>
        /// List of CouchDB user names.
        /// </summary>
        [JsonProperty("names")]
        public List<string> Names { get; }

        /// <summary>
        /// List of users roles.
        /// </summary>
        [JsonProperty("roles")]
        public List<string> Roles { get; }
    }
}