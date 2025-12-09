using System;

namespace CouchDB.Driver.Query;

public class QueryContext(Uri endpoint, string databaseName, bool throwOnQueryWarning)
{
    public Uri Endpoint { get; set; } = endpoint;
    public string DatabaseName { get; set; } = databaseName;
    public string EscapedDatabaseName { get; set; } = Uri.EscapeDataString(databaseName);

    public bool ThrowOnQueryWarning { get; set; } = throwOnQueryWarning;
}