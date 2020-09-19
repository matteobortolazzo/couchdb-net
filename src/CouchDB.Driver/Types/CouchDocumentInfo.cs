#nullable disable
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace CouchDB.Driver.Types
{
    /// <summary>
    /// Represents a CouchDB document info.
    /// </summary>
    public class CouchDocumentInfo
    {
        [DataMember]
        [JsonProperty("id")]
        public string Id { get; private set; }

        [DataMember]
        [JsonProperty("key")]
        public string Key { get; private set; }

        [DataMember]
        [JsonProperty("value")]
        private CouchDocumentInfoValue Value { get; set; }

        public string Rev => Value.Rev;

        private class CouchDocumentInfoValue
        {
            [JsonProperty("rev")]
            public string Rev { get; set; }
        }
    }
}
#nullable restore