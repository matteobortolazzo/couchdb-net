using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace CouchDB.Driver.Types
{
    /// <summary>
    /// Represents a CouchDB document.
    /// </summary>
    public abstract class CouchDocument
    {
        public CouchDocument()
        {
            _conflicts = new List<string>();
            _attachments = new Dictionary<string, CouchAttachment>();
            Attachments = new CouchAttachmentsCollection();
        }

        /// <summary>
        /// The document ID.
        /// </summary>
        [DataMember]
        [JsonProperty("_id", NullValueHandling = NullValueHandling.Ignore)]
        public virtual string Id { get; set; }
        [DataMember]
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
#pragma warning disable IDE0051 // Remove unused private members
        private string IdOther { set => Id = value; }
#pragma warning restore IDE0051 // Remove unused private members

        /// <summary>
        /// The current document revision ID.
        /// </summary>
        [DataMember]
        [JsonProperty("_rev", NullValueHandling = NullValueHandling.Ignore)]
        public string Rev { get; set; }
        [DataMember]
        [JsonProperty("rev", NullValueHandling = NullValueHandling.Ignore)]
#pragma warning disable IDE0051 // Remove unused private members
        private string RevOther { set => Rev = value; }
#pragma warning restore IDE0051 // Remove unused private members

        [DataMember]
        [JsonProperty("_conflicts")]
        private readonly List<string> _conflicts;

        [JsonIgnore]
        public IReadOnlyCollection<string> Conflicts => _conflicts.AsReadOnly();


#pragma warning disable IDE0051 // Remove unused private members
        // This must be Deserilizable-only field
        [JsonIgnore]
        private Dictionary<string, CouchAttachment> _attachments;
        [DataMember]
        [JsonProperty("_attachments")]
        private Dictionary<string, CouchAttachment> AttachmentsSetter
        {
            set { _attachments = value; }
        }
#pragma warning restore IDE0051 // Remove unused private members

        [JsonIgnore]
        public CouchAttachmentsCollection Attachments { get; internal set; }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            if (_attachments != null && _attachments.Count > 0)
            {
                Attachments = new CouchAttachmentsCollection(_attachments);
            }
        }
    }
}
