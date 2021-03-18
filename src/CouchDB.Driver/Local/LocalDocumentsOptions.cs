using System.ComponentModel;
using Newtonsoft.Json;

namespace CouchDB.Driver.Local
{
    public class LocalDocumentsOptions
    {
        /// <summary>
        /// Includes conflicts information in response. Ignored if IncludeDocs isn’t <c>True</c>.
        /// </summary>
        [JsonProperty("conflicts")]
        [DefaultValue(false)]
        public bool Conflicts { get; set; }

        /// <summary>
        /// Return the change results in descending sequence order (most recent change first).
        /// </summary>
        [JsonProperty("descending")]
        [DefaultValue(false)]
        public bool Descending { get; set; }

        /// <summary>
        /// Return the change results in descending sequence order (most recent change first).
        /// </summary>
        [JsonProperty("end_key")]
        [DefaultValue(null)]
        public string? EndKey { get; set; }

        /// <summary>
        /// Stop returning records when the specified local document ID is reached.
        /// </summary>
        [JsonProperty("end_key_doc_id")]
        [DefaultValue(null)]
        public string? EndKeyDocId { get; set; }
        
        /// <summary>
        /// Specifies whether the specified end key should be included in the result.
        /// </summary>
        [JsonProperty("include_docs")]
        [DefaultValue(true)]
        public bool InclusiveEnd { get; set; } = true;

        /// <summary>
        /// Return only local documents that match the specified key.
        /// </summary>
        [JsonProperty("key")]
        [DefaultValue(null)]
        public string? Key { get; set; }

        /// <summary>
        /// Limit the number of the returned local documents to the specified number. 
        /// </summary>
        [JsonProperty("limit")]
        [DefaultValue(null)]
        public int? Limit { get; set; }

        /// <summary>
        /// Skip this number of records before starting to return the results.
        /// </summary>
        [JsonProperty("skip ")]
        [DefaultValue(0)]
        public int Skip { get; set; }

        /// <summary>
        /// Return records starting with the specified key.
        /// </summary>
        [JsonProperty("start_key")]
        [DefaultValue(null)]
        public string? StartKey { get; set; }

        /// <summary>
        /// Return records starting with the specified local document ID.
        /// </summary>
        [JsonProperty("start_key_doc_id")]
        [DefaultValue(null)]
        public string? StartKeyDocId { get; set; }
    }
}
