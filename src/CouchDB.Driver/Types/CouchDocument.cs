#nullable disable
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace CouchDB.Driver.Types
{
    /// <summary>
    /// Represents a CouchDB document.
    /// </summary>
    public abstract class CouchDocument: CouchDocumentBase
    {
        protected CouchDocument()
        {
            _attachments = new Dictionary<string, CouchAttachment>();
            Attachments = new CouchAttachmentsCollection();
        }

        // This must be Deserilizable-only field
        [JsonIgnore]
        private Dictionary<string, CouchAttachment> _attachments;
        [DataMember]
        [JsonProperty("_attachments")]
        private Dictionary<string, CouchAttachment> AttachmentsSetter
        {
            set { _attachments = value; }
        }

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
#nullable restore