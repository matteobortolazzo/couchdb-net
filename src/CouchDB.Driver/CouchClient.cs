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
using CouchDB.Driver.DTOs;
using CouchDB.Driver.Exceptions;
using Newtonsoft.Json;
using System.Net;
using System.Threading;
using CouchDB.Driver.Options;
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "<Pending>")]
        private readonly IFlurlClient _flurlClient;
        private readonly CouchOptions _options;
        private readonly string[] _systemDatabases = { "_users", "_replicator", "_global_changes" };
        public Uri Endpoint { get; }

        /// <summary>
        /// Creates a new CouchDB client.
        /// </summary>
        /// <param name="endpoint">URI to the CouchDB endpoint.</param>
        /// <param name="couchSettingsFunc">A function to configure options.</param>
        public CouchClient(string endpoint, Action<CouchOptionsBuilder>? couchSettingsFunc = null)
            : this(new Uri(endpoint), couchSettingsFunc) { }

        /// <summary>
        /// Creates a new CouchDB client.
        /// </summary>
        /// <param name="endpoint">URI to the CouchDB endpoint.</param>
        /// <param name="couchSettingsFunc">A function to configure options.</param>
        public CouchClient(Uri endpoint, Action<CouchOptionsBuilder>? couchSettingsFunc = null)
        {
            var optionsBuilder = new CouchOptionsBuilder();
            couchSettingsFunc?.Invoke(optionsBuilder);
            _options = optionsBuilder.Options;
            Endpoint = endpoint;
            _flurlClient = GetConfiguredClient();
        }

        /// <summary>
        /// Creates a new CouchDB client.
        /// </summary>
        /// <param name="couchSettingsFunc">A function to configure options.</param>
        public CouchClient(Action<CouchOptionsBuilder>? couchSettingsFunc = null)
        {
            var optionsBuilder = new CouchOptionsBuilder();
            couchSettingsFunc?.Invoke(optionsBuilder);

            if (optionsBuilder.Options.Endpoint == null)
            {
                throw new InvalidOperationException("Database endpoint must be set.");
            }

            _options = optionsBuilder.Options;
            Endpoint = _options.Endpoint;
            _flurlClient = GetConfiguredClient();
        }

        internal CouchClient(CouchOptions options)
        {
            if (options.Endpoint == null)
            {
                throw new InvalidOperationException("Database endpoint must be set.");
            }

            _options = options;
            Endpoint = _options.Endpoint;
            _flurlClient = GetConfiguredClient();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
        private IFlurlClient GetConfiguredClient() =>
            new FlurlClient(Endpoint.AbsoluteUri).Configure(s =>
            {
                s.JsonSerializer = new NewtonsoftJsonSerializer(new JsonSerializerSettings
                {
                    ContractResolver = new CouchContractResolver(_options.PropertiesCase),
                    NullValueHandling = _options.NullValueHandling ?? NullValueHandling.Include
                });
                s.BeforeCallAsync = OnBeforeCallAsync;
                if (_options.ServerCertificateCustomValidationCallback != null)
                {
                    s.HttpClientFactory = new CertClientFactory(_options.ServerCertificateCustomValidationCallback);
                }

                _options.ClientFlurlHttpSettingsAction?.Invoke(s);
            });

        #region Operations

        #region CRUD

        /// <inheritdoc />
        public ICouchDatabase<TSource> GetDatabase<TSource>(string database, string? discriminator = null) where TSource : CouchDocument
        {
            CheckDatabaseName(database);
            var queryContext = new QueryContext(Endpoint, database);
            return new CouchDatabase<TSource>(_flurlClient, _options, queryContext, discriminator);
        }

        /// <inheritdoc />
        public async Task<ICouchDatabase<TSource>> CreateDatabaseAsync<TSource>(string database, 
            int? shards = null, int? replicas = null, bool? partitioned = null, string? discriminator = null, CancellationToken cancellationToken = default)
            where TSource : CouchDocument
        {
            QueryContext queryContext = NewQueryContext(database);
            IFlurlResponse response = await CreateDatabaseAsync(queryContext, shards, replicas, partitioned, cancellationToken)
                .ConfigureAwait(false);
            
            if (response.IsSuccessful())
            {
                return new CouchDatabase<TSource>(_flurlClient, _options, queryContext, discriminator);
            }

            if (response.StatusCode == (int)HttpStatusCode.PreconditionFailed)
            {
                throw new CouchException($"Database with name {database} already exists.");
            }

            throw new CouchException($"Something wrong happened while creating database {database}.");
        }

        /// <inheritdoc />
        public async Task<ICouchDatabase<TSource>> GetOrCreateDatabaseAsync<TSource>(string database,
            int? shards = null, int? replicas = null, bool? partitioned = null, string? discriminator = null, CancellationToken cancellationToken = default)
            where TSource : CouchDocument
        {
            QueryContext queryContext = NewQueryContext(database);
            IFlurlResponse response = await CreateDatabaseAsync(queryContext, shards, replicas, partitioned, cancellationToken)
                .ConfigureAwait(false);

            if (response.IsSuccessful() || response.StatusCode == (int)HttpStatusCode.PreconditionFailed)
            {
                return new CouchDatabase<TSource>(_flurlClient, _options, queryContext, discriminator);
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

        private Task<IFlurlResponse> CreateDatabaseAsync(QueryContext queryContext,
            int? shards = null, int? replicas = null, bool? partitioned = null, CancellationToken cancellationToken = default)
        {
            IFlurlRequest request = NewRequest()
                .AppendPathSegment(queryContext.EscapedDatabaseName);

            if (shards.HasValue && shards.Value != 8)
            {
                request = request.SetQueryParam("q", shards.Value);
            }

            if (replicas.HasValue && replicas.Value != 3)
            {
                request = request.SetQueryParam("n", replicas.Value);
            }

            if (partitioned.HasValue && partitioned.Value)
            {
                request = request.SetQueryParam("partitioned", "true");
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
        public Task<ICouchDatabase<TSource>> CreateDatabaseAsync<TSource>(int? shards = null, int? replicas = null, bool? partitioned = null, string? discriminator = null,
            CancellationToken cancellationToken = default) where TSource : CouchDocument
        {
            return CreateDatabaseAsync<TSource>(GetClassName<TSource>(), shards, replicas, partitioned, discriminator, cancellationToken);
        }

        /// <inheritdoc />
        public Task<ICouchDatabase<TSource>> GetOrCreateDatabaseAsync<TSource>(int? shards = null, int? replicas = null, bool? partitioned = null, string? discriminator = null,
            CancellationToken cancellationToken = default) where TSource : CouchDocument
        {
            return GetOrCreateDatabaseAsync<TSource>(GetClassName<TSource>(), shards, replicas, partitioned, discriminator, cancellationToken);
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
            return GetOrCreateDatabaseAsync<CouchUser>(null, null, null, null, cancellationToken);
        }

        /// <inheritdoc />
        public Task<ICouchDatabase<TUser>> GetOrCreateUsersDatabaseAsync<TUser>(CancellationToken cancellationToken = default) where TUser : CouchUser
        {
            return GetOrCreateDatabaseAsync<TUser>(null, null, null, null, cancellationToken);
        }

        #endregion

        #region Utils

        /// <inheritdoc />
        public async Task<bool> ExistsAsync(string database, CancellationToken cancellationToken = default)
        {
            QueryContext queryContext = NewQueryContext(database);
            IFlurlResponse? response = await NewRequest()
                .AllowHttpStatus(HttpStatusCode.NotFound)
                .AppendPathSegment(queryContext.EscapedDatabaseName)
                .HeadAsync(cancellationToken)
                .ConfigureAwait(false);
            return response.IsSuccessful();
        }

        /// <inheritdoc />
        public async Task<bool> IsUpAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                StatusResult result = await NewRequest()
                    .AllowAnyHttpStatus()
                    .AppendPathSegment("/_up")
                    .GetJsonAsync<StatusResult>(cancellationToken)
                    .SendRequestAsync()
                    .ConfigureAwait(false);
                return result?.Status == "ok";
            }
            catch (CouchException)
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

        #region Replication
        public async Task<bool> ReplicateAsync(string source, string target, CouchReplication? replication = null, bool persistent = true, CancellationToken cancellationToken = default)
        {
            var request = NewRequest();

            if (replication == null)
            {
                replication = new CouchReplication();
            }

            if (replication.SourceCredentials == null)
            {
                replication.Source = source;
            }
            else
            {
                replication.Source = new CouchReplicationHost()
                {
                    Url = source,
                    Auth = new CouchReplicationAuth()
                    {
                        BasicCredentials = replication.SourceCredentials,
                    }
                };
            }

            if (replication.TargetCredentials == null)
            {
                replication.Target = target;
            }
            else
            {
                replication.Target = new CouchReplicationHost()
                {
                    Url = target,
                    Auth = new CouchReplicationAuth()
                    {
                        BasicCredentials = replication.TargetCredentials,
                    }
                };
            }

            OperationResult result = await request
                .AppendPathSegments(persistent ? "_replicator" : "_replicate")
                .PostJsonAsync(replication, cancellationToken)
                .SendRequestAsync()
                .ReceiveJson<OperationResult>()
                .ConfigureAwait(false);

            return result.Ok;
        }

        public async Task<bool> RemoveReplicationAsync(string source, string target, CouchReplication? replication = null, bool persistent = true, CancellationToken cancellationToken = default)
        {
            var request = NewRequest();

            if (replication == null)
            {
                replication = new CouchReplication();
            }

            if (replication.SourceCredentials == null)
            {
                replication.Source = source;
            }
            else
            {
                replication.Source = new CouchReplicationHost()
                {
                    Url = source,
                    Auth = new CouchReplicationAuth()
                    {
                        BasicCredentials = replication.SourceCredentials,
                    }
                };
            }

            if (replication.TargetCredentials == null)
            {
                replication.Target = target;
            }
            else
            {
                replication.Target = new CouchReplicationHost()
                {
                    Url = target,
                    Auth = new CouchReplicationAuth()
                    {
                        BasicCredentials = replication.TargetCredentials,
                    }
                };
            }

            replication.Cancel = true;

            OperationResult result = await request
                .AppendPathSegments(persistent ? "_replicator" : "_replicate")
                .PostJsonAsync(replication, cancellationToken)
                .SendRequestAsync()
                .ReceiveJson<OperationResult>()
                .ConfigureAwait(false);

            return result.Ok;
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
            await DisposeAsync(true).ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }

        protected virtual async Task DisposeAsync(bool disposing)
        {
            if (disposing)
            {
                if (_options.AuthenticationType == AuthenticationType.Cookie && _options.LogOutOnDispose)
                {
                    await LogoutAsync().ConfigureAwait(false);
                }

                _flurlClient.Dispose();
            }
        }

        #endregion

        private string GetClassName<TSource>()
        {
            Type type = typeof(TSource);
            return GetClassName(type);
        }

        public string GetClassName(Type type)
        {
            Check.NotNull(type, nameof(type));
            return type.GetName(_options);
        }
    }
}
