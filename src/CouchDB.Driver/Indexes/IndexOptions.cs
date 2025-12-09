namespace CouchDB.Driver.Indexes;

public class IndexOptions
{
    public string? DesignDocument { get; init; }
    public bool? Partitioned { get; init; }
}