namespace CouchDB.Driver.DTOs;

[Serializable]
internal class OperationResult
{
    [property:JsonPropertyName("ok")]
    public bool Ok { get; init; }
}