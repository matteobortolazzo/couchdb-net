namespace CouchDB.Driver.Types;

/// <summary>
/// Options relevant to replication operations.
/// </summary>
public record ConfigureReplicationOptions : ReplicationOptions
{
    /// <summary>
    /// Configure the replication to be continuous
    /// </summary>
    public bool Continuous { get; init; }

    /// <summary>
    /// Creates the target database. Required administrator’s privileges on target server.
    /// </summary>
    public bool CreateTarget { get; init; }

    /// <summary>
    /// Parameters to be used when creating the target database
    /// </summary>
    public CreateDatabaseOptions? CreateTargetParams { get; init; }

    /// <summary>
    /// Replicate winning revisions only.
    /// </summary>
    public bool WinningRevOnly { get; init; }

    /// <summary>
    /// Array of document IDs to be synchronized. 
    /// </summary>
    public string[]? DocIds { get; init; }

    /// <summary>
    /// The name of a filter function.
    /// </summary>
    public string? Filter { get; init; }

    /// <summary>
    ///  A selector to filter documents for synchronization. Has the same behavior as the selector objects in replication documents. 
    /// </summary>
    public object? Selector { get; init; }
}

/// <summary>
/// Options relevant to replication operations.
/// </summary>
public record ReplicationOptions
{
    /// <summary>
    /// Address of a proxy server through which replication from the source should occur.
    /// </summary>
    public string? SourceProxy { get; init; }

    /// <summary>
    /// Address of a proxy server through which replication to the target should occur.
    /// </summary>
    public string? TargetProxy { get; init; }

    /// <summary>
    /// Credentials for accessing the source database.
    /// </summary>
    public ReplicationBasicCredentials? SourceCredentials { get; init; }

    /// <summary>
    /// Credentials for accessing the target database.
    /// </summary>
    public ReplicationBasicCredentials? TargetCredentials { get; init; }
}