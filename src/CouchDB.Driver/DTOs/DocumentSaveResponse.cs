namespace CouchDB.Driver.DTOs;

[Serializable]
internal class DocumentSaveResponse
{    
    [property:JsonPropertyName("ok")]
    public bool Ok { get; init; }
    [property:JsonPropertyName("id")]
    public string? Id { get; init; }
    [property:JsonPropertyName("rev")]
    public string? Rev { get; init; }
    [property:JsonPropertyName("error")]
    public string? Error { get; init; }
    [property:JsonPropertyName("reason")]
    public string? Reason { get; init; }
}