

namespace CouchDB.Driver.Options;

public class CouchDatabaseBuilder
{
    internal readonly Dictionary<Type, CouchDocumentBuilder> DocumentBuilders;

    internal CouchDatabaseBuilder()
    {
        DocumentBuilders = new Dictionary<Type, CouchDocumentBuilder>();
    }

    public CouchDocumentBuilder<TSource> Document<TSource>()
        where TSource: class
    {
        Type documentType = typeof(TSource);
        if (!DocumentBuilders.TryGetValue(documentType, out CouchDocumentBuilder? value))
        {
            value = new CouchDocumentBuilder<TSource>();
            DocumentBuilders.Add(documentType, value);
        }

        return (CouchDocumentBuilder<TSource>)value;
    }
}