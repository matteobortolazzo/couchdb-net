using System;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.DTOs;

[Serializable]
internal class AttachmentResult
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    [JsonPropertyName("rev")]
    public string Rev { get; set; }
}