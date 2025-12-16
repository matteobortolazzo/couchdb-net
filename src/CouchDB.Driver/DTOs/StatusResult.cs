namespace CouchDB.Driver.DTOs;

[Serializable]
internal class StatusResult
{
    [property:JsonPropertyName("status")]
    public required string Status { get; init; }
}