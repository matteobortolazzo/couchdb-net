namespace CouchDB.Driver.Types;

// TODO: Review if needed
public class DocumentId(string id, string rev)
{
    public DocumentId(CouchDocument document) : this(document.Id, document.Rev)
    {
    }
        
    public string Id { get; } = id;
    public string Rev { get; } = rev;

    public static explicit operator DocumentId(CouchDocument documentId)
    {
        return new DocumentId(documentId.Id,  documentId.Rev);
    }
}