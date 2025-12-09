using System;
using CouchDB.Driver.Types;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace CouchDB.Driver.DTOs;

[Serializable]
internal class BulkGetResult<TSource> where TSource : CouchDocument
{
    [JsonPropertyName("results")]
    public List<BulkGetResultDoc<TSource>> Results { get; set; }
}