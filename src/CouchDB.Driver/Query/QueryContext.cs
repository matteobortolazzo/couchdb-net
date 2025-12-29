using CouchDB.Driver.Options;

namespace CouchDB.Driver.Query;

internal record QueryContext(Uri Endpoint, CouchInternalOptions Options, string DatabaseName)
{
    public string EscapedDatabaseName => Uri.EscapeDataString(DatabaseName);
}