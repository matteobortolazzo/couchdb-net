using System;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.Types;

/// <summary>
/// Represents a CouchDB document info.
/// </summary>
[Serializable]
public class CouchDocumentInfo
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("key")]
    public required string Key { get; init; }

    [JsonPropertyName("value")]
    private CouchDocumentInfoValue Value { get; set; } = null!;

    public string Rev => Value.Rev;

    [Serializable]
    private class CouchDocumentInfoValue
    {
        [JsonPropertyName("rev")]
        public required string Rev { get; init; }
    }
}