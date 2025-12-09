using System;
using System.Collections.Generic;
using CouchDB.Driver.Types;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.Local;

[Serializable]
internal class LocalDocumentsResult
{
    [JsonPropertyName("rows")]
    public IList<CouchDocumentInfo> Rows { get; set; }
}