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
    /// Client for querying a CouchDB database.
    /// </summary>
    public partial class CouchClient : IDisposable
    {
        private DateTime? _cookieCreationDate;
        private string _cookieToken;
        private readonly CouchSettings _settings;
        private readonly FlurlClient _flurlClient;
        public string ConnectionString { get; private set; }

        /// <summary>
        /// Creates a new CouchDB client.
        /// </summary>
        /// <param name="connectionString">URI to the CouchDB endpoint.</param>
        /// <param name="couchSettingsFunc"></param>
        /// <param name="flurlSettingsFunc"></param>
        public CouchClient(string connectionString, Action<CouchSettings> couchSettingsFunc = null, Action<ClientFlurlHttpSettings> flurlSettingsFunc = null)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            _settings = new CouchSettings();
            couchSettingsFunc?.Invoke(_settings);

            ConnectionString = connectionString;
            _flurlClient = new FlurlClient(connectionString);

            _flurlClient.Configure(s =>
            {
                s.BeforeCall = OnBeforeCall;
                if (_settings.ServerCertificateCustomValidationCallback != null)
                {
                    s.HttpClientFactory = new CertClientFactory(_settings.ServerCertificateCustomValidationCallback);
                }

                flurlSettingsFunc?.Invoke(s);
            });
        }

        #region Operations

        #region CRUD

        /// <summary>
        /// Return an instance of a CouchDB database with the given name. If EnsureDatabaseExists is configured, it creates the database if it doesn't exists.
        /// </summary>
        /// <typeparam name="TSource">Type of the objects in the database.</typeparam>
        /// <param name="database">Database name</param>
        /// <returns></returns>
        public CouchDatabase<TSource> GetDatabase<TSource>(string database) where TSource : CouchEntity
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            if (_settings.CheckDatabaseExists)
            {
                var dbs = AsyncContext.Run(() => GetDatabasesNamesAsync());
                if (!dbs.Contains(database))
                {
                    return AsyncContext.Run(() => CreateDatabaseAsync<TSource>(database));
                }
            }

            return new CouchDatabase<TSource>(_flurlClient, _settings, ConnectionString, database);
        }
        /// <summary>
        /// Create a new database in the server with the given name. 
        /// The name must begin with a lowercase letter and can contains only lowercase characters, digits or _, $, (, ), +, - and /.s
        /// </summary>
        /// <typeparam name="TSource">Type of the objects in the database.</typeparam>
        /// <param name="database">Database name</param>
        /// <returns></returns>
        public async Task<CouchDatabase<TSource>> CreateDatabaseAsync<TSource>(string database) where TSource : CouchEntity
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            if (!new Regex(@"^[a-z][a-z0-9_$()+/-]*$").IsMatch(database))
            {
                throw new ArgumentException(nameof(database), $"Name {database} contains invalid characters. Please visit: https://docs.couchdb.org/en/stable/api/database/common.html#put--db");
            }

            await NewRequest()
                .AppendPathSegment(database)
                .PutAsync(null)
                .SendRequestAsync();

            return new CouchDatabase<TSource>(_flurlClient, _settings, ConnectionString, database);
        }
        /// <summary>
        /// Delete the database with the given name in the server.
        /// </summary>
        /// <typeparam name="TSource">Type of the objects in the database.</typeparam>
        /// <param name="database">Database name</param>
        /// <returns></returns>
        public async Task DeleteDatabaseAsync<TSource>(string database) where TSource : CouchEntity
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            await NewRequest()
                .AppendPathSegment(database)
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
