namespace CouchDB.Driver.Types;

/// <summary>
/// Represents an attachment for a document.
/// </summary>
/// <param name="Stub">Whether the attachment object contains stub info and no content.</param>
/// <param name="ContentType">The attachment MIME type.</param>
/// <param name="Digest">The content hash digest. It starts with prefix which announce hash type (md5-) and continues with Base64-encoded hash digest.</param>
/// <param name="Length">The real attachment size in bytes. Not available if attachment content requested.</param>
/// <param name="RevPos">The revision number when attachment was added.</param>
/// <param name="Encoding">
/// The compression codec.
/// Available if content_type is in list of compressible types when the attachment was added and the following query parameters are specified:
/// <list type="bullet">
///     <item>
///         <description><code><see creGetItemOptionsons.AttachmentsEncodingInfo"/>=<c>True</c></code> when querying a document</description>
///     </item>
///     <item>
///         <description>att_encoding_info=true&amp;include_docs=true when querying a changes feed or a view</description>
///     </item>
/// </list>
/// </param>
/// <param name="EncodedLength">
/// The compressed attachment size in bytes.
/// Available if content_type is in list of compressible types when the attachment was added and the following query parameters are specified:
/// <list type="bullet">
///     <item>
///         <description><code><see creGetItemOptionsons.AttachmentsEncodingInfo"/>=<c>True</c></code> when querying a document</description>
///     </item>
///     <item>
///         <description>att_encoding_info=true&amp;include_docs=true when querying a changes feed or a view</description>
///     </item>
/// </list>
/// </param>
/// <param name="Data">
/// The Base64-encoded content. Only populated if queried for and <see cref="Stub"/> is false.
/// Available if attachment content is requested by using the following query parameters:
/// <list type="bullet">
///     <item>
///         <description><code><see creGetItemOptionsons.Attachments"/>=<c>True</c></code> when querying a document</description>
///     </item>
///     <item>
///         <description>attachments=true&amp;include_docs=true when querying a changes feed or a view</description>
///     </item>
/// </list>
/// </param>
[Serializable]
public sealed record CouchAttachment(
    [property: JsonPropertyName("stub")]
    bool Stub,
    [property: JsonPropertyName("content_type")]
    string ContentType,
    [property: JsonPropertyName("digest")]
    string Digest,
    [property: JsonPropertyName("length")]
    long Length,
    [property: JsonPropertyName("revpos")]
    int RevPos,
    [property: JsonPropertyName("encoding")]
    string? Encoding,
    [property: JsonPropertyName("encoded_length")]
    long? EncodedLength,
    [property: JsonPropertyName("data")]
    string? Data
);