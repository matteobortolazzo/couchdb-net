namespace CouchDB.Driver.Types;

public abstract class BulkItemOperation
{
    public static BulkItemOperation Add<TSource>(TSource document) where TSource: class
        => new AddItemOperation(document);

    public static BulkItemOperation Update<TSource>(TSource document, string id, string rev) where TSource: class
        => new UpdateItemOperation(id, rev, document);

    public static BulkItemOperation Delete(string id, string rev)
        => new DeleteItemOperation(id, rev);
}

internal sealed class AddItemOperation(object document) : BulkItemOperation
{
    public readonly object Document = document;
}

internal sealed class UpdateItemOperation(string id, string rev, object document) : BulkItemOperation
{
    public readonly string Id = id;
    public readonly string Rev = rev;
    public readonly object Document = document;
}

internal sealed class DeleteItemOperation(string id, string rev) : BulkItemOperation
{
    public readonly string Id = id;
    public readonly string Rev = rev;
}