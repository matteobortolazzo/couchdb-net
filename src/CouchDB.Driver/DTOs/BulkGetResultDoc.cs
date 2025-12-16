using System;
using System.Collections.Generic;
using CouchDB.Driver.Types;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.DTOs;

[Serializable]
internal class BulkGetResultDoc<TSource> where TSource : CouchDocument
{
    [property:JsonPropertyName("docs")]
    public required List<BulkGetResultItem<TSource>> Docs { get; init; }
}