namespace CouchDB.Driver.Types;

/// <summary>
/// Represents a CouchDB document.
/// <param name="Conflicts">
/// The number of copies of every document.
/// Available if requested with <see cref="FindDocumentRequestOptions.Conflicts"/> set to <c>True</c>
/// </param>
/// <param name="DeletedConflicts">
/// List of deleted conflicted revisions.
/// Available if requested with <see cref="FindDocumentRequestOptions.DeleteConflicts"/> set to <c>True</c>
/// </param>
/// <param name="LocalSequence">
/// Document’s update sequence in current database.
/// Available if requested with <see cref="FindDocumentRequestOptions.LocalSequence"/> set to <c>True</c>
/// </param>
/// <param name="RevisionsInfo">
/// List of objects with information about local revisions and their status.
/// Available if requested with <see cref="FindDocumentRequestOptions.OpenRevisions"/>
/// </param>
/// <param name="Revisions">
/// List of local revision tokens without.
/// Available if requested with <see cref="FindDocumentRequestOptions.Revisions"/> set to <c>True</c>
/// </param>
/// <param name="Revisions">
/// Marks whether the document has been deleted.
/// </param>
/// </summary>
public record FindResponse<TSource>(
    TSource Document,
    string Rev,
    string[]? Conflicts,
    string[]? DeletedConflicts,
    int? LocalSequence,
    RevisionInfo[]? RevisionsInfo,
    Revisions? Revisions,
    bool Deleted) where TSource : class
{
    public static implicit operator TSource(FindResponse<TSource> response) => response.Document;
}