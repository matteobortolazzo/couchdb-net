namespace CouchDB.Driver.Types;

/// <summary>
/// Represents the response received after a document request.
/// </summary>
/// <param name="Id">Document ID</param>
/// <param name="Rev">Document revision</param>
[Serializable]
public sealed record DocumentRequestResponse(
    string Id,
    string Rev
);

/// <summary>
/// Represents the response received after a bulk document request.
/// </summary>
/// <param name="Id">Document ID</param>
/// <param name="Rev">Document revision on success</param>
/// <param name="Error">Error type</param>
/// <param name="Reason">Error reason</param>
[Serializable]
public sealed record DocumentBulkRequestResponse(
    bool Ok,
    string Id,
    string? Rev,
    string? Error,
    string? Reason
);
