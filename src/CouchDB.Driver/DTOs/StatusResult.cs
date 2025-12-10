using System;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.DTOs;

[Serializable]
internal class StatusResult
{
    [JsonPropertyName("status")]
    public required string Status { get; init; }
}