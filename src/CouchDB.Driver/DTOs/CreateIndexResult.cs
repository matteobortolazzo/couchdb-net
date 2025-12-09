using System;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.DTOs;

[Serializable]
internal class CreateIndexResult
{
    [JsonPropertyName("result")]
    public string Result { get; set; }
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
}