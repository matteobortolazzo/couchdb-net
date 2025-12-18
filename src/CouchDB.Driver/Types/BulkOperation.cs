namespace CouchDB.Driver.Types;

public abstract class BulkOperation
{
    public static BulkOperation Add<TSource>(TSource document) where TSource: class
        => new AddOperation(document);

    public static BulkOperation Update<TSource>(TSource document, string id, string rev) where TSource: class
        => new UpdateOperation(id, rev, document);

    public static BulkOperation Delete(string id, string rev)
        => new DeleteOperation(id, rev);
}

internal sealed class AddOperation(object document) : BulkOperation
{
    public object Document = document;
}

internal sealed class UpdateOperation(string id, string rev, object document) : BulkOperation
{
    public string Id = id;
    public string Rev = rev;
    public object Document = document;
}

internal sealed class DeleteOperation(string id, string rev) : BulkOperation
{
    public string Id = id;
    public string Rev = rev;
}