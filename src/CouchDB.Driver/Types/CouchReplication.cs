using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace CouchDB.Driver.Types
{
    [JsonObject("_replication")]
    public class CouchReplication : CouchDocument
    {
        [DataMember]
        [JsonProperty("source")]
        public string? Source { get; internal set; }

        [DataMember]
        [JsonProperty("target")]
        public string? Target { get; internal set; }

        [DataMember]
        [JsonProperty("continuous")]
        public bool Continuous { get; set; }

        [DataMember]
        [JsonProperty("selector")]
        public object? Selector { get; set; }

        [DataMember]
        [JsonProperty("cancel")]
        public bool Cancel { get; internal set; }
    }
}
