using System;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.DTOs;

[Serializable]
internal class StatusResult
{
    [JsonPropertyName("status")]
    public string Status { get; set; }
}