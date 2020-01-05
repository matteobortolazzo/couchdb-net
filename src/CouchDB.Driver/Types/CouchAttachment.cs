using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace CouchDB.Driver.Types
{
    public class CouchAttachment
    {
        [JsonIgnore]
        internal FileInfo FileInfo { get; set; }

        [JsonIgnore]
        internal bool Deleted { get; set; }

        [JsonIgnore]
        public string Name { get; set; }

        [DataMember]
        [JsonProperty("content_type")]
        public string ContentType { get; set; }
    }
}
