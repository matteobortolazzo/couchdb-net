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
using System.Threading;
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
        /// <param name="endpoint">URI to the CouchDB endpoint.</param>
        /// <param name="couchSettingsFunc">A function to configure the client settings.</param>
        public CouchClient(string endpoint, Action<ICouchConfigurator>? couchSettingsFunc = null)
            : this(new Uri(endpoint), couchSettingsFunc) { }

        /// <summary>
        /// Creates a new CouchDB client.
        /// </summary>
        /// <param name="endpoint">URI to the CouchDB endpoint.</param>
        /// <param name="couchSettingsFunc">A function to configure the client settings.</param>
        public CouchClient(Uri endpoint, Action<ICouchConfigurator>? couchSettingsFunc = null)
        {
            Endpoint = endpoint;
            _settings = new CouchSettings();
            couchSettingsFunc?.Invoke(_settings);
            _flurlClient = GetConfiguredClient();
        }

        internal CouchClient(CouchSettings couchSettings)
        {
            Endpoint = couchSettings.Endpoint;
            _settings = couchSettings;
            _flurlClient = GetConfiguredClient();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
        private IFlurlClient GetConfiguredClient() =>
            new FlurlClient(Endpoint.AbsoluteUri).Configure(s =>
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

                _settings.FlurlSettingsAction?.Invoke(s);
            });

        #region Operations

        #region CRUD

        /// <inheritdoc />
        public ICouchDatabase<TSource> GetDatabase<TSource>(string database) where TSource : CouchDocument
        {
            CheckDatabaseName(database);
            var queryContext = new QueryContext(Endpoint, database);
            return new CouchDatabase<TSource>(_flurlClient, _settings, queryContext);
        }

        /// <inheritdoc />
        public async Task<ICouchDatabase<TSource>> CreateDatabaseAsync<TSource>(string database, 
            int? shards = null, int? replicas = null, CancellationToken cancellationToken = default)
            where TSource : CouchDocument
        {
            QueryContext queryContext = NewQueryContext(database);
            HttpResponseMessage response = await CreateDatabaseAsync(queryContext, shards, replicas, cancellationToken)
                .ConfigureAwait(false);
            
            if (response.IsSuccessStatusCode)
            {
                return new CouchDatabase<TSource>(_flurlClient, _settings, queryContext);
            }

            if (response.StatusCode == HttpStatusCode.PreconditionFailed)
            {
                throw new CouchException($"Database with name {database} already exists.");
            }

            throw new CouchException($"Something wrong happened while creating database {database}.");
        }

        /// <inheritdoc />
        public async Task<ICouchDatabase<TSource>> GetOrCreateDatabaseAsync<TSource>(string database,
            int? shards = null, int? replicas = null, CancellationToken cancellationToken = default)
            where TSource : CouchDocument
        {
            QueryContext queryContext = NewQueryContext(database);
            HttpResponseMessage response = await CreateDatabaseAsync(queryContext, shards, replicas, cancellationToken)
                .ConfigureAwait(false);

            if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.PreconditionFailed)
            {
                return new CouchDatabase<TSource>(_flurlClient, _settings, queryContext);
            }

            throw new CouchException($"Something wrong happened while creating database {database}.");
        }

        /// <inheritdoc />
        public async Task DeleteDatabaseAsync(string database, CancellationToken cancellationToken = default)
        {
            QueryContext queryContext = NewQueryContext(database);

            OperationResult result = await NewRequest()
                .AppendPathSegment(queryContext.EscapedDatabaseName)
                .DeleteAsync(cancellationToken)
                .ReceiveJson<OperationResult>()
                .SendRequestAsync()
                .ConfigureAwait(false);

            if (!result.Ok) 
            {
                throw new CouchException("Something went wrong during the delete.", null, "S");
            }
        }

        private Task<HttpResponseMessage> CreateDatabaseAsync(QueryContext queryContext,
            int? shards = null, int? replicas = null, CancellationToken cancellationToken = default)
        {
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

            return request
                .AllowHttpStatus(HttpStatusCode.PreconditionFailed)
                .PutAsync(null, cancellationToken)
                .SendRequestAsync();
        }

        #endregion

        #region CRUD reflection

        /// <inheritdoc />
        public ICouchDatabase<TSource> GetDatabase<TSource>() where TSource : CouchDocument
        {
            return GetDatabase<TSource>(GetClassName<TSource>());
        }

        /// <inheritdoc />
        public Task<ICouchDatabase<TSource>> CreateDatabaseAsync<TSource>(int? shards = null, int? replicas = null,
            CancellationToken cancellationToken = default) where TSource : CouchDocument
        {
            return CreateDatabaseAsync<TSource>(GetClassName<TSource>(), shards, replicas, cancellationToken);
        }

        /// <inheritdoc />
        public Task<ICouchDatabase<TSource>> GetOrCreateDatabaseAsync<TSource>(int? shards = null, int? replicas = null,
            CancellationToken cancellationToken = default) where TSource : CouchDocument
        {
            return GetOrCreateDatabaseAsync<TSource>(GetClassName<TSource>(), shards, replicas, cancellationToken);
        }

        /// <inheritdoc />
        public Task DeleteDatabaseAsync<TSource>(CancellationToken cancellationToken = default) where TSource : CouchDocument
        {
            return DeleteDatabaseAsync(GetClassName<TSource>(), cancellationToken);
        }

        #endregion

        #region Users

        /// <inheritdoc />
        public ICouchDatabase<CouchUser> GetUsersDatabase()
        {
            return GetDatabase<CouchUser>();
        }

        /// <inheritdoc />
        public ICouchDatabase<TUser> GetUsersDatabase<TUser>() where TUser : CouchUser
        {
            return GetDatabase<TUser>(GetClassName<TUser>());
        }

        /// <inheritdoc />
        public Task<ICouchDatabase<CouchUser>> GetOrCreateUsersDatabaseAsync(CancellationToken cancellationToken = default)
        {
            return GetOrCreateDatabaseAsync<CouchUser>(null, null, cancellationToken);
        }

        /// <inheritdoc />
        public Task<ICouchDatabase<TUser>> GetOrCreateUsersDatabaseAsync<TUser>(CancellationToken cancellationToken = default) where TUser : CouchUser
        {
            return GetOrCreateDatabaseAsync<TUser>(null, null, cancellationToken);
        }

        #endregion

        #region Utils

        /// <inheritdoc />
        public async Task<bool> ExistsAsync(string database, CancellationToken cancellationToken = default)
        {
            QueryContext queryContext = NewQueryContext(database);
            HttpResponseMessage? response = await NewRequest()
                .AllowHttpStatus(HttpStatusCode.NotFound)
                .AppendPathSegment(queryContext.EscapedDatabaseName)
                .HeadAsync(cancellationToken)
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        /// <inheritdoc />
        public async Task<bool> IsUpAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                StatusResult result = await NewRequest()
                    .AppendPathSegment("/_up")
                    .GetJsonAsync<StatusResult>(cancellationToken)
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
        public async Task<IEnumerable<string>> GetDatabasesNamesAsync(CancellationToken cancellationToken = default)
        {
            return await NewRequest()
                .AppendPathSegment("_all_dbs")
                .GetJsonAsync<IEnumerable<string>>(cancellationToken)
                .SendRequestAsync()
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CouchActiveTask>> GetActiveTasksAsync(CancellationToken cancellationToken = default)
        {
            return await NewRequest()
                .AppendPathSegment("_active_tasks")
                .GetJsonAsync<IEnumerable<CouchActiveTask>>(cancellationToken)
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

        private QueryContext NewQueryContext(string database)
        {
            CheckDatabaseName(database);
            return new QueryContext(Endpoint, database);
        }

        private void CheckDatabaseName(string database)
        {
            Check.NotNull(database, nameof(database));

            if (!_systemDatabases.Contains(database) && !new Regex(@"^[a-z][a-z0-9_$()+/-]*$").IsMatch(database))
            {
                throw new ArgumentException($"Name {database} contains invalid characters. Please visit: https://docs.couchdb.org/en/stable/api/database/common.html#put--db", nameof(database));
            }
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
