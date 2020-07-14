using System;

namespace CouchDB.Driver.Query
{
    public class QueryContext
    {
        public Uri Endpoint { get; set; }
        public string DatabaseName { get; set; }
        public string EscapedDatabaseName { get; set; }

        public QueryContext(Uri endpoint, string databaseName)
        {
            Endpoint = endpoint;
            DatabaseName = databaseName;
            EscapedDatabaseName = Uri.EscapeDataString(databaseName);
        }
    }
}