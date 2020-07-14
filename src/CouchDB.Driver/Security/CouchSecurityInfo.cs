using Newtonsoft.Json;

namespace CouchDB.Driver.Security
{
    /// <summary>
    /// Represents two compulsory elements, admins and members, which are used to specify the list of users and/or roles that have admin and members rights to the database.
    /// </summary>
    public sealed class CouchSecurityInfo
    {
        public CouchSecurityInfo()
        {
            Members = new CouchSecurityInfoType();
            Admins = new CouchSecurityInfoType();
        }

        /// <summary>
        /// They can read all types of documents from the DB, and they can write (and edit) documents to the DB except for design documents.
        /// </summary>
        [JsonProperty("members")]
        public CouchSecurityInfoType Members { get; set; }
        /// <summary>
        /// They have all the privileges of members plus the privileges: 
        /// write (and edit) design documents, add/remove database admins and members and set the database revisions limit. 
        /// They can not create a database nor delete a database.
        /// </summary>
        [JsonProperty("admins")]
        public CouchSecurityInfoType Admins { get; set; }
    }
}
