#nullable disable
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace CouchDB.Driver.Types
{
    /// <summary>
    /// Represents a base CouchDB document.
    /// </summary>
    public abstract class CouchDocumentBase
    {
        protected CouchDocumentBase()
        {
            _conflicts = new List<string>();
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
    }
}
#nullable restore