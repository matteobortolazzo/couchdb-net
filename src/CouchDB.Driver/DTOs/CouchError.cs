namespace CouchDB.Driver.DTOs;

[Serializable]
internal class CouchError
{
    [property:JsonPropertyName("error")]
    public string? Error { get; init; }
    [property:JsonPropertyName("reason")]
    public string? Reason { get; init; }
}