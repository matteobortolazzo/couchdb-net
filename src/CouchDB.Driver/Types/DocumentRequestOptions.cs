namespace CouchDB.Driver.Types;

/// <summary>
/// Options for document requests.
/// </summary>
public class DocumentRequestOptions
{
    /// <summary>
    /// Stores document in batch mode. Check https://docs.couchdb.org/en/stable/api/database/common.html#api-doc-batch-writes
    /// </summary>
    public bool Batch { get; init; }
}
