using System;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.Types;

/// <summary>
/// Represents information a specific database.
/// </summary>
[Serializable]
public sealed class CouchDatabaseInfo
{
    /// <summary>
    /// Cluster information.
    /// </summary>
    [JsonPropertyName("cluster")]
    public required Cluster Cluster { get; init; }

    /// <summary>
    /// Set to true if the database compaction routine is operating on this database.
    /// </summary>
    [JsonPropertyName("compact_running")]
    public bool CompactRunning { get; init; }

    /// <summary>
    /// The name of the database.
    /// </summary>
    [JsonPropertyName("db_name")]
    public required string DbName { get; init; }        

    /// <summary>
    /// The version of the physical format used for the data when it is stored on disk.
    /// </summary>
    [JsonPropertyName("disk_format_version")]
    public int DiskFormatVersion { get; init; }

    /// <summary>
    /// A count of the documents in the specified database.
    /// </summary>
    [JsonPropertyName("doc_count")]
    public int DocCount { get; init; }

    /// <summary>
    /// Number of deleted documents.
    /// </summary>
    [JsonPropertyName("doc_del_count")]
    public int DocDelCount { get; init; }

    /// <summary>
    /// An opaque string that describes the purge state of the database. Do not rely on this string for counting the number of purge operations.
    /// </summary>
    [JsonPropertyName("purge_seq")]
    public required string PurgeSeq { get; init; }

    /// <summary>
    /// Size information
    /// </summary>
    [JsonPropertyName("sizes")]
    public required Sizes Sizes { get; init; }

    /// <summary>
    /// An opaque string that describes the state of the database. Do not rely on this string for counting the number of updates.
    /// </summary>
    [JsonPropertyName("update_seq")]
    public required string UpdateSeq { get; init; }

    /// <summary>
    /// Indicates whether the database is partitioned or not.
    /// </summary>
    [JsonPropertyName("props")]
    public required DatabaseProps Props { get; init; }
}

/// <summary>
/// Represents database properties.
/// </summary>
[Serializable]
public sealed class DatabaseProps
{
    /// <summary>
    /// Indicates whether the database is partitioned.
    /// </summary>
    [JsonPropertyName("partitioned")]
    public bool Partitioned { get; init; }
}