using Newtonsoft.Json;
using System.Collections.Generic;

namespace CouchDB.Driver.Types
{
    /// <summary>
    /// Represents list of users and/or roles that have rights to the database.
    /// </summary>
    public class CouchSecurityUserType
    {
        internal CouchSecurityUserType() { }

        /// <summary>
        /// List of CouchDB user names.
        /// </summary>
        [JsonProperty("names")]
        public List<string> Names { get; internal set; }

        /// <summary>
        /// List of users roles.
        /// </summary>
        [JsonProperty("roles")]
        public List<string> Roles { get; internal set; }
    }

    /// <summary>
    /// Represents two compulsory elements, admins and members, which are used to specify the list of users and/or roles that have admin and members rights to the database.
    /// </summary>
    public class CouchSecurity
    {
        internal CouchSecurity() { }

        /// <summary>
        /// They can read all types of documents from the DB, and they can write (and edit) documents to the DB except for design documents.
        /// </summary>
        [JsonProperty("members")]
        public CouchSecurityUserType Members { get; internal set; }
        /// <summary>
        /// They have all the privileges of members plus the privileges: 
        /// write (and edit) design documents, add/remove database admins and members and set the database revisions limit. 
        /// They can not create a database nor delete a database.
        /// </summary>
        [JsonProperty("admins")]
        public CouchSecurityUserType Admins { get; internal set; }
    }
}
