#nullable disable
using System.Text.Json.Serialization;

namespace CouchDB.Driver.Types
{
    /// <summary>
    /// Represents information a specific database.
    /// </summary>
    public sealed class CouchDatabaseInfo
    {
        /// <summary>
        /// Cluster information.
        /// </summary>
        [JsonPropertyName("cluster")]
        public Cluster Cluster { get; internal set; }

        /// <summary>
        /// Set to true if the database compaction routine is operating on this database.
        /// </summary>
        [JsonPropertyName("compact_running")]
        public bool CompactRunning { get; internal set; }

        /// <summary>
        /// The name of the database.
        /// </summary>
        [JsonPropertyName("db_name")]
        public string DbName { get; internal set; }        

        /// <summary>
        /// The version of the physical format used for the data when it is stored on disk.
        /// </summary>
        [JsonPropertyName("disk_format_version")]
        public int DiskFormatVersion { get; internal set; }

        /// <summary>
        /// A count of the documents in the specified database.
        /// </summary>
        [JsonPropertyName("doc_count")]
        public int DocCount { get; internal set; }

        /// <summary>
        /// Number of deleted documents.
        /// </summary>
        [JsonPropertyName("doc_del_count")]
        public int DocDelCount { get; internal set; }

        /// <summary>
        /// An opaque string that describes the purge state of the database. Do not rely on this string for counting the number of purge operations.
        /// </summary>
        [JsonPropertyName("purge_seq")]
        public string PurgeSeq { get; internal set; }

        /// <summary>
        /// Size information
        /// </summary>
        [JsonPropertyName("sizes")]
        public Sizes Sizes { get; internal set; }

        /// <summary>
        /// An opaque string that describes the state of the database. Do not rely on this string for counting the number of updates.
        /// </summary>
        [JsonPropertyName("update_seq")]
        public string UpdateSeq { get; internal set; }

        /// <summary>
        /// Indicates whether the database is partitioned or not.
        /// </summary>
        [JsonPropertyName("props")]
        public DatabaseProps Props { get; internal set; }
    }

    /// <summary>
    /// Represents database properties.
    /// </summary>
    public sealed class DatabaseProps
    {
        /// <summary>
        /// Indicates whether the database is partitioned.
        /// </summary>
        [JsonPropertyName("partitioned")]
        public bool Partitioned { get; internal set; }
    }
}
#nullable restore