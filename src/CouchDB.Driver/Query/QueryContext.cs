namespace CouchDB.Driver.Query;

public class QueryContext(Uri endpoint, string databaseName, bool throwOnQueryWarning)
{
    public Uri Endpoint { get; } = endpoint;
    public string DatabaseName { get; } = databaseName;
    public string EscapedDatabaseName { get; } = Uri.EscapeDataString(databaseName);
    public bool ThrowOnQueryWarning { get; } = throwOnQueryWarning;
}