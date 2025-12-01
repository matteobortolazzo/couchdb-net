using Newtonsoft.Json;

namespace CouchDB.Driver.Types
{
    /// <summary>
    /// Represents information about a specific partition in a partitioned database.
    /// </summary>
    public sealed class CouchPartitionInfo
    {
        /// <summary>
        /// The name of the database.
        /// </summary>
        [JsonProperty("db_name")]
        public string DbName { get; internal set; } = string.Empty;

        /// <summary>
        /// A count of the documents in the specified partition.
        /// </summary>
        [JsonProperty("doc_count")]
        public int DocCount { get; internal set; }

        /// <summary>
        /// Number of deleted documents in the partition.
        /// </summary>
        [JsonProperty("doc_del_count")]
        public int DocDelCount { get; internal set; }

        /// <summary>
        /// The partition key.
        /// </summary>
        [JsonProperty("partition")]
        public string Partition { get; internal set; } = string.Empty;

        /// <summary>
        /// Size information for the partition.
        /// </summary>
        [JsonProperty("sizes")]
        public Sizes Sizes { get; internal set; } = new Sizes();
    }
}
