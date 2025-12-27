namespace CouchDB.Driver.Types;

/// <summary>
/// Options for document requests.
/// </summary>
public class ItemRequestOptions
{
    /// <summary>
    /// Stores document in batch mode. Check https://docs.couchdb.org/en/stable/api/database/common.html#api-doc-batch-writes
    /// </summary>
    public bool Batch { get; init; }
}

/// <summary>
/// Options relevant to creating a document.
/// </summary>
public class CreateItemRequestOptions : ItemRequestOptions
{
    /// <summary>
    /// List of attachments to add to the document.
    /// </summary>
    public CreateItemAttachment[]? Attachments { get; init; }
}

/// <summary>
/// Represents an attachment to be added when creating a document.
/// </summary>
/// <param name="FilePath">Path to the file to upload</param>
/// <param name="ContentType">MIME content-type. If not provided, it tries to map it from a list.</param>
/// <param name="Name">Name of the attachment. If not provided, the filename is used</param>
public record CreateItemAttachment(string FilePath, string? ContentType = null, string? Name = null);