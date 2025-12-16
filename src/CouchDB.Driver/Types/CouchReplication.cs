namespace CouchDB.Driver.Types;

// TODO: Review
[Serializable]
public class CouchReplication : CouchDocument
{
    [property:JsonPropertyName("source")]
    public object? Source { get; set; }

    public CouchReplicationBasicCredentials? SourceCredentials { get; init; }

    [property:JsonPropertyName("target")]
    public object? Target { get; set; }

    public CouchReplicationBasicCredentials? TargetCredentials { get; init; }

    [property:JsonPropertyName("continuous")]
    public bool Continuous { get; init; }

    [property:JsonPropertyName("selector")]
    public object? Selector { get; init; }

    [property:JsonPropertyName("cancel")]
    public bool Cancel { get; set; }
        
    [property:JsonPropertyName("create_target")]
    public bool CreateTarget{ get; init; }
}