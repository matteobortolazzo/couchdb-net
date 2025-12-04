#nullable disable
using System.Collections.Generic;
using System.Runtime.Serialization;
using CouchDB.Driver.Converters;
using CouchDB.Driver.DatabaseApiMethodOptions;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.Types
{
    /// <summary>
    /// Represents a CouchDB document.
    /// </summary>
    public abstract class CouchDocument
    {
        protected CouchDocument()
        {
            AttachmentsParsed = new Dictionary<string, CouchAttachment>();
            Attachments = new CouchAttachmentsCollection();
        }

        /// <summary>
        /// The document ID.
        /// </summary>
        [DataMember, JsonProperty("_id", NullValueHandling = NullValueHandling.Ignore)]
        public virtual string Id { get; set; }

        [DataMember, JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        private string IdOther { set => Id = value; }

        /// <summary>
        /// The current document revision ID.
        /// </summary>
        [DataMember, JsonProperty("_rev", NullValueHandling = NullValueHandling.Ignore)]
        public string Rev { get; set; }

        [DataMember, JsonProperty("rev", NullValueHandling = NullValueHandling.Ignore)]
        private string RevOther { set => Rev = value; }

        /// <summary>
        /// Deletion flag.
        /// Available if document was removed.
        /// </summary>
        [DataMember, JsonIgnore]
        public bool Deleted { get; private set; }

        [DataMember, JsonProperty("_deleted", NullValueHandling = NullValueHandling.Ignore)]
        private bool DeletedOther { set => Deleted = value; }

        /// <summary>
        /// List of conflicted revisions.
        /// Available if requested with <see cref="FindOptions.Conflicts"/> set to <c>True</c>
        /// </summary>
        [JsonIgnore]
        public IReadOnlyCollection<string> Conflicts { get; private set; }

        [DataMember, JsonProperty("_conflicts", NullValueHandling = NullValueHandling.Ignore)]
        private List<string> ConflictsOther { set { Conflicts = value?.AsReadOnly(); } }

        /// <summary>
        /// List of deleted conflicted revisions.
        /// Available if requested with <see cref="FindOptions.DeleteConflicts"/> set to <c>True</c>
        /// </summary>
        [JsonIgnore]
        public IReadOnlyCollection<string> DeletedConflicts { get; private set; }

        [DataMember, JsonProperty("_deleted_conflicts", NullValueHandling = NullValueHandling.Ignore)]
        private List<string> DeletedConflictsOther { set { DeletedConflicts = value?.AsReadOnly(); } }

        /// <summary>
        /// Document’s update sequence in current database.
        /// Available if requested with <see cref="FindOptions.LocalSequence"/> set to <c>True</c>
        /// </summary>
        [JsonIgnore]
        public int LocalSequence { get; private set; }

        [DataMember, JsonProperty("_localSeq", NullValueHandling = NullValueHandling.Ignore)]
        private int LocalSequenceOther { set { LocalSequence = value; } }

        /// <summary>
        /// List of objects with information about local revisions and their status.
        /// Available if requested with <see cref="FindOptions.OpenRevisions"/>
        /// </summary>
        [JsonIgnore]
        public IReadOnlyCollection<RevisionInfo> RevisionsInfo { get; private set; }

        [DataMember, JsonProperty("_revs_info", NullValueHandling = NullValueHandling.Ignore)]
        private List<RevisionInfo> RevisionsInfoOther { set { RevisionsInfo = value?.AsReadOnly(); } }

        /// <summary>
        /// List of local revision tokens without.
        /// Available if requested with <see cref="FindOptions.Revisions"/> set to <c>True</c>
        /// </summary>
        [JsonIgnore]
        public Revisions Revisions { get; private set; }

        [DataMember, JsonProperty("_revisions", NullValueHandling = NullValueHandling.Ignore)]
        private Revisions RevisionsOther { set { Revisions = value; } }


        /// <summary>
        /// Attachment’s stubs. Available if document has any attachments
        /// </summary>
        [JsonIgnore]
        public CouchAttachmentsCollection Attachments { get; private set; }

        [DataMember, JsonProperty("_attachments", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(AttachmentsParsedConverter))]
        private Dictionary<string, CouchAttachment> AttachmentsParsed { get; set; }

        /// <summary>
        /// Used for database splitting
        /// </summary>
        [DataMember]
        [JsonPropertyName(CouchClient.DefaultDatabaseSplitDiscriminator, NullValueHandling = NullValueHandling.Ignore)]
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