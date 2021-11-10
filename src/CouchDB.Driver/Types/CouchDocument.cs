#nullable disable
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace CouchDB.Driver.Types
{
    /// <summary>
    /// Represents a CouchDB document.
    /// </summary>
    public abstract class CouchDocument
    {
        protected CouchDocument()
        {
            _conflicts = new List<string>();
            AttachmentsParsed = new Dictionary<string, CouchAttachment>();
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
        private string IdOther { set => Id = value; }

        /// <summary>
        /// The current document revision ID.
        /// </summary>
        [DataMember]
        [JsonProperty("_rev", NullValueHandling = NullValueHandling.Ignore)]
        public string Rev { get; set; }
        [DataMember]
        [JsonProperty("rev", NullValueHandling = NullValueHandling.Ignore)]
        private string RevOther { set => Rev = value; }

        [DataMember]
        [JsonProperty("_conflicts")]
        private readonly List<string> _conflicts;

        [JsonIgnore]
        public IReadOnlyCollection<string> Conflicts => _conflicts.AsReadOnly();

        // This must be for serialization only field
        [DataMember]
        [JsonProperty("_attachments", NullValueHandling = NullValueHandling.Ignore)]
        private Dictionary<string, CouchAttachment> AttachmentsParsed { get; set; }

        [JsonIgnore]
        public CouchAttachmentsCollection Attachments { get; internal set; }

        [DataMember]
        [JsonProperty("split_discriminator", NullValueHandling = NullValueHandling.Ignore)]
        internal string SplitDiscriminator { get; set; }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            if (AttachmentsParsed is { Count: > 0 })
            {
                Attachments = new CouchAttachmentsCollection(AttachmentsParsed);
            }
        }
    }
}
#nullable restore