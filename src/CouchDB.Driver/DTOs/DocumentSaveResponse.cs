using System;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.DTOs;

[Serializable]
internal class DocumentSaveResponse
{    
    [JsonPropertyName("ok")]
    public bool Ok { get; init; }
    [JsonPropertyName("id")]
    public required string Id { get; init; }
    [JsonPropertyName("rev")]
    public required string Rev { get; init; }
    [JsonPropertyName("error")]
    public required string Error { get; init; }
    [JsonPropertyName("reason")]
    public required string Reason { get; init; }
}