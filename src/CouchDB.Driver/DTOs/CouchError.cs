using System;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.DTOs;

[Serializable]
internal class CouchError
{
    [JsonPropertyName("error")]
    public string? Error { get; init; }
    [JsonPropertyName("reason")]
    public string? Reason { get; init; }
}