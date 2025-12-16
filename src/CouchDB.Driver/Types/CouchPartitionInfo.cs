namespace CouchDB.Driver.Types;

/// <summary>
/// Represents information about a specific partition in a partitioned database.
/// </summary>
/// <param name="DbName">The name of the database.</param>
/// <param name="DocCount">A count of the documents in the specified partition.</param>
/// <param name="DocDelCount">Number of deleted documents in the partition.</param>
/// <param name="Partition">The partition key.</param>
/// <param name="Sizes">Size information for the partition.</param>
[Serializable]
public sealed record CouchPartitionInfo(
    [property: JsonPropertyName("db_name")]
    string DbName,
    [property: JsonPropertyName("doc_count")]
    int DocCount,
    [property: JsonPropertyName("doc_del_count")]
    int DocDelCount,
    [property: JsonPropertyName("partition")]
    string Partition,
    [property: JsonPropertyName("sizes")]
    Sizes Sizes
);