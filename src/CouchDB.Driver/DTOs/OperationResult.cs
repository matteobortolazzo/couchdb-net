using System;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.DTOs;

[Serializable]
internal class OperationResult
{
    [JsonPropertyName("ok")]
    public required bool Ok { get; init; }
}