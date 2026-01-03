namespace CouchDB.Driver.Types;

/// <summary>
/// Options relevant to creating a database (supported by PUT HTTP-method).
/// </summary>
[Serializable]
public class CreateDatabaseOptions
{
    /// <summary>
    /// The number of range partitions. Default is 8, unless overridden in the cluster config.
    /// </summary>
    [JsonPropertyName("q")]
    public int? Shards { get; init; }

    /// <summary>
    /// The number of copies of the database in the cluster. The default is 3, unless overridden in the cluster config.
    /// </summary>
    [JsonPropertyName("n")]
    public int? Replicas { get; init; }

    /// <summary>
    /// Whether to create a partitioned database. Default is <c>False</c>.
    /// </summary>
    [JsonIgnore]
    public bool? Partitioned { get; init; }
}