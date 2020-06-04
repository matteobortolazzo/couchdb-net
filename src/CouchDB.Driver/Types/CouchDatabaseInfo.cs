#nullable disable
using Newtonsoft.Json;

namespace CouchDB.Driver.Types
{
    /// <summary>
    /// Represents size information.
    /// </summary>
    public class Sizes
    {
        /// <summary>
        /// The size of the database file on disk in bytes. Views indexes are not included in the calculation.
        /// </summary>
        [JsonProperty("file")]
        public int File { get; internal set; }

        /// <summary>
        /// The uncompressed size of database contents in bytes.
        /// </summary>
        [JsonProperty("external")]
        public int External { get; internal set; }

        /// <summary>
        /// The size of live data inside the database, in bytes.
        /// </summary>
        [JsonProperty("active")]
        public int Active { get; internal set; }
    }

    /// <summary>
    /// Represents cluster information.
    /// </summary>
    public class Cluster
    {
        /// <summary>
        /// The number of copies of every document.
        /// </summary>
        [JsonProperty("n")]
        public int Replicas { get; internal set; }

        /// <summary>
        /// The number of range partitions.
        /// </summary>
        [JsonProperty("q")]
        public int Shards { get; internal set; }

        /// <summary>
        /// The number of consistent copies of a document that need to be read before a successful reply.
        /// </summary>
        [JsonProperty("r")]
        public int ReadQuorum { get; internal set; }

        /// <summary>
        /// The number of copies of a document that need to be written before a successful reply.
        /// </summary>
        [JsonProperty("w")]
        public int WriteQuorum { get; internal set; }        
    }

    /// <summary>
    /// Represents information a specific database.
    /// </summary>
    public class CouchDatabaseInfo
    {
        /// <summary>
        /// Cluster information.
        /// </summary>
        [JsonProperty("cluster")]
        public Cluster Cluster { get; internal set; }

        /// <summary>
        /// Set to true if the database compaction routine is operating on this database.
        /// </summary>
        [JsonProperty("compact_running")]
        public bool CompactRunning { get; internal set; }

        /// <summary>
        /// The name of the database.
        /// </summary>
        [JsonProperty("db_name")]
        public string DbName { get; internal set; }        

        /// <summary>
        /// The version of the physical format used for the data when it is stored on disk.
        /// </summary>
        [JsonProperty("disk_format_version")]
        public int DiskFormatVersion { get; internal set; }

        /// <summary>
        /// A count of the documents in the specified database.
        /// </summary>
        [JsonProperty("doc_count")]
        public int DocCount { get; internal set; }

        /// <summary>
        /// Number of deleted documents.
        /// </summary>
        [JsonProperty("doc_del_count")]
        public int DocDelCount { get; internal set; }

        /// <summary>
        /// An opaque string that describes the purge state of the database. Do not rely on this string for counting the number of purge operations.
        /// </summary>
        [JsonProperty("purge_seq")]
        public int PurgeSeq { get; internal set; }

        /// <summary>
        /// Size information
        /// </summary>
        [JsonProperty("sizes")]
        public Sizes Sizes { get; internal set; }

        /// <summary>
        /// An opaque string that describes the state of the database. Do not rely on this string for counting the number of updates.
        /// </summary>
        [JsonProperty("update_seq")]
        public string UpdateSeq { get; internal set; }
    }
}
#nullable restore