using System.IO;

namespace CouchDB.Driver.Types;

/// <summary>
/// Represents an attachment for a document.
/// </summary>
[Serializable]
public sealed class CouchAttachment
{
    [JsonIgnore]
    internal string DocumentId { get; set; } = null!;

    [JsonIgnore]
    internal string DocumentRev { get; set; } = null!;

    [JsonIgnore]
    internal FileInfo? FileInfo { get; set; }

    [JsonIgnore]
    internal bool Deleted { get; set; }

    [JsonIgnore]
    public string Name { get; set; } = null!;

    [JsonIgnore]
    public Uri Uri { get; set; } = null!;

    /// <summary>
    ///     Gets whether the attachment object contains stub info and no content. 
    /// </summary>
    [property:JsonPropertyName("stub")]
    public bool Stub { get; init; }

    /// <summary>
    ///     Gets the attachment MIME type.
    /// </summary>
    [property:JsonPropertyName("content_type")]
    public string? ContentType { get; set; }

    /// <summary>
    ///     Gets the content hash digest. It starts with prefix which announce hash type (md5-) and continues with
    ///     Base64-encoded hash digest.
    /// </summary>
    [property:JsonPropertyName("digest")]
    public string? Digest { get; init; }

    /// <summary>
    ///     Gets the real attachment size in bytes. Not available if attachment content requested.
    /// </summary>
    [property:JsonPropertyName("length")]
    public long? Length { get; init; }

    /// <summary>
    ///     Gets the revision number when attachment was added.
    /// </summary>
    [property:JsonPropertyName("revpos")]
    public int? RevPos { get; init; }

    /// <summary>
    ///     Gets the compressed attachment size in bytes.
    /// </summary>
    /// <remarks>
    ///     Available if content_type is in list of compressible types when the attachment was added and the following query
    ///     parameters are specified:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>att_encoding_info=true when querying a document</description>
    ///         </item>
    ///         <item>
    ///             <description>att_encoding_info=true&amp;include_docs=true when querying a changes feed or a view</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [property:JsonPropertyName("encoded_length")]
    public long? EncodedLength { get; init; }

    /// <summary>
    ///     Gets the Base64-encoded content. Only populated if queried for and <see cref="Stub"/> is false.
    /// </summary>
    /// <remarks>
    ///     Base64-encoded content. Available if attachment content is requested by using the following query parameters:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>attachments=true when querying a document</description>
    ///         </item>
    ///         <item>
    ///             <description>attachments=true&amp;include_docs=true when querying a changes feed or a view</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [property:JsonPropertyName("data")]
    public string? Data { get; init; }
}