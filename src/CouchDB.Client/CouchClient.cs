using CouchDB.Client.Extensions;
using Flurl.Http;
using System;

namespace CouchDB.Client
{
    public class CouchClient : IDisposable
    {
        private FlurlClient flurlClient;
        public string ConnectionString { get; private set; }

        public CouchClient(string connectionString)
        {
            ConnectionString = connectionString;
            flurlClient = new FlurlClient(connectionString);            
        }

        public CouchDatabase<T> GetDatabase<T>()
        {
            var type = typeof(T);
            var db = type.GetName();
            return GetDatabase<T>(db);
        }
        public CouchDatabase<T> GetDatabase<T>(string db)
        {
            if (db == null)
                throw new ArgumentNullException(nameof(db));

            var queryProvider = new CouchQueryProvider(flurlClient, ConnectionString, db);
            return new CouchDatabase<T>(queryProvider, db);
        }

        public void Dispose()
        {
            flurlClient.Dispose();
        }
    }
}
