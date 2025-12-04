using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace CouchDB.Driver.Types
{
    public class CouchReplicationHost
    {
        [DataMember]
        [JsonPropertyName("url")]
        public string? Url { get; internal set; }

        [DataMember]
        [JsonPropertyName("auth")]
        public CouchReplicationAuth? Auth { get; internal set; }


    }
}
