namespace CouchDB.Driver.DTOs;

[Serializable]
internal class CreateIndexResult
{
    [property:JsonPropertyName("result")]
    public required string Result { get; init; }
    [property:JsonPropertyName("id")]
    public required string Id { get; init; }
    [property:JsonPropertyName("name")]
    public required string Name { get; init; }
}