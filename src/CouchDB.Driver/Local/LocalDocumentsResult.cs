using CouchDB.Driver.Types;

namespace CouchDB.Driver.Local;

[Serializable]
internal class LocalDocumentsResult
{
    [property:JsonPropertyName("rows")]
    public required IList<DocumentInfo> Rows { get; init; }
}