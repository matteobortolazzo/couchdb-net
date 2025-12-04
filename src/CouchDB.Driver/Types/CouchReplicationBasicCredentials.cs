using CouchDB.Driver.Helpers;
using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Linq;

namespace CouchDB.Driver.Types
{
    public class CouchReplicationBasicCredentials
    {
        public CouchReplicationBasicCredentials(string username, string password)
        {
            Check.NotNull(username, nameof(username));
            Check.NotNull(password, nameof(password));

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
}
