using CouchDB.Driver.Extensions;
using CouchDB.Driver.Types;
using Flurl.Http;
using System;

namespace CouchDB.Driver
{
    public class CouchClient : IDisposable
    {
        private readonly CouchSettings settings;
        private readonly FlurlClient flurlClient;
        public string ConnectionString { get; private set; }

        public CouchClient(string connectionString, Action<CouchSettings> configFunc = null)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            ConnectionString = connectionString;
            flurlClient = new FlurlClient(connectionString);
            settings = new CouchSettings();
            configFunc?.Invoke(settings);
        }
        public CouchDatabase<TSource> GetDatabase<TSource>() where TSource : CouchEntity
        {
            var type = typeof(TSource);
            var db = type.GetName();
            return GetDatabase<TSource>(db);
        }
        public CouchDatabase<TSource> GetDatabase<TSource>(string db) where TSource : CouchEntity
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
