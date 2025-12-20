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
/// </summary>
public record FindResponse<TSource>(
    TSource Document,
    [property: JsonPropertyName("_rev")]
    string Rev,
    [property: JsonPropertyName("_conflicts")]
    string[]? Conflicts,
    [property: JsonPropertyName("_deleted_conflicts")]
    string[]? DeletedConflicts,
    [property: JsonPropertyName("_localSeq")]
    int? LocalSequence,
    [property: JsonPropertyName("_revs_info")]
    RevisionInfo[]? RevisionsInfo,
    [property: JsonPropertyName("_revisions")]
    Revisions? Revisions
) where TSource : class;