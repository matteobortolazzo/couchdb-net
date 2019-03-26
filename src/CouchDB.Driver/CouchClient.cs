using CouchDB.Driver.Extensions;
using CouchDB.Driver.Helpers;
using CouchDB.Driver.Types;
using Flurl.Http;
using Flurl.Http.Configuration;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;

namespace CouchDB.Driver
{
    /// <summary>
    /// 
    /// </summary>
    public partial class CouchClient : IDisposable
    {
        private DateTime? _cookieCreationDate;
        private string _cookieToken;
        private readonly CouchSettings _settings;
        private readonly FlurlClient _flurlClient;
        public string ConnectionString { get; private set; }

        public CouchClient(string connectionString, Action<CouchSettings> configFunc = null, Action<ClientFlurlHttpSettings> flurlConfigFunc = null)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            _settings = new CouchSettings();
            configFunc?.Invoke(_settings);

            ConnectionString = connectionString;
            _flurlClient = new FlurlClient(connectionString);

            _flurlClient.Configure(s =>
            {
                s.BeforeCall = OnBeforeCall;
                if (_settings.ServerCertificateCustomValidationCallback != null)
                {
                    s.HttpClientFactory = new CertClientFactory(_settings.ServerCertificateCustomValidationCallback);
                }

                flurlConfigFunc?.Invoke(s);
            });
        }

        #region Operations

        #region CRUD

        public CouchDatabase<TSource> GetDatabase<TSource>(string db) where TSource : CouchEntity
        {
            if (db == null)
                throw new ArgumentNullException(nameof(db));

            if (_settings.CheckDatabaseExists)
            {
                var dbs = AsyncContext.Run(() => GetDatabasesNamesAsync());
                if (!dbs.Contains(db))
                {
                    return AsyncContext.Run(() => CreateDatabaseAsync<TSource>(db));
                }
            }

            return new CouchDatabase<TSource>(_flurlClient, _settings, ConnectionString, db);
        }
        public async Task<CouchDatabase<TSource>> CreateDatabaseAsync<TSource>(string db) where TSource : CouchEntity
        {
            if (db == null)
                throw new ArgumentNullException(nameof(db));

            if (!new Regex(@"^[a-z][a-z0-9_$()+/-]*$").IsMatch(db))
            {
                throw new ArgumentException(nameof(db), $"Name {db} contains invalid characters. Please visit: https://docs.couchdb.org/en/stable/api/database/common.html#put--db");
            }

            await NewRequest()
                .AppendPathSegment(db)
                .PutAsync(null)
                .SendRequestAsync();

            return new CouchDatabase<TSource>(_flurlClient, _settings, ConnectionString, db);
        }
        public async Task DeleteDatabaseAsync<TSource>(string db) where TSource : CouchEntity
        {
            if (db == null)
                throw new ArgumentNullException(nameof(db));

            await NewRequest()
                .AppendPathSegment(db)
                .DeleteAsync()
                .SendRequestAsync();
        }

        #endregion

        #region CRUD reflection

        public CouchDatabase<TSource> GetDatabase<TSource>() where TSource : CouchEntity
        {            
            return GetDatabase<TSource>(GetClassName<TSource>());
        }
        public Task<CouchDatabase<TSource>> CreateDatabaseAsync<TSource>() where TSource : CouchEntity
        {
            return CreateDatabaseAsync<TSource>(GetClassName<TSource>());
        }
        public Task DeleteDatabaseAsync<TSource>() where TSource : CouchEntity
        {
            return DeleteDatabaseAsync<TSource>(GetClassName<TSource>());
        }
        private string GetClassName<TSource>()
        {
            var type = typeof(TSource);
            return type.GetName(_settings);
        }

        #endregion

        #region Utils

        public async Task<IEnumerable<string>> GetDatabasesNamesAsync()
        {
            return await NewRequest()
                .AppendPathSegment("_all_dbs")
                .GetJsonAsync<IEnumerable<string>>()
                .SendRequestAsync();
        }
        public async Task<IEnumerable<CouchActiveTask>> GetActiveTasksAsync()
        {
            return await NewRequest()
                .AppendPathSegment("_active_tasks")
                .GetJsonAsync<IEnumerable<CouchActiveTask>>()
                .SendRequestAsync();
        }

        #endregion

        #endregion

        #region Implementations
        private IFlurlRequest NewRequest()
        {
            return _flurlClient.Request(ConnectionString);
        }
        public void Dispose()
        {
            AsyncContext.Run(() => LogoutAsync());
            _flurlClient.Dispose();
        }

        #endregion
    }
}
