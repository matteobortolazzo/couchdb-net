using CouchDB.Driver.Helpers;
using Newtonsoft.Json;
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
        [JsonProperty("username")]
        public string Username { get; set; }

        [DataMember]
        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
