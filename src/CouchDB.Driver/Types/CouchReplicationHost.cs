using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace CouchDB.Driver.Types
{
    public class CouchReplicationHost
    {
        [DataMember]
        [JsonProperty("url")]
        public string? Url { get; internal set; }

        [DataMember]
        [JsonProperty("auth")]
        public CouchReplicationAuth? Auth { get; internal set; }


    }
}
