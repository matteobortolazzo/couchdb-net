using System;

namespace CouchDB.Driver.Query
{
    public class QueryContext
    {
        public Uri Endpoint { get; set; }
        public string DatabaseName { get; set; }
        public string EscapedDatabaseName { get; set; }

        public bool ThrowOnQueryWarning { get; set; }

        public QueryContext(Uri endpoint, string databaseName, bool throwOnQueryWarning)
        {
            Endpoint = endpoint;
            DatabaseName = databaseName;
            EscapedDatabaseName = Uri.EscapeDataString(databaseName);
            ThrowOnQueryWarning = throwOnQueryWarning;
        }
    }
}