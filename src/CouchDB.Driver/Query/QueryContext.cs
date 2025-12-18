namespace CouchDB.Driver.Query;

public class QueryContext(Uri endpoint, string databaseName, bool throwOnQueryWarning)
{
    public Uri Endpoint { get; init; } = endpoint;
    public string DatabaseName { get; init; } = databaseName;
    public string EscapedDatabaseName { get; init; } = Uri.EscapeDataString(databaseName);

    public bool ThrowOnQueryWarning { get; init; } = throwOnQueryWarning;
}