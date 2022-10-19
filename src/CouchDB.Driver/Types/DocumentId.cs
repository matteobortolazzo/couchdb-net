using CouchDB.Driver.Types;

namespace CouchDB.Driver;

public class DocumentId
{
    public DocumentId(string id, string rev)
    {
        Id = id;
        Rev = rev;
    }

    public DocumentId(CouchDocument document)
    {
        Id = document.Id;
        Rev = document.Rev;
    }
        
    public string Id { get; }
    public string Rev { get; }
        
    public static explicit operator DocumentId(CouchDocument documentId)
    {
        return new DocumentId(documentId.Id,  documentId.Rev);
    }
}