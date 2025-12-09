using System;
using System.Collections.Generic;
using CouchDB.Driver.Converters;
using CouchDB.Driver.DatabaseApiMethodOptions;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.Types;

/// <summary>
/// Represents a CouchDB document.
/// </summary>
[Serializable]
public abstract class CouchDocument
{
    protected CouchDocument()
    {
        AttachmentsParsed = new Dictionary<string, CouchAttachment>();
        Attachments = new CouchAttachmentsCollection();
    }

    /// <summary>
    /// The document ID.
    /// </summary>
    [JsonPropertyName("_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public virtual string Id { get; set; }

    [JsonPropertyName("id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    private string IdOther { set => Id = value; }

    /// <summary>
    /// The current document revision ID.
    /// </summary>
    [JsonPropertyName("_rev")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Rev { get; set; }

    [JsonPropertyName("rev")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    private string RevOther { set => Rev = value; }

    /// <summary>
    /// Deletion flag.
    /// Available if document was removed.
    /// </summary>
    [JsonIgnore]
    public bool Deleted { get; private set; }

    [JsonPropertyName("_deleted")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    private bool DeletedOther { set => Deleted = value; }

    /// <summary>
    /// List of conflicted revisions.
    /// Available if requested with <see cref="FindOptions.Conflicts"/> set to <c>True</c>
    /// </summary>
    [JsonIgnore]
    public IReadOnlyCollection<string> Conflicts { get; private set; }

    [JsonPropertyName("_conflicts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    private List<string> ConflictsOther { set { Conflicts = value.AsReadOnly(); } }

    /// <summary>
    /// List of deleted conflicted revisions.
    /// Available if requested with <see cref="FindOptions.DeleteConflicts"/> set to <c>True</c>
    /// </summary>
    [JsonIgnore]
    public IReadOnlyCollection<string> DeletedConflicts { get; private set; }

    [JsonPropertyName("_deleted_conflicts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    private List<string> DeletedConflictsOther { set { DeletedConflicts = value.AsReadOnly(); } }

    /// <summary>
    /// Document’s update sequence in current database.
    /// Available if requested with <see cref="FindOptions.LocalSequence"/> set to <c>True</c>
    /// </summary>
    [JsonIgnore]
    public int LocalSequence { get; private set; }

    [JsonPropertyName("_localSeq")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    private int LocalSequenceOther { set { LocalSequence = value; } }

    /// <summary>
    /// List of objects with information about local revisions and their status.
    /// Available if requested with <see cref="FindOptions.OpenRevisions"/>
    /// </summary>
    [JsonIgnore]
    public IReadOnlyCollection<RevisionInfo> RevisionsInfo { get; private set; }

    [JsonPropertyName("_revs_info")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    private List<RevisionInfo> RevisionsInfoOther { set { RevisionsInfo = value.AsReadOnly(); } }

    /// <summary>
    /// List of local revision tokens without.
    /// Available if requested with <see cref="FindOptions.Revisions"/> set to <c>True</c>
    /// </summary>
    [JsonIgnore]
    public Revisions Revisions { get; private set; }

    [JsonPropertyName("_revisions")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    private Revisions RevisionsOther { set { Revisions = value; } }


    /// <summary>
    /// Attachment’s stubs. Available if document has any attachments
    /// </summary>
    [JsonIgnore]
    public CouchAttachmentsCollection Attachments { get; private set; }

    [JsonPropertyName("_attachments")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter(typeof(AttachmentsParsedConverter))]
    private Dictionary<string, CouchAttachment> AttachmentsParsed { get; set; }

    /// <summary>
    /// Used for database splitting
    /// </summary>
    [JsonPropertyName(CouchClient.DefaultDatabaseSplitDiscriminator)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    internal string? SplitDiscriminator { get; set; }

    [OnDeserialized]
    internal void OnDeserializedMethod(StreamingContext context)
    {
        if (AttachmentsParsed is { Count: > 0 })
        {
            Attachments = new CouchAttachmentsCollection(AttachmentsParsed);
        }
    }
}