using System;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.DTOs;

[Serializable]
internal class CouchError
{
    [JsonPropertyName("error")]
    public string? Error { get; set; }
    [JsonPropertyName("reason")]
    public string? Reason { get; set; }
}