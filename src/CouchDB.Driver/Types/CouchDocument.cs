using CouchDB.Driver.Converters;
using CouchDB.Driver.DatabaseApiMethodOptions;

namespace CouchDB.Driver.Types;

/// <summary>
/// Represents a CouchDB document.
/// </summary>
[Serializable]
public abstract class CouchDocument
{
    /// <summary>
    /// List of conflicted revisions.
    /// Available if requested with <see cref="FindDocumentRequestOptions.Conflicts"/> set to <c>True</c>
    /// </summary>
    [JsonIgnore]
    public IReadOnlyCollection<string> Conflicts { get; private set; } = [];

    [JsonInclude]
    [property:JsonPropertyName("_conflicts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    private List<string> ConflictsOther { set { Conflicts = value.AsReadOnly(); } }

    /// <summary>
    /// List of deleted conflicted revisions.
    /// Available if requested with <see cref="FindDocumentRequestOptions.DeleteConflicts"/> set to <c>True</c>
    /// </summary>
    [JsonIgnore]
    public IReadOnlyCollection<string> DeletedConflicts { get; private set; } = [];

    [JsonInclude]
    [property:JsonPropertyName("_deleted_conflicts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    private List<string> DeletedConflictsOther { set { DeletedConflicts = value.AsReadOnly(); } }

    /// <summary>
    /// Document’s update sequence in current database.
    /// Available if requested with <see cref="FindDocumentRequestOptions.LocalSequence"/> set to <c>True</c>
    /// </summary>
    [JsonIgnore]
    public int LocalSequence { get; private set; }

    [JsonInclude]
    [property:JsonPropertyName("_localSeq")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    private int LocalSequenceOther { set { LocalSequence = value; } }

    /// <summary>
    /// List of objects with information about local revisions and their status.
    /// Available if requested with <see cref="FindDocumentRequestOptions.OpenRevisions"/>
    /// </summary>
    [JsonIgnore]
    public IReadOnlyCollection<RevisionInfo> RevisionsInfo { get; private set; } = [];

    [JsonInclude]
    [property:JsonPropertyName("_revs_info")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    private List<RevisionInfo> RevisionsInfoOther { set { RevisionsInfo = value.AsReadOnly(); } }

    /// <summary>
    /// List of local revision tokens without.
    /// Available if requested with <see cref="FindDocumentRequestOptions.Revisions"/> set to <c>True</c>
    /// </summary>
    [JsonIgnore]
    public Revisions Revisions { get; private set; } = null!;

    [JsonInclude]
    [property:JsonPropertyName("_revisions")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    private Revisions RevisionsOther { set { Revisions = value; } }

    /// <summary>
    /// Attachment’s stubs. Available if document has any attachments
    /// </summary>
    [JsonIgnore]
    public CouchAttachmentsCollection Attachments { get; set; } = new();

    [JsonInclude]
    [property:JsonPropertyName("_attachments")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter(typeof(AttachmentsParsedConverter))]
    internal Dictionary<string, CouchAttachment> AttachmentsParsed { get; set; } = new();

    /// <summary>
    /// Used for database splitting
    /// </summary>
    [JsonInclude]
    [property:JsonPropertyName(CouchClient.DefaultDatabaseSplitDiscriminator)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    internal string? SplitDiscriminator { get; set; }
}