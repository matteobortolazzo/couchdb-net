using CouchDB.Driver.Extensions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CouchDB.Driver.DTOs;
using CouchDB.Driver.Exceptions;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using CouchDB.Driver.Helpers;
using CouchDB.Driver.Options;
using CouchDB.Driver.Query;
using CouchDB.Driver.Types;

namespace CouchDB.Driver;

/// <summary>
/// Client for querying a CouchDB database.
/// </summary>
public partial class CouchClient : ICouchClient
{
    private readonly HttpClient _httpClient;
    private readonly CouchCredentials _credentials;
    private readonly InternalCouchClientOptions _options;
    private readonly string[] _systemDatabases = ["_users", "_replicator", "_global_changes"];
    public Uri Endpoint { get; }

    /// <summary>
    /// Creates a new CouchDB client.
    /// </summary>
    /// <param name="endpoint">URI to the CouchDB endpoint.</param>
    /// <param name="credentials"></param>
    /// <param name="options"></param>
    public CouchClient(
        string endpoint,
        CouchCredentials credentials,
        CouchClientOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentNullException.ThrowIfNull(credentials);

        JsonSerializerOptions jsonSerializerOptions = options?.JsonSerializerOptions != null
            ? new JsonSerializerOptions(options.JsonSerializerOptions)
            : new JsonSerializerOptions();

        if (options?.JsonSerializerOptions?.TypeInfoResolver != null)
        {
            jsonSerializerOptions.TypeInfoResolver = JsonTypeInfoResolver.Combine(
                options.JsonSerializerOptions.TypeInfoResolver,
                new DefaultJsonTypeInfoResolver()
            );
        }

        _httpClient = options?.HttpClient ?? CreateDefaultHttpClient();
        _options = new InternalCouchClientOptions(
            credentials,
            jsonSerializerOptions,
            options?.LogOutOnDispose ?? true,
            options?.ThrowOnQueryWarning ?? true);

        _credentials = credentials;
        Endpoint = new Uri(endpoint);
    }

    private static HttpClient CreateDefaultHttpClient()
    {
        var socketHandler = new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(15) };
        return new HttpClient(socketHandler);
    }

    #region Operations

    #region CRUD

    /// <inheritdoc />
    public ICouchDatabase<TSource> GetDatabase<TSource>(string database)
        where TSource : class
    {
        CheckDatabaseName(database);
        var queryContext = new QueryContext(Endpoint, _options, database);
        return new CouchDatabase<TSource>(_httpClient, _options, queryContext);
    }

    /// <inheritdoc />
    public async Task<ICouchDatabase<TSource>> CreateDatabaseAsync<TSource>(string database,
        CreateDatabaseOptions? options = null,
        CancellationToken cancellationToken = default)
        where TSource : class
    {
        QueryContext queryContext = NewQueryContext(database);
        HttpResponseMessage response =
            await CreateDatabaseAsync(queryContext, options, cancellationToken)
                .ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            return new CouchDatabase<TSource>(_httpClient, _options, queryContext);
        }

        if (response.StatusCode == HttpStatusCode.PreconditionFailed)
        {
            throw new CouchException($"Database with name {database} already exists.");
        }

        throw new CouchException($"Something wrong happened while creating database {database}.");
    }

    /// <inheritdoc />
    public async Task<ICouchDatabase<TSource>> GetOrCreateDatabaseAsync<TSource>(string database,
        CreateDatabaseOptions? options = null,
        CancellationToken cancellationToken = default)
        where TSource : class
    {
        QueryContext queryContext = NewQueryContext(database);
        HttpResponseMessage response =
            await CreateDatabaseAsync(queryContext, options, cancellationToken)
                .ConfigureAwait(false);

        if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.PreconditionFailed)
        {
            return new CouchDatabase<TSource>(_httpClient, _options, queryContext);
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

    private Task<HttpResponseMessage> CreateDatabaseAsync(QueryContext queryContext,
        CreateDatabaseOptions? options,
        CancellationToken cancellationToken = default)
    {
        HttpRequestBuilder request = NewRequest()
            .AppendPathSegment(queryContext.EscapedDatabaseName);

        if (options != null)
        {
            if (options.Shards.HasValue && options.Shards.Value != 8)
            {
                request = request.SetQueryParam("q", options.Shards.Value);
            }

            if (options.Replicas.HasValue && options.Replicas.Value != 3)
            {
                request = request.SetQueryParam("n", options.Replicas.Value);
            }

            if (options.Partitioned.HasValue && options.Partitioned.Value)
            {
                request = request.SetQueryParam("partitioned", "true");
            }
        }

        return request
            .AllowHttpStatus(HttpStatusCode.PreconditionFailed)
            .PutAsync(cancellationToken: cancellationToken)
            .SendRequestAsync();
    }

    #endregion

    #region Users

    private const string UsersDatabaseName = "_users";

    /// <inheritdoc />
    public ICouchDatabase<DatabaseUser> GetUsersDatabase()
    {
        return GetDatabase<DatabaseUser>(UsersDatabaseName);
    }

    /// <inheritdoc />
    public ICouchDatabase<TUser> GetUsersDatabase<TUser>() where TUser : DatabaseUser
    {
        return GetDatabase<TUser>(UsersDatabaseName);
    }

    /// <inheritdoc />
    public Task<ICouchDatabase<DatabaseUser>> GetOrCreateUsersDatabaseAsync(CancellationToken cancellationToken = default)
    {
        return GetOrCreateDatabaseAsync<DatabaseUser>(UsersDatabaseName, null, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ICouchDatabase<TUser>> GetOrCreateUsersDatabaseAsync<TUser>(
        CancellationToken cancellationToken = default) where TUser : DatabaseUser
    {
        return GetOrCreateDatabaseAsync<TUser>(UsersDatabaseName, null, cancellationToken);
    }

    #endregion

    #region Utils

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string database, CancellationToken cancellationToken = default)
    {
        QueryContext queryContext = NewQueryContext(database);
        HttpResponseMessage response = await NewRequest()
            .AllowHttpStatus(HttpStatusCode.NotFound)
            .AppendPathSegment(queryContext.EscapedDatabaseName)
            .HeadAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return response.IsSuccessStatusCode;
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
            return result.Status == "ok";
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
    public async Task<IReadOnlyCollection<ActiveTask>> GetActiveTasksAsync(
        CancellationToken cancellationToken = default)
    {
        return await NewRequest()
            .AppendPathSegment("_active_tasks")
            .GetJsonAsync<ActiveTask[]>(cancellationToken: cancellationToken)
            .SendRequestAsync()
            .ConfigureAwait(false);
    }

    #endregion

    #region Replication

    public async Task<bool> ReplicateAsync(string source, string target, Replication? replication = null,
        bool persistent = true, CancellationToken cancellationToken = default)
    {
        HttpRequestBuilder request = NewRequest();

        replication ??= new Replication();

        if (replication.SourceCredentials == null)
        {
            replication.Source = source;
        }
        else
        {
            replication.Source = new ReplicationHost(source,
                new ReplicationAuth(replication.SourceCredentials));
        }

        if (replication.TargetCredentials == null)
        {
            replication.Target = target;
        }
        else
        {
            replication.Target = new ReplicationHost(target,
                new ReplicationAuth(replication.TargetCredentials));
        }

        OperationResult result = await request
            .AppendPathSegments(persistent ? "_replicator" : "_replicate")
            .PostJsonAsync(replication, cancellationToken: cancellationToken)
            .ReceiveJson<OperationResult>()
            .SendRequestAsync()
            .ConfigureAwait(false);

        return result.Ok;
    }

    public async Task<bool> RemoveReplicationAsync(string source, string target, Replication? replication = null,
        bool persistent = true, CancellationToken cancellationToken = default)
    {
        HttpRequestBuilder request = NewRequest();

        replication ??= new Replication();

        if (replication.SourceCredentials == null)
        {
            replication.Source = source;
        }
        else
        {
            replication.Source = new ReplicationHost(source,
                new ReplicationAuth(replication.SourceCredentials));
        }

        if (replication.TargetCredentials == null)
        {
            replication.Target = target;
        }
        else
        {
            replication.Target = new ReplicationHost(target,
                new ReplicationAuth(replication.TargetCredentials));
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

    private HttpRequestBuilder NewRequest()
    {
        return _httpClient.Request(Endpoint, _options.Credentials);
    }

    private QueryContext NewQueryContext(string database)
    {
        CheckDatabaseName(database);
        return new QueryContext(Endpoint, _options, database);
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
            if (_credentials is CookieCredentials && _options.LogOutOnDispose)
            {
                await LogoutAsync().ConfigureAwait(false);
            }

            _httpClient.Dispose();
        }
    }

    private async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        OperationResult result = await _httpClient
            .Request(Endpoint, _options.Credentials)
            .AppendPathSegment("_session")
            .DeleteAsync(cancellationToken: cancellationToken)
            .ReceiveJson<OperationResult>()
            .ConfigureAwait(false);

        if (!result.Ok)
        {
            throw new CouchDeleteException();
        }
    }

    #endregion

    [GeneratedRegex(@"^[a-z][a-z0-9_$()+/-]*$")]
    private static partial Regex DatabaseNamePattern();
}