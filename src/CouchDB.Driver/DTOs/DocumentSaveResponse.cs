using System;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.DTOs;

[Serializable]
internal class DocumentSaveResponse
{    
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("rev")]
    public string Rev { get; set; }
    [JsonPropertyName("error")]
    public string Error { get; set; }
    [JsonPropertyName("reason")]
    public string Reason { get; set; }
}