using System;

namespace CouchDB.Driver.Types;

public class DocumentId(string id, string rev)
{
    public string Id { get; } = id;
    public string Rev { get; } = rev;

    public static explicit operator DocumentId(CouchDocument documentId)
    {
        ArgumentNullException.ThrowIfNull(documentId.Id);
        ArgumentNullException.ThrowIfNull(documentId.Rev);
        return new DocumentId(documentId.Id,  documentId.Rev);
    }
}