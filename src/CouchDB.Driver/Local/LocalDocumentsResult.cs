using System;
using System.Collections.Generic;
using CouchDB.Driver.Types;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.Local;

[Serializable]
internal class LocalDocumentsResult
{
    [property:JsonPropertyName("rows")]
    public required IList<CouchDocumentInfo> Rows { get; init; }
}