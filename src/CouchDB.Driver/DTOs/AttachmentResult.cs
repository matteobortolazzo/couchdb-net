using System;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.DTOs;

[Serializable]
internal class AttachmentResult
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    [JsonPropertyName("rev")]
    public required string Rev { get; set; }
}