using System;
using CouchDB.Driver.Types;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.DTOs;

[Serializable]
internal class BulkGetResultItem<TSource> where TSource : CouchDocument
{
    [JsonPropertyName("ok")]
    public TSource? Item { get; set; }
}