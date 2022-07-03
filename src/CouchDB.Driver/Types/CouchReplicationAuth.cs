using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace CouchDB.Driver.Types
{
    public class CouchReplicationAuth
    {
        [DataMember]
        [JsonProperty("basic")]
        public CouchReplicationBasicCredentials? BasicCredentials { get; internal set; }
    }
}
