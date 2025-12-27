using System.Collections.ObjectModel;

namespace CouchDB.Driver.Types;

/// <summary>
/// Represents a CouchDB document.
/// <param name="Conflicts">
/// The number of copies of every document.
/// Available if requested with <see creGetItemOptionsons.Conflicts"/> set to <c>True</c>
/// </param>
/// <param name="DeletedConflicts">
/// List of deleted conflicted revisions.
/// Available if requested with <see creGetItemOptionsons.DeleteConflicts"/> set to <c>True</c>
/// </param>
/// <param name="LocalSequence">
/// Document’s update sequence in current database.
/// Available if requested with <see creGetItemOptionsons.LocalSequence"/> set to <c>True</c>
/// </param>
/// <param name="RevisionsInfo">
/// List of objects with information about local revisions and their status.
/// Available if requested with <see creGetItemOptionsons.OpenRevisions"/>
/// </param>
/// <param name="Revisions">
/// List of local revision tokens without.
/// Available if requested with <see creGetItemOptionsons.Revisions"/> set to <c>True</c>
/// </param>
/// <param name="Revisions">
/// Marks whether the document has been deleted.
/// </param>
/// <param name="Attachments">
/// Attachment's stubs. Available if document has any attachments.
/// </param>
/// </summary>
[Serializable]
public record ReadItemResponse<TSource>(
    TSource Document,
    string Rev,
    string[]? Conflicts,
    string[]? DeletedConflicts,
    int? LocalSequence,
    RevisionInfo[]? RevisionsInfo,
    Revisions? Revisions,
    ReadOnlyDictionary<string, CouchAttachment>? Attachments,
    bool Deleted) where TSource : class
{
    public static implicit operator TSource(ReadItemResponse<TSource> response) => response.Document;
}