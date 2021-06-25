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
        public CouchReplication(string source, string target, bool continuous, object? selector = null)
        {
            Source = source;
            Target = target;
            Continuous = continuous;
            Selector = selector;
        }

        [DataMember]
        [JsonProperty("source")]
        public string Source { get; private set; }

        [DataMember]
        [JsonProperty("target")]
        public string Target { get; private set; }

        [DataMember]
        [JsonProperty("continuous")]
        public bool Continuous { get; private set; }

        [DataMember]
        [JsonProperty("selector")]
        public object? Selector { get; private set; }
    }
}
