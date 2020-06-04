using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CouchDB.Driver.Types
{
    /// <summary>
    /// Represents a CouchDB user.
    /// </summary>
    [JsonObject("_users")]
    public class CouchUser : CouchDocument
    {
        internal const string Prefix = "org.couchdb.user:";
      
        public CouchUser(string name, string password, List<string>? roles = null, string type = "user")
        {
            Id = name != null ? Prefix + name : throw new ArgumentNullException(nameof(name));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Password = password;
            Roles = roles ?? new List<string>();
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }
        
        [DataMember]
        [JsonProperty("password")]
        internal string Password { get; }

        /// <summary>
        /// User’s name aka login. Immutable e.g. you cannot rename an existing user - you have to create new one.
        /// </summary>
        [DataMember]
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// List of user roles. CouchDB doesn’t provide any built-in roles, so you’re free to define your own depending on your needs. 
        /// However, you cannot set system roles like _admin there. 
        /// Also, only administrators may assign roles to users - by default all users have no roles
        /// </summary>
        [DataMember]
        [JsonProperty("roles")]
        public List<string> Roles { get; set; }        

        /// <summary>
        /// Document type. Constantly has the value user.
        /// </summary>
        [DataMember]
        [JsonProperty("type")]
        public string Type { get; set; }
    }
}