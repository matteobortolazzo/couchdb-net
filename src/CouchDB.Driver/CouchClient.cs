using CouchDB.Driver.Extensions;
using CouchDB.Driver.Helpers;
using CouchDB.Driver.Types;
using Flurl.Http;
using Flurl.Http.Configuration;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using CouchDB.Driver.Settings;
using CouchDB.Driver.DTOs;
using CouchDB.Driver.Exceptions;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using CouchDB.Driver.Query;

namespace CouchDB.Driver
{
    /// <summary>
    /// Client for querying a CouchDB database.
    /// </summary>
    public partial class CouchClient : ICouchClient
    {
        private DateTime? _cookieCreationDate;
        private string? _cookieToken;
        private readonly CouchSettings _settings;
        private readonly IFlurlClient _flurlClient;
        private readonly string[] _systemDatabases = { "_users", "_replicator", "_global_changes" };
        public Uri Endpoint { get; }

        /// <summary>
        /// Creates a new CouchDB client.
        /// </summary>
        /// <param name="databaseUri">URI to the CouchDB endpoint.</param>
        /// <param name="couchSettingsFunc">A function to configure the client settings.</param>
        /// <param name="flurlSettingsFunc">A function to configure the HTTP client.</param>
        public CouchClient(string databaseUri, Action<ICouchConfiguration>? couchSettingsFunc = null,
            Action<ClientFlurlHttpSettings>? flurlSettingsFunc = null): this(new Uri(databaseUri), couchSettingsFunc, flurlSettingsFunc) { }

        /// <summary>
        /// Creates a new CouchDB client.
        /// </summary>
        /// <param name="endpoint">URI to the CouchDB endpoint.</param>
        /// <param name="couchSettingsFunc">A function to configure the client settings.</param>
        /// <param name="flurlSettingsFunc">A function to configure the HTTP client.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
        public CouchClient(Uri endpoint, Action<ICouchConfiguration>? couchSettingsFunc = null, Action<ClientFlurlHttpSettings>? flurlSettingsFunc = null)
        {
            _settings = new CouchSettings();
            couchSettingsFunc?.Invoke(_settings);

            Endpoint = endpoint;
            _flurlClient = new FlurlClient(endpoint.AbsoluteUri).Configure(s =>
            {
                s.JsonSerializer = new NewtonsoftJsonSerializer(new JsonSerializerSettings
                {
                    ContractResolver = new CouchContractResolver(_settings.PropertiesCase)
                });
                s.BeforeCallAsync = OnBeforeCallAsync;
                if (_settings.ServerCertificateCustomValidationCallback != null)
                {
                    s.HttpClientFactory = new CertClientFactory(_settings.ServerCertificateCustomValidationCallback);
                }

                flurlSettingsFunc?.Invoke(s);
            });
        }

        #region Operations

        #region CRUD

        /// <inheritdoc />
        public ICouchDatabase<TSource> GetDatabase<TSource>(string database) where TSource : CouchDocument
        {
            var queryContext = new QueryContext(Endpoint, database);
            return new CouchDatabase<TSource>(_flurlClient, _settings, queryContext);
        }

        /// <inheritdoc />
        public Task<ICouchDatabase<TSource>> GetSafeDatabaseAsync<TSource>(string database, int? shards = null, int? replicas = null) where TSource : CouchDocument
        {
            return CreateDatabaseAsync<TSource>(database, shards, replicas);
        }

        /// <inheritdoc />
        public async Task<ICouchDatabase<TSource>> CreateDatabaseAsync<TSource>(string database, int? shards = null, int? replicas = null) where TSource : CouchDocument
        {
            var queryContext = new QueryContext(Endpoint, database);

            IFlurlRequest request = NewRequest()
                .AppendPathSegment(queryContext.EscapedDatabaseName);

            if (shards.HasValue)
            {
                request = request.SetQueryParam("q", shards.Value);
            }

            if (replicas.HasValue)
            {
                request = request.SetQueryParam("n", replicas.Value);
            }

            HttpResponseMessage response = await request
                .AllowHttpStatus(HttpStatusCode.PreconditionFailed)
                .PutAsync(null)
                .SendRequestAsync()
                .ConfigureAwait(false);

            

            // Database already exists
            if (response.StatusCode == HttpStatusCode.PreconditionFailed)
            {
                return new CouchDatabase<TSource>(_flurlClient, _settings, queryContext);
            }

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            OperationResult result = JsonConvert.DeserializeObject<OperationResult>(content);

            if (!result.Ok)
            {
                throw new CouchException("Something went wrong during the creation.");
            }

            return new CouchDatabase<TSource>(_flurlClient, _settings, queryContext);
        }

        /// <inheritdoc />
        public async Task DeleteDatabaseAsync<TSource>(string database) where TSource : CouchDocument
        {
            database = EscapeDatabaseName(database);

            OperationResult result = await NewRequest()
                .AppendPathSegment(database)
                .DeleteAsync()
                .ReceiveJson<OperationResult>()
                .SendRequestAsync()
                .ConfigureAwait(false);

            if (!result.Ok) 
            {
                throw new CouchException("Something went wrong during the delete.", null, "S");
            }
        }

        #endregion

        #region CRUD reflection

        /// <inheritdoc />
        public ICouchDatabase<TSource> GetDatabase<TSource>() where TSource : CouchDocument
        {
            return GetDatabase<TSource>(GetClassName<TSource>());
        }

        /// <inheritdoc />
        public Task<ICouchDatabase<TSource>> CreateDatabaseAsync<TSource>() where TSource : CouchDocument
        {
            return CreateDatabaseAsync<TSource>(GetClassName<TSource>());
        }

        /// <inheritdoc />
        public Task DeleteDatabaseAsync<TSource>() where TSource : CouchDocument
        {
            return DeleteDatabaseAsync<TSource>(GetClassName<TSource>());
        }

        #endregion

        #region Users

        /// <inheritdoc />
        public ICouchDatabase<CouchUser> GetUsersDatabase()
        {
            return GetDatabase<CouchUser>(GetClassName<CouchUser>());
        }

        /// <inheritdoc />
        public ICouchDatabase<TUser> GetUsersDatabase<TUser>() where TUser : CouchUser
        {
            return GetDatabase<TUser>(GetClassName<TUser>());
        }

        #endregion

        #region Utils

        /// <inheritdoc />
        public async Task<bool> ExistsAsync(string database)
        {
            HttpResponseMessage? response = await NewRequest()
                .AllowHttpStatus(HttpStatusCode.NotFound)
                .AppendPathSegment(database)
                .HeadAsync()
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        /// <inheritdoc />
        public async Task<bool> IsUpAsync()
        {
            try
            {
                StatusResult result = await NewRequest()
                    .AppendPathSegment("/_up")
                    .GetJsonAsync<StatusResult>()
                    .SendRequestAsync()
                    .ConfigureAwait(false);
                return result.Status == "ok";
            }
            catch (CouchNotFoundException)
            {
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<string>> GetDatabasesNamesAsync()
        {
            return await NewRequest()
                .AppendPathSegment("_all_dbs")
                .GetJsonAsync<IEnumerable<string>>()
                .SendRequestAsync()
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CouchActiveTask>> GetActiveTasksAsync()
        {
            return await NewRequest()
                .AppendPathSegment("_active_tasks")
                .GetJsonAsync<IEnumerable<CouchActiveTask>>()
                .SendRequestAsync()
                .ConfigureAwait(false);
        }

        #endregion

        #endregion

        #region Implementations

        private IFlurlRequest NewRequest()
        {
            return _flurlClient.Request(Endpoint);
        }

        private string EscapeDatabaseName(string database)
        {
            Check.NotNull(database, nameof(database));


            if (!_systemDatabases.Contains(database) && !new Regex(@"^[a-z][a-z0-9_$()+/-]*$").IsMatch(database))
            {
                throw new ArgumentException($"Name {database} contains invalid characters. Please visit: https://docs.couchdb.org/en/stable/api/database/common.html#put--db", nameof(database));
            }

            return Uri.EscapeDataString(database);
        }

        /// <summary>
        /// Performs the logout and disposes the HTTP client.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (_settings.AuthenticationType == AuthenticationType.Cookie && _settings.LogOutOnDispose)
            {
                await LogoutAsync().ConfigureAwait(false);
            }
            _flurlClient.Dispose();
        }

        #endregion

        private string GetClassName<TSource>()
        {
            Type type = typeof(TSource);
            return type.GetName(_settings);
        }
    }
}
