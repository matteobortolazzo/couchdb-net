#nullable disable
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace CouchDB.Driver.Types
{
    /// <summary>
    /// Represents a local CouchDB document.
    /// </summary>
    public class LocalCouchDocument: CouchDocumentBase
    {
        [DataMember]
        [JsonProperty("update_seq")]
        public int? UpdateSeq { get; set; }
    }
}
#nullable enable