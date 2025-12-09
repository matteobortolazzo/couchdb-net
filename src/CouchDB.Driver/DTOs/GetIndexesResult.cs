using System;
using System.Collections.Generic;
using CouchDB.Driver.Types;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.DTOs;

[Serializable]
internal class GetIndexesResult
{
    [JsonPropertyName("indexes")]
    public required List<IndexInfo> Indexes { get; init; }
}