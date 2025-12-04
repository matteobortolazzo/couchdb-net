using System.Text.Json.Serialization;

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
        [JsonPropertyName("db_name")]
        public string DbName { get; internal set; } = string.Empty;

        /// <summary>
        /// A count of the documents in the specified partition.
        /// </summary>
        [JsonPropertyName("doc_count")]
        public int DocCount { get; internal set; }

        /// <summary>
        /// Number of deleted documents in the partition.
        /// </summary>
        [JsonPropertyName("doc_del_count")]
        public int DocDelCount { get; internal set; }

        /// <summary>
        /// The partition key.
        /// </summary>
        [JsonPropertyName("partition")]
        public string Partition { get; internal set; } = string.Empty;

        /// <summary>
        /// Size information for the partition.
        /// </summary>
        [JsonPropertyName("sizes")]
        public Sizes Sizes { get; internal set; } = new Sizes();
    }
}
