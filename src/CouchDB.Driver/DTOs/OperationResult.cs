using System;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.DTOs;

[Serializable]
internal class OperationResult
{
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }
}