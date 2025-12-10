namespace CouchDB.Driver.DatabaseApiMethodOptions;

/// <summary>
/// Options relevant to saving a document (supported by PUT HTTP-method).
/// Check https://docs.couchdb.org/en/stable/api/document/common.html#put--db-docid
/// </summary>
public class AddOrUpdateOptions
{
    /// <summary>
    /// Stores document in batch mode. Check https://docs.couchdb.org/en/stable/api/database/common.html#api-doc-batch-writes
    /// </summary>
    public bool Batch { get; init; }

    /// <summary>
    /// Document’s revision if updating an existing document.
    /// </summary>
    public string? Rev { get; init; }
}