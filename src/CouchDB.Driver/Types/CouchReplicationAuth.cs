using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace CouchDB.Driver.Types
{
    public class CouchReplicationAuth
    {
        [DataMember]
        [JsonPropertyName("basic")]
        public CouchReplicationBasicCredentials? BasicCredentials { get; internal set; }
    }
}
