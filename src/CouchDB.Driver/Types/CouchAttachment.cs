#nullable disable
using System;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace CouchDB.Driver.Types
{
    public sealed class CouchAttachment
    {
        [JsonIgnore]
        internal string DocumentId { get; set; }

        [JsonIgnore]
        internal string DocumentRev { get; set; }

        [JsonIgnore]
        internal FileInfo FileInfo { get; set; }

        [JsonIgnore]
        internal bool Deleted { get; set; }

        [JsonIgnore]
        public string Name { get; internal set; }

        [JsonIgnore]
        public Uri Uri { get; internal set; }

        [DataMember]
        [JsonProperty("stub")]
        public bool Stub { get; set; }

        [DataMember]
        [JsonProperty("content_type")]
        public string ContentType { get; set; }

        [DataMember]
        [JsonProperty("digest")]
        public string Digest { get; private set; }

        [DataMember]
        [JsonProperty("length")]
        public int? Length { get; private set; }

        [DataMember]
        [JsonProperty("revpos")]
        public int? RevPos { get; private set; }
    }
}
#nullable restore