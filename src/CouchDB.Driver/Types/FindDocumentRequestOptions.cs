namespace CouchDB.Driver.Types;

/// <summary>
/// Options relevant to getting a document (supported by GET HTTP-method).
/// Check https://docs.couchdb.org/en/stable/api/document/common.html#get--db-docid
/// </summary>
public class FindDocumentRequestOptions
{
    /// <summary>
    /// Includes attachments bodies in response. Default is <c>False</c>
    /// </summary>
    public bool Attachments { get; init; }
        
    /// <summary>
    /// Includes encoding information in attachment stubs if the particular attachment is compressed. Default is <c>False</c>
    /// </summary>
    public bool AttachmentsEncodingInfo { get; init; }
        
    /// <summary>
    /// Includes attachments only since specified revisions. Doesn't include attachments for specified revisions. Optional
    /// </summary>
    public IList<string>? AttachmentsSince { get; init; }
        
    /// <summary>
    /// Includes information about conflicts in document. Default is <c>False</c>
    /// </summary>
    public bool Conflicts { get; init; }

    /// <summary>
    /// Includes information about deleted conflicted revisions. Default is <c>False</c>
    /// </summary>
    public bool DeleteConflicts { get; init; }
        
    /// <summary>
    /// Forces retrieving latest “leaf” revision, no matter what rev was requested. Default is <c>False</c>
    /// </summary>
    public bool Latest { get; init; }
        
    /// <summary>
    /// Includes last update sequence for the document. Default is <c>False</c>
    /// </summary>
    public bool LocalSequence { get; init; }

    /// <summary>
    /// Acts same as specifying all <see cref="Conflicts"/>, <see cref="DeleteConflicts"/> and <see cref="RevisionsInfo"/> query parameters. Default is <c>False</c>
    /// </summary>
    public bool Meta { get; init; }
        
    /// <summary>
    /// Retrieves documents of specified leaf revisions. Additionally, it accepts value as all to return all leaf revisions. Optional
    /// </summary>
    public IList<string>? OpenRevisions { get; init; }
        
    /// <summary>
    /// Retrieves document of specified revision. Optional
    /// </summary>
    public string? Revision { get; init; }
        
    /// <summary>
    /// Includes list of all known document revisions. Default is <c>False</c>
    /// </summary>
    public bool Revisions { get; init; }
        
    /// <summary>
    /// Includes detailed information for all known document revisions. Default is <c>False</c>
    /// </summary>
    public bool RevisionsInfo { get; init; }
}