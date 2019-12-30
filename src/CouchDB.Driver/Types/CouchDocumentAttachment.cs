using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CouchDB.Driver.Types
{
    public abstract class CouchDocumentAttachment : CouchDocument
    {
        [DataMember]
        [JsonProperty("_attachments", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, CouchDocumentAttachmentItem> Attachments { get; set; }
    }

    public class CouchDocumentAttachmentItem
    {
        [DataMember]
        [JsonProperty("content_type", NullValueHandling = NullValueHandling.Ignore)]
        public string ContentType { get; set; }

        [DataMember]
        [JsonProperty("revpos", NullValueHandling = NullValueHandling.Ignore)]
        public int RevPos { get; set; }

        [DataMember]
        [JsonProperty("digest", NullValueHandling = NullValueHandling.Ignore)]
        public string Digest { get; set; }

        [DataMember]
        [JsonProperty("length", NullValueHandling = NullValueHandling.Ignore)]
        public int Length { get; set; }

        [DataMember]
        [JsonProperty("stub", NullValueHandling = NullValueHandling.Ignore)]
        public bool Stub { get; set; }

        [DataMember]
        public string FileTobeAttach { get; set; }
    }
}
