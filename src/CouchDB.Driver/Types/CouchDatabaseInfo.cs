namespace CouchDB.Driver.Types;

/// <summary>
/// Represents information a specific database.
/// </summary>
/// <param name="Cluster">Cluster information.</param>
/// <param name="CompactRunning">Set to true if the database compaction routine is operating on this database.</param>
/// <param name="DbName">The name of the database.</param>
/// <param name="DiskFormatVersion">The version of the physical format used for the data when it is stored on disk.</param>
/// <param name="DocCount">A count of the documents in the specified database.</param>
/// <param name="DocDelCount">Number of deleted documents.</param>
/// <param name="PurgeSeq">An opaque string that describes the purge state of the database. Do not rely on this string for counting the number of purge operations.</param>
/// <param name="Sizes">Size information.</param>
/// <param name="UpdateSeq">An opaque string that describes the state of the database. Do not rely on this string for counting the number of updates.</param>
/// <param name="Props">Database properties.</param>
[Serializable]
public sealed record CouchDatabaseInfo(
    [property: JsonPropertyName("cluster")]
    Cluster Cluster,
    [property: JsonPropertyName("compact_running")]
    bool CompactRunning,
    [property: JsonPropertyName("db_name")]
    string DbName,
    [property: JsonPropertyName("disk_format_version")]
    int DiskFormatVersion,
    [property: JsonPropertyName("doc_count")]
    int DocCount,
    [property: JsonPropertyName("doc_del_count")]
    int DocDelCount,
    [property: JsonPropertyName("purge_seq")]
    string PurgeSeq,
    [property: JsonPropertyName("sizes")]
    Sizes Sizes,
    [property: JsonPropertyName("update_seq")]
    string UpdateSeq,
    [property: JsonPropertyName("props")]
    DatabaseProps Props
);

/// <summary>
/// Represents database properties.
/// </summary>
/// <param name="Partitioned">Indicates whether the database is partitioned.</param>
[Serializable]
public sealed record DatabaseProps(
    [property: JsonPropertyName("partitioned")]
    bool Partitioned
);
