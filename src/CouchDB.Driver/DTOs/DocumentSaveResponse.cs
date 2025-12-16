using System;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.DTOs;

[Serializable]
internal class DocumentSaveResponse
{    
    [JsonPropertyName("ok")]
    public bool Ok { get; init; }
    [JsonPropertyName("id")]
    public string? Id { get; init; }
    [JsonPropertyName("rev")]
    public string? Rev { get; init; }
    [JsonPropertyName("error")]
    public string? Error { get; init; }
    [JsonPropertyName("reason")]
    public string? Reason { get; init; }
}