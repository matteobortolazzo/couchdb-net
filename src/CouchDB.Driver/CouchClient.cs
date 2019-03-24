using CouchDB.Driver.Extensions;
using Flurl.Http;
using System;

namespace CouchDB.Driver
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

        public CouchDatabase<TSource> GetDatabase<TSource>() 
        {
            var type = typeof(TSource);
            var db = type.GetName();
            return GetDatabase<TSource>(db);
        }
        public CouchDatabase<TSource> GetDatabase<TSource>(string db) 
        {
            if (db == null)
                throw new ArgumentNullException(nameof(db));

            return new CouchDatabase<TSource>(flurlClient, ConnectionString, db);
        }

        public void Dispose()
        {
            flurlClient.Dispose();
        }
    }
}
