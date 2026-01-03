namespace CouchDB.Driver.Types;

[Serializable]
public record Replication 
{
    [property:JsonPropertyName("source")]
    public object? Source { get; set; }

    public ReplicationBasicCredentials? SourceCredentials { get; init; }

    [property:JsonPropertyName("target")]
    public object? Target { get; set; }

    public ReplicationBasicCredentials? TargetCredentials { get; init; }

    [property:JsonPropertyName("continuous")]
    public bool Continuous { get; init; }

    [property:JsonPropertyName("selector")]
    public object? Selector { get; init; }

    [property:JsonPropertyName("cancel")]
    public bool Cancel { get; set; }
        
    [property:JsonPropertyName("create_target")]
    public bool CreateTarget{ get; init; }
}