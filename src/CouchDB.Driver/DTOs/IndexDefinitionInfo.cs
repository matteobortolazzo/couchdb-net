namespace CouchDB.Driver.DTOs;

[Serializable]
internal sealed record IndexDefinitionInfo(
    [property: JsonPropertyName("fields")]
    Dictionary<string, string>[] Fields);