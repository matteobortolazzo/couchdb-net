namespace CouchDB.Driver.Types;

/// <summary>
/// Options relevant to creating a database (supported by PUT HTTP-method).
/// </summary>
public class CreateDatabaseOptions
{
    /// <summary>
    /// The number of range partitions. Default is 8, unless overridden in the cluster config.
    /// </summary>
    public int? Shards { get; set; }

    /// <summary>
    /// The number of copies of the database in the cluster. The default is 3, unless overridden in the cluster config.
    /// </summary>
    public int? Replicas { get; set; }

    /// <summary>
    /// Whether to create a partitioned database. Default is <c>False</c>.
    /// </summary>
    public bool? Partitioned { get; set; }
}