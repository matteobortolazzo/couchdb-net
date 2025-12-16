using CouchDB.Driver.Extensions;
using CouchDB.Driver.Helpers;
using CouchDB.Driver.Types;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CouchDB.Driver.DTOs;
using CouchDB.Driver.Exceptions;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using CouchDB.Driver.DelegatingHandlers;
using CouchDB.Driver.Options;
using CouchDB.Driver.Query;
using Flurl.Http.Configuration;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace CouchDB.Driver;

/// <summary>
/// Client for querying a CouchDB database.
/// </summary>
public partial class CouchClient : ICouchClient
{
    public const string DefaultDatabaseSplitDiscriminator = "split_discriminator";

    private readonly IFlurlClient _flurlClient;
    private readonly AuthenticationDelegatingHandler _authenticationHandler;
    private readonly CouchOptions _options;
    private readonly string[] _systemDatabases = ["_users", "_replicator", "_global_changes"];
    public Uri Endpoint { get; }

    /// <summary>
    /// Creates a new CouchDB client.
    /// </summary>
    /// <param name="endpoint">URI to the CouchDB endpoint.</param>
    /// <param name="couchSettingsFunc">A function to configure options.</param>
    public CouchClient(string endpoint, Action<CouchOptionsBuilder>? couchSettingsFunc = null)
        : this(new Uri(endpoint), couchSettingsFunc)
    {
    }

    /// <summary>
    /// Creates a new CouchDB client.
    /// </summary>
    /// <param name="endpoint">URI to the CouchDB endpoint.</param>
    /// <param name="couchSettingsFunc">A function to configure options.</param>
    public CouchClient(Uri endpoint, Action<CouchOptionsBuilder>? couchSettingsFunc = null)
        : this(BuildOptions(couchSettingsFunc, endpoint))
    {
    }

    /// <summary>
    /// Creates a new CouchDB client.
    /// </summary>
    /// <param name="couchSettingsFunc">A function to configure options.</param>
    public CouchClient(Action<CouchOptionsBuilder>? couchSettingsFunc = null)
        : this(BuildOptions(couchSettingsFunc))
    {
    }

    internal CouchClient(CouchOptions options)
    {
        if (options.Endpoint == null)
        {
            throw new InvalidOperationException("Database endpoint must be set.");
        }

        _options = options;
        Endpoint = _options.Endpoint;

        ResiliencePipeline<HttpResponseMessage> retryPipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new HttpRetryStrategyOptions { BackoffType = DelayBackoffType.Exponential, MaxRetryAttempts = 3 })
            .Build();

        var socketHandler = new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(15) };
        var resilienceHandler = new ResilienceHandler(retryPipeline) { InnerHandler = socketHandler };
        _authenticationHandler = new AuthenticationDelegatingHandler(_options.Authentication)
        {
            InnerHandler = resilienceHandler
        };

        var httpClient = new HttpClient(_authenticationHandler);
        httpClient.BaseAddress = options.Endpoint;

        _flurlClient = new FlurlClient(httpClient)
            .WithSettings(s =>
            {
                _options.JsonSerializerOptions ??= new JsonSerializerOptions();
                _options.JsonSerializerOptions.PropertyNamingPolicy ??= JsonNamingPolicy.CamelCase;

                // TODO: Check type resolver
                _options.JsonSerializerOptions.TypeInfoResolver =
                    new CouchJsonTypeInfoResolver(_options.DatabaseSplitDiscriminator);

                _options.JsonSerializerOptions.Converters.Add(new CouchDocumentConverter());
                s.JsonSerializer = new DefaultJsonSerializer(_options.JsonSerializerOptions);
            });
    }

    private static CouchOptions BuildOptions(Action<CouchOptionsBuilder>? couchSettingsFunc, Uri? endpoint = null)
    {
        var optionsBuilder = new CouchOptionsBuilder();
        couchSettingsFunc?.Invoke(optionsBuilder);

        if (endpoint != null)
        {
            optionsBuilder.Options.Endpoint = endpoint;
        }

        return optionsBuilder.Options.Endpoint == null
            ? throw new InvalidOperationException("Database endpoint must be set.")
            : optionsBuilder.Options;
    }

    #region Operations

    #region CRUD

    /// <inheritdoc />
    public ICouchDatabase<TSource> GetDatabase<TSource>(string database, string? discriminator = null)
        where TSource : CouchDocument
    {
        CheckDatabaseName(database);
        var queryContext = new QueryContext(Endpoint, database, _options.ThrowOnQueryWarning);
        return new CouchDatabase<TSource>(_flurlClient, _options, queryContext, discriminator);
    }

    /// <inheritdoc />
    public async Task<ICouchDatabase<TSource>> CreateDatabaseAsync<TSource>(string database,
        int? shards = null, int? replicas = null, bool? partitioned = null, string? discriminator = null,
        CancellationToken cancellationToken = default)
        where TSource : CouchDocument
    {
        QueryContext queryContext = NewQueryContext(database);
        IFlurlResponse response =
            await CreateDatabaseAsync(queryContext, shards, replicas, partitioned, cancellationToken)
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
        int? shards = null, int? replicas = null, bool? partitioned = null, string? discriminator = null,
        CancellationToken cancellationToken = default)
        where TSource : CouchDocument
    {
        QueryContext queryContext = NewQueryContext(database);
        IFlurlResponse response =
            await CreateDatabaseAsync(queryContext, shards, replicas, partitioned, cancellationToken)
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
            .DeleteAsync(cancellationToken: cancellationToken)
            .ReceiveJson<OperationResult>()
            .SendRequestAsync()
            .ConfigureAwait(false);

        if (!result.Ok)
        {
            throw new CouchException("Something went wrong during the delete.", null, "S");
        }
    }

    private Task<IFlurlResponse> CreateDatabaseAsync(QueryContext queryContext,
        int? shards = null, int? replicas = null, bool? partitioned = null,
        CancellationToken cancellationToken = default)
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
            .AllowHttpStatus((int)HttpStatusCode.PreconditionFailed)
            .PutAsync(cancellationToken: cancellationToken)
            .SendRequestAsync();
    }

    #endregion

    #region CRUD reflection

    /// <inheritdoc />
    public ICouchDatabase<TSource> GetDatabase<TSource>() where TSource : CouchDocument
    {
        return GetDatabase<TSource>(TypeExtensions.GetDatabaseName<TSource>());
    }

    /// <inheritdoc />
    public Task<ICouchDatabase<TSource>> CreateDatabaseAsync<TSource>(int? shards = null, int? replicas = null,
        bool? partitioned = null, string? discriminator = null,
        CancellationToken cancellationToken = default) where TSource : CouchDocument
    {
        return CreateDatabaseAsync<TSource>(TypeExtensions.GetDatabaseName<TSource>(), shards, replicas, partitioned,
            discriminator,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<ICouchDatabase<TSource>> GetOrCreateDatabaseAsync<TSource>(int? shards = null, int? replicas = null,
        bool? partitioned = null, string? discriminator = null,
        CancellationToken cancellationToken = default) where TSource : CouchDocument
    {
        return GetOrCreateDatabaseAsync<TSource>(TypeExtensions.GetDatabaseName<TSource>(), shards, replicas,
            partitioned, discriminator,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task DeleteDatabaseAsync<TSource>(CancellationToken cancellationToken = default)
        where TSource : CouchDocument
    {
        return DeleteDatabaseAsync(TypeExtensions.GetDatabaseName<TSource>(), cancellationToken);
    }

    #endregion

    #region Users

    private const string UsersDatabaseName = "_users";

    /// <inheritdoc />
    public ICouchDatabase<CouchUser> GetUsersDatabase()
    {
        return GetDatabase<CouchUser>(UsersDatabaseName);
    }

    /// <inheritdoc />
    public ICouchDatabase<TUser> GetUsersDatabase<TUser>() where TUser : CouchUser
    {
        return GetDatabase<TUser>(UsersDatabaseName);
    }

    /// <inheritdoc />
    public Task<ICouchDatabase<CouchUser>> GetOrCreateUsersDatabaseAsync(CancellationToken cancellationToken = default)
    {
        return GetOrCreateDatabaseAsync<CouchUser>(UsersDatabaseName, null, null, null, null, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ICouchDatabase<TUser>> GetOrCreateUsersDatabaseAsync<TUser>(
        CancellationToken cancellationToken = default) where TUser : CouchUser
    {
        return GetOrCreateDatabaseAsync<TUser>(UsersDatabaseName, null, null, null, null, cancellationToken);
    }

    #endregion

    #region Utils

    /// <inheritdoc />
    public Task<bool> ExistsAsync<TSource>(CancellationToken cancellationToken = default) =>
        ExistsAsync(TypeExtensions.GetDatabaseName<TSource>(), cancellationToken);

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string database, CancellationToken cancellationToken = default)
    {
        QueryContext queryContext = NewQueryContext(database);
        IFlurlResponse? response = await NewRequest()
            .AllowHttpStatus((int)HttpStatusCode.NotFound)
            .AppendPathSegment(queryContext.EscapedDatabaseName)
            .HeadAsync(cancellationToken: cancellationToken)
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
                .GetJsonAsync<StatusResult>(cancellationToken: cancellationToken)
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
    public async Task<IReadOnlyCollection<string>> GetDatabasesNamesAsync(CancellationToken cancellationToken = default)
    {
        return await NewRequest()
            .AppendPathSegment("_all_dbs")
            .GetJsonAsync<string[]>(cancellationToken: cancellationToken)
            .SendRequestAsync()
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<CouchActiveTask>> GetActiveTasksAsync(
        CancellationToken cancellationToken = default)
    {
        return await NewRequest()
            .AppendPathSegment("_active_tasks")
            .GetJsonAsync<CouchActiveTask[]>(cancellationToken: cancellationToken)
            .SendRequestAsync()
            .ConfigureAwait(false);
    }

    #endregion

    #region Replication

    public async Task<bool> ReplicateAsync(string source, string target, CouchReplication? replication = null,
        bool persistent = true, CancellationToken cancellationToken = default)
    {
        IFlurlRequest request = NewRequest();

        replication ??= new CouchReplication();

        if (replication.SourceCredentials == null)
        {
            replication.Source = source;
        }
        else
        {
            replication.Source = new CouchReplicationHost()
            {
                Url = source,
                Auth = new CouchReplicationAuth() { BasicCredentials = replication.SourceCredentials, }
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
                Auth = new CouchReplicationAuth() { BasicCredentials = replication.TargetCredentials, }
            };
        }

        OperationResult result = await request
            .AppendPathSegments(persistent ? "_replicator" : "_replicate")
            .PostJsonAsync(replication, cancellationToken: cancellationToken)
            .SendRequestAsync()
            .ReceiveJson<OperationResult>()
            .ConfigureAwait(false);

        return result.Ok;
    }

    public async Task<bool> RemoveReplicationAsync(string source, string target, CouchReplication? replication = null,
        bool persistent = true, CancellationToken cancellationToken = default)
    {
        IFlurlRequest request = NewRequest();

        replication ??= new CouchReplication();

        if (replication.SourceCredentials == null)
        {
            replication.Source = source;
        }
        else
        {
            replication.Source = new CouchReplicationHost()
            {
                Url = source,
                Auth = new CouchReplicationAuth() { BasicCredentials = replication.SourceCredentials, }
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
                Auth = new CouchReplicationAuth() { BasicCredentials = replication.TargetCredentials, }
            };
        }

        replication.Cancel = true;

        OperationResult result = await request
            .AppendPathSegments(persistent ? "_replicator" : "_replicate")
            .PostJsonAsync(replication, cancellationToken: cancellationToken)
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
        return new QueryContext(Endpoint, database, _options.ThrowOnQueryWarning);
    }

    private void CheckDatabaseName(string database)
    {
        ArgumentNullException.ThrowIfNull(database);

        if (!_systemDatabases.Contains(database) && !DatabaseNamePattern().IsMatch(database))
        {
            throw new ArgumentException(
                $"Name {database} contains invalid characters. Please visit: https://docs.couchdb.org/en/stable/api/database/common.html#put--db",
                nameof(database));
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
            if (_options is { Authentication: CookieCouchAuthentication, LogOutOnDispose: true })
            {
                await LogoutAsync().ConfigureAwait(false);
            }

            _flurlClient.Dispose();
        }
    }

    private async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        OperationResult? result = await _flurlClient
            .Request(Endpoint)
            .AppendPathSegment("_session")
            .DeleteAsync(cancellationToken: cancellationToken)
            .ReceiveJson<OperationResult>()
            .ConfigureAwait(false);

        if (!result.Ok)
        {
            throw new CouchDeleteException();
        }

        _authenticationHandler.ClearCookie();
    }

    #endregion

    [GeneratedRegex(@"^[a-z][a-z0-9_$()+/-]*$")]
    private static partial Regex DatabaseNamePattern();
}