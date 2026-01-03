using CouchDB.Driver.DTOs;
using CouchDB.Driver.Exceptions;
using CouchDB.Driver.Helpers;
using CouchDB.Driver.Security;
using System.IO;
using System.Linq.Expressions;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CouchDB.Driver.ChangesFeed;
using CouchDB.Driver.ChangesFeed.Responses;
using CouchDB.Driver.Indexes;
using CouchDB.Driver.Local;
using CouchDB.Driver.Options;
using CouchDB.Driver.Query;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.Views;
using CouchDB.Driver.Types;

namespace CouchDB.Driver;

/// <summary>
/// Represents a CouchDB database.
/// </summary>
/// <typeparam name="TSource">The type of database documents.</typeparam>
public partial class CouchDatabase<TSource> : ICouchDatabase<TSource>
    where TSource : class
{
    private readonly Regex _feedChangeLineStartPattern;
    private readonly IAsyncQueryProvider _queryProvider;
    private readonly HttpClient _httpClient;
    private readonly InternalCouchClientOptions _options;
    private readonly QueryContext _queryContext;
    private const string IfMatchHeader = "If-Match";
    private const string ContentTypeHeader = "Content-Type";

    /// <inheritdoc />
    public string Database => _queryContext.DatabaseName;

    /// <inheritdoc />
    public ICouchSecurity Security { get; }

    /// <inheritdoc />
    public ILocalDocuments LocalDocuments { get; }

    internal CouchDatabase(
        HttpClient httpClient,
        InternalCouchClientOptions options,
        QueryContext queryContext)
    {
        _feedChangeLineStartPattern = FeedChangeStartLinePattern();
        _httpClient = httpClient;
        _options = options;
        _queryContext = queryContext;

        var queryOptimizer = new QueryOptimizer();
        var queryTranslator = new QueryTranslator(_options);
        var querySender = new QuerySender(httpClient, queryContext);
        var queryCompiler = new QueryCompiler(queryOptimizer, queryTranslator, querySender);
        _queryProvider = new CouchQueryProvider(queryCompiler);

        Security = new CouchSecurity(NewRequest);
        LocalDocuments = new LocalDocuments(httpClient, queryContext);
    }

    #region Find

    /// <inheritdoc />
    public async Task<ReadItemResponse<TSource>?> ReadItemAsync(string id, ReadItemOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        HttpRequestBuilder request = NewRequest()
            .WithHeader("Accept", "application/json")
            .AppendPathSegment(Uri.EscapeDataString(id));

        HttpResponseMessage response = await SetFindOptions(request, options)
            .AllowHttpStatus(HttpStatusCode.NotFound)
            .GetAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (response is not { StatusCode: HttpStatusCode.OK })
        {
            return null;
        }

        var json = await response.GetStringAsync().ConfigureAwait(false);
        return JsonSerializer.Deserialize<ReadItemResponse<TSource>>(json, _options.DocumentJsonSerializerOptions);
    }

    /// <inheritdoc />
    public Task<List<TSource>> QueryAsync(string mangoQueryJson, bool? throwExceptionOnWarning = null,
        CancellationToken cancellationToken = default)
    {
        return SendQueryAsync(r => r
            .WithHeader("Content-Type", "application/json")
            .PostStringAsync(mangoQueryJson, cancellationToken: cancellationToken), throwExceptionOnWarning);
    }

    /// <inheritdoc />
    public Task<List<TSource>> QueryAsync(object mangoQuery, bool? throwExceptionOnWarning = null,
        CancellationToken cancellationToken = default)
    {
        return SendQueryAsync(r => r
            .PostJsonAsync(mangoQuery, cancellationToken: cancellationToken), throwExceptionOnWarning);
    }

    /// <inheritdoc />
    public async Task<List<TSource>> ReadItemsAsync(IList<string> ids,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        BulkGetResult<ReadItemResponse<TSource>> bulkGetResult = await NewRequest()
            .AppendPathSegment("_bulk_get")
            .PostJsonAsync(new { docs = ids.Select(id => new { id }) }, cancellationToken: cancellationToken)
            .ReceiveJson<BulkGetResult<ReadItemResponse<TSource>>>(_options.DocumentJsonSerializerOptions)
            .SendRequestAsync()
            .ConfigureAwait(false);

        var documents = bulkGetResult.Results
            .SelectMany(result => result.Docs)
            .Select(item => item.Item)
            .Where(response => response != null && (includeDeleted || !response.Deleted))
            .Select(response => response!.Document)
            .ToList();
        return documents;
    }

    private async Task<List<TSource>> SendQueryAsync(
        Func<HttpRequestBuilder, Task<HttpResponseMessage>> requestFunc,
        bool? throwExceptionOnWarning)
    {
        HttpRequestBuilder request = NewRequest()
            .AppendPathSegment("_find");

        Task<HttpResponseMessage> message = requestFunc(request);

        FindResult<TSource> findResult = await message
            .ReceiveJson<FindResult<TSource>>(_options.DocumentJsonSerializerOptions)
            .SendRequestAsync()
            .ConfigureAwait(false);

        if ((throwExceptionOnWarning ?? _options.ThrowOnQueryWarning) && !string.IsNullOrEmpty(findResult.Warning))
        {
            throw new CouchQueryWarningException(findResult.Warning);
        }

        return findResult.Docs.ToList();
    }

    #endregion

    #region Writing

    /// <inheritdoc />
    public async Task<WriteItemResponse> CreateItemAsync(TSource document,
        CreateItemRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);

        HttpRequestBuilder request = NewRequest();

        if (options?.Batch == true)
        {
            request = request.SetQueryParam("batch", "ok");
        }

        JsonObject jsonObject = GetTransformedJsonObject(document);
        if (options?.Attachments != null)
        {
            var requestAttachments = new Dictionary<string, CreateAttachmentRequest>();
            foreach (CreateItemAttachment attachment in options.Attachments)
            {
                var contentType = attachment.ContentType ??
                                  ExtensionsHelper.GetContentTypeFromExtension(attachment.FilePath);
                var fileName = attachment.Name ?? Path.GetFileName(attachment.FilePath);
                var fileBytes = await File.ReadAllBytesAsync(attachment.FilePath, cancellationToken);
                var base64Content = Convert.ToBase64String(fileBytes);
                requestAttachments[fileName] = new CreateAttachmentRequest(contentType, base64Content);
            }

            jsonObject["_attachments"] = JsonSerializer.SerializeToNode(
                requestAttachments, CouchJsonSerializerOptions.Default);
        }

        var jsonContent = JsonContent.Create(jsonObject, options: _options.DocumentJsonSerializerOptions);
        return await request
            .PostAsync(jsonContent, cancellationToken: cancellationToken)
            .ReceiveJson<WriteItemResponse>()
            .SendRequestAsync()
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<WriteItemResponse> UpdateItemAsync(TSource document, string id, string rev,
        ItemRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(document);

        HttpRequestBuilder request = NewRequest()
            .AppendPathSegment(Uri.EscapeDataString(id));

        if (options?.Batch == true)
        {
            request = request.SetQueryParam("batch", "ok");
        }

        JsonObject jsonObject = GetTransformedJsonObject(document);
        var jsonContent = JsonContent.Create(jsonObject, options: _options.DocumentJsonSerializerOptions);
        return await request
            .WithHeader(IfMatchHeader, rev)
            .PutAsync(jsonContent, cancellationToken: cancellationToken)
            .ReceiveJson<WriteItemResponse>()
            .SendRequestAsync()
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task DeleteItemAsync(string id, string rev, ItemRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        HttpRequestBuilder request = NewRequest()
            .AppendPathSegment(Uri.EscapeDataString(id));

        if (options?.Batch == true)
        {
            request = request.SetQueryParam("batch", "ok");
        }

        OperationResult result = await request
            .WithHeader(IfMatchHeader, rev)
            .DeleteAsync(cancellationToken: cancellationToken)
            .SendRequestAsync()
            .ReceiveJson<OperationResult>()
            .ConfigureAwait(false);

        if (!result.Ok)
        {
            throw new CouchDeleteException();
        }
    }

    public async Task<BulkWriteItemResponse[]> ExecuteBulkItemOperationsAsync(IList<BulkItemOperation> operations,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operations);

        List<JsonNode> docs = [];

        foreach (BulkItemOperation operation in operations)
        {
            switch (operation)
            {
                case AddItemOperation add:
                    {
                        JsonObject addNode = GetTransformedJsonObject((TSource)add.Document);
                        docs.Add(addNode);
                        break;
                    }
                case UpdateItemOperation update:
                    {
                        JsonObject updateNode = GetTransformedJsonObject((TSource)update.Document);
                        updateNode["_id"] = update.Id;
                        updateNode["_rev"] = update.Rev;
                        docs.Add(updateNode);
                        break;
                    }
                case DeleteItemOperation delete:
                    {
                        var deleteNode = new JsonObject
                        {
                            ["_id"] = delete.Id, ["_rev"] = delete.Rev, ["_deleted"] = true
                        };
                        docs.Add(deleteNode);
                        break;
                    }
            }
        }

        return await NewRequest()
            .AppendPathSegment("_bulk_docs")
            .PostJsonAsync(new { docs }, cancellationToken: cancellationToken)
            .ReceiveJson<BulkWriteItemResponse[]>()
            .SendRequestAsync()
            .ConfigureAwait(false);
    }

    private JsonObject GetTransformedJsonObject(TSource document)
    {
        var jsonObject =
            (JsonSerializer.SerializeToNode(document, _options.DocumentJsonSerializerOptions) as JsonObject)!;

        // Remove rev
        jsonObject.Remove("rev");
        jsonObject.Remove("Rev");
        jsonObject.Remove("_rev");

        // Transform id to _id for writes
        if (jsonObject.Remove("id", out JsonNode? idNode) || jsonObject.Remove("Id", out idNode))
        {
            if (idNode?.GetValue<string>() is { } id)
            {
                jsonObject["_id"] = id;
            }
        }

        return jsonObject;
    }

    #endregion

    #region Attachments

    /// <inheritdoc />
    public async Task<WriteItemResponse> UpsertAttachmentAsync(
        string id, string rev,
        string filePath, string? contentType = null, string? name = null,
        CancellationToken cancellationToken = default)
    {
        FileStream? fileStream = null;
        try
        {
            name ??= Path.GetFileName(filePath);
            contentType ??= ExtensionsHelper.GetContentTypeFromExtension(filePath);
            fileStream = new FileStream(filePath, FileMode.Open);
            using var stream = new StreamContent(fileStream);

            AttachmentResult response = await NewRequest()
                .AppendPathSegment(Uri.EscapeDataString(id))
                .AppendPathSegment(Uri.EscapeDataString(name))
                .WithHeader(ContentTypeHeader, contentType)
                .WithHeader(IfMatchHeader, rev)
                .PutAsync(stream, cancellationToken: cancellationToken)
                .ReceiveJson<AttachmentResult>()
                .ConfigureAwait(false);
            return new WriteItemResponse(response.Id, response.Rev);
        }
        finally
        {
            if (fileStream != null)
            {
                await fileStream.DisposeAsync();
            }
        }
    }

    /// <inheritdoc />
    public async Task<WriteItemResponse> DeleteAttachmentAsync(
        string name, string id, string rev,
        CancellationToken cancellationToken = default)
    {
        FileStream? fileStream = null;
        try
        {
            AttachmentResult response = await NewRequest()
                .AppendPathSegment(Uri.EscapeDataString(id))
                .AppendPathSegment(Uri.EscapeDataString(name))
                .WithHeader(IfMatchHeader, rev)
                .DeleteAsync(cancellationToken: cancellationToken)
                .ReceiveJson<AttachmentResult>()
                .ConfigureAwait(false);
            return new WriteItemResponse(response.Id, response.Rev);
        }
        finally
        {
            if (fileStream != null)
            {
                await fileStream.DisposeAsync();
            }
        }
    }

    #endregion

    #region Feed

    /// <inheritdoc />
    public async Task<ChangesFeedResponse<TSource>> GetChangesAsync(ChangesFeedOptions? options = null,
        ChangesFeedFilter? filter = null, CancellationToken cancellationToken = default)
    {
        HttpRequestBuilder request = NewRequest()
            .WithHeader("Accept", "application/json")
            .AppendPathSegment("_changes");

        if (options?.LongPoll == true)
        {
            _ = request.SetQueryParam("feed", "longpoll");
        }

        request = ApplyChangesFeedOptions(request, options);
        return filter == null
            ? await request
                .GetJsonAsync<ChangesFeedResponse<TSource>>(
                    jsonSerializerOptions: _options.DocumentJsonSerializerOptions,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false)
            : await request.QueryWithFilterAsync<TSource>(
                    _queryProvider, filter,
                    jsonSerializerOptions: _options.DocumentJsonSerializerOptions,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChangesFeedResponseResult<TSource>> GetContinuousChangesAsync(
        ChangesFeedOptions? options, ChangesFeedFilter? filter,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var infiniteTimeout = TimeSpan.FromMilliseconds(Timeout.Infinite);
        HttpRequestBuilder request = NewRequest()
            .WithTimeout(infiniteTimeout)
            .WithHeader("Accept", "application/json")
            .AppendPathSegment("_changes")
            .SetQueryParam("feed", "continuous");

        request = ApplyChangesFeedOptions(request, options);

        var lastSequence = options?.Since ?? "0";

        do
        {
            await using Stream stream = filter == null
                ? await request.GetStreamAsync(HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                    .ConfigureAwait(false)
                : await request.QueryContinuousWithFilterAsync<TSource>(_queryProvider, filter, cancellationToken)
                    .ConfigureAwait(false);

            await foreach (var line in stream.ReadLinesAsync(cancellationToken))
            {
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                MatchCollection matches = _feedChangeLineStartPattern.Matches(line);
                for (var i = 0; i < matches.Count; i++)
                {
                    var startIndex = matches[i].Index;
                    var endIndex = i < matches.Count - 1 ? matches[i + 1].Index : line.Length;
                    var lineLength = endIndex - startIndex;
                    var substring = line.Substring(startIndex, lineLength);
                    ChangesFeedResponseResult<TSource>? result =
                        JsonSerializer.Deserialize<ChangesFeedResponseResult<TSource>>(
                            substring, _options.DocumentJsonSerializerOptions);
                    lastSequence = result!.Seq;
                    yield return result;
                }
            }

            // stream broke, pick up listening after last successful processed sequence
            request = request.SetQueryParam("since", lastSequence);
        } while (!cancellationToken.IsCancellationRequested);
    }

    #endregion

    #region Index

    /// <inheritdoc />
    public async Task<IList<IndexInfo>> GetIndexesAsync(CancellationToken cancellationToken = default)
    {
        GetIndexesResult response = await NewRequest()
            .AppendPathSegment("_index")
            .GetJsonAsync<GetIndexesResult>(cancellationToken: cancellationToken)
            .SendRequestAsync()
            .ConfigureAwait(false);
        return response.Indexes;
    }

    /// <inheritdoc />
    public Task<string> CreateIndexAsync(string name,
        Action<IIndexBuilder<TSource>> indexBuilderAction,
        IndexOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(indexBuilderAction);

        IndexDefinition indexDefinition = NewIndexBuilder(indexBuilderAction).Build();
        return CreateIndexAsync(name, indexDefinition, options, cancellationToken);
    }

    private async Task<string> CreateIndexAsync(string name,
        IndexDefinition indexDefinition,
        IndexOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var indexJson = indexDefinition.ToString();

        var sb = new StringBuilder();
        sb.Append('{')
            .Append($"\"index\":{indexJson},")
            .Append($"\"name\":\"{name}\",")
            .Append("\"type\":\"json\"");

        if (options?.DesignDocument != null)
        {
            sb.Append($",\"ddoc\":\"{options.DesignDocument}\"");
        }

        if (options?.Partitioned != null)
        {
            sb.Append($",\"partitioned\":{options.Partitioned.Value.ToString().ToLowerInvariant()}");
        }

        sb.Append('}');

        var request = sb.ToString();

        CreateIndexResult result = await NewRequest()
            .WithHeader("Content-Type", "application/json")
            .AppendPathSegment("_index")
            .PostStringAsync(request, cancellationToken: cancellationToken)
            .ReceiveJson<CreateIndexResult>()
            .SendRequestAsync()
            .ConfigureAwait(false);

        return result.Id;
    }

    /// <inheritdoc />
    public async Task DeleteIndexAsync(string designDocument, string name,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(designDocument);
        ArgumentNullException.ThrowIfNull(name);

        _ = await NewRequest()
            .AppendPathSegments("_index", designDocument, "json", name)
            .DeleteAsync(cancellationToken: cancellationToken)
            .SendRequestAsync()
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task DeleteIndexAsync(IndexInfo indexInfo, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(indexInfo);

        return DeleteIndexAsync(indexInfo.DesignDocument, indexInfo.Name, cancellationToken);
    }

    #endregion

    #region View

    /// <inheritdoc/>
    public async Task<CouchViewList<TKey, TValue, TSource>> QueryViewAsync<TKey, TValue>(string design, string view,
        CouchViewOptions<TKey>? options = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(design);
        ArgumentNullException.ThrowIfNull(view);

        HttpRequestBuilder request = NewRequest()
            .AppendPathSegments("_design", design, "_view", view);

        Task<CouchViewList<TKey, TValue, TSource>> requestTask = options == null
            ? request.GetJsonAsync<CouchViewList<TKey, TValue, TSource>>(
                jsonSerializerOptions: _options.DocumentJsonSerializerOptions,
                cancellationToken: cancellationToken)
            : request
                .PostJsonAsync(options, cancellationToken: cancellationToken)
                .ReceiveJson<CouchViewList<TKey, TValue, TSource>>(
                    options: _options.DocumentJsonSerializerOptions);
        return await requestTask.SendRequestAsync().ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<CouchViewList<TKey, TValue, TSource>[]> QueryViewQueryAsync<TKey, TValue>(string design,
        string view,
        IList<CouchViewOptions<TKey>> queries, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(design);
        ArgumentNullException.ThrowIfNull(view);
        ArgumentNullException.ThrowIfNull(queries);

        HttpRequestBuilder request = NewRequest()
            .AppendPathSegments("_design", design, "_view", view, "queries");

        RunViewQueriesResult<TKey, TValue, TSource> results =
            await request
                .PostJsonAsync(new { queries }, cancellationToken: cancellationToken)
                .ReceiveJson<RunViewQueriesResult<TKey, TValue, TSource>>(_options.DocumentJsonSerializerOptions)
                .SendRequestAsync()
                .ConfigureAwait(false);

        return results.Results;
    }

    #endregion

    #region Utils

    /// <inheritdoc />
    public async Task<string> DownloadAttachmentAsync(string name, string id, string rev, string localFolderPath,
        string? localFileName = null, int bufferSize = 4096,
        CancellationToken cancellationToken = default)
    {
        Stream stream = await NewRequest()
            .AppendPathSegment(id)
            .AppendPathSegment(Uri.EscapeDataString(name))
            .WithHeader(IfMatchHeader, rev)
            .GetStreamAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        localFileName ??= name;
        var localFilePath = Path.Combine(localFolderPath, localFileName);

        await using (stream)
        {
            await using var fileStream = new FileStream(localFilePath, FileMode.Create, FileAccess.Write,
                FileShare.None, bufferSize, useAsync: true);
            await stream.CopyToAsync(fileStream, bufferSize, cancellationToken).ConfigureAwait(false);
        }

        return localFilePath;
    }

    /// <inheritdoc />
    public async Task<Stream> DownloadAttachmentAsStreamAsync(string name, string id, string rev,
        CancellationToken cancellationToken = default)
    {
        return await NewRequest()
            .AppendPathSegment(id)
            .AppendPathSegment(Uri.EscapeDataString(name))
            .WithHeader(IfMatchHeader, rev)
            .GetStreamAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task CompactAsync(CancellationToken cancellationToken = default)
    {
        OperationResult result = await NewRequest()
            .AppendPathSegment("_compact")
            .PostAsync(cancellationToken: cancellationToken)
            .ReceiveJson<OperationResult>()
            .SendRequestAsync()
            .ConfigureAwait(false);

        if (!result.Ok)
        {
            throw new CouchException("Something wrong happened while compacting.");
        }
    }

    /// <inheritdoc />
    public async Task<CouchDatabaseInfo> GetInfoAsync(CancellationToken cancellationToken = default)
    {
        return await NewRequest()
            .GetJsonAsync<CouchDatabaseInfo>(cancellationToken: cancellationToken)
            .SendRequestAsync()
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<CouchPartitionInfo> GetPartitionInfoAsync(string partitionKey,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(partitionKey);

        return await NewRequest()
            .AppendPathSegment("_partition")
            .AppendPathSegment(Uri.EscapeDataString(partitionKey))
            .GetJsonAsync<CouchPartitionInfo>(cancellationToken: cancellationToken)
            .SendRequestAsync()
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<List<TSource>> QueryPartitionAsync(string partitionKey, string mangoQueryJson,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(partitionKey);
        ArgumentNullException.ThrowIfNull(mangoQueryJson);

        FindResult<TSource> findResult = await NewRequest()
            .AppendPathSegment("_partition")
            .AppendPathSegment(Uri.EscapeDataString(partitionKey))
            .AppendPathSegment("_find")
            .WithHeader("Content-Type", "application/json")
            .PostStringAsync(mangoQueryJson, cancellationToken: cancellationToken)
            .ReceiveJson<FindResult<TSource>>(options: _options.DocumentJsonSerializerOptions)
            .SendRequestAsync()
            .ConfigureAwait(false);

        return findResult.Docs.ToList();
    }

    /// <inheritdoc />
    public async Task<List<TSource>> QueryPartitionAsync(string partitionKey, object mangoQuery,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(partitionKey);
        ArgumentNullException.ThrowIfNull(mangoQuery);

        FindResult<TSource> findResult = await NewRequest()
            .AppendPathSegment("_partition")
            .AppendPathSegment(Uri.EscapeDataString(partitionKey))
            .AppendPathSegment("_find")
            .WithHeader("Content-Type", "application/json")
            .PostJsonAsync(mangoQuery, _options.DocumentJsonSerializerOptions, cancellationToken: cancellationToken)
            .ReceiveJson<FindResult<TSource>>(_options.DocumentJsonSerializerOptions)
            .SendRequestAsync()
            .ConfigureAwait(false);

        return findResult.Docs.ToList();
    }

    /// <inheritdoc />
    public async Task<List<TSource>> GetPartitionAllDocsAsync(string partitionKey,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(partitionKey);

        AllDocsResult<TSource> result = await NewRequest()
            .AppendPathSegment("_partition")
            .AppendPathSegment(Uri.EscapeDataString(partitionKey))
            .AppendPathSegment("_all_docs")
            .SetQueryParam("include_docs", "true")
            .GetJsonAsync<AllDocsResult<TSource>>(_options.DocumentJsonSerializerOptions,
                cancellationToken: cancellationToken)
            .SendRequestAsync()
            .ConfigureAwait(false);

        var documents = result.Rows
            .Where(r => r.Doc != null)
            .Select(r => r.Doc!)
            .ToList();
        return documents;
    }

    public async Task<int> GetRevisionLimitAsync(CancellationToken cancellationToken = default)
    {
        return Convert.ToInt32(await NewRequest()
            .AppendPathSegment("_revs_limit")
            .GetStringAsync(cancellationToken: cancellationToken)
            .SendRequestAsync()
            .ConfigureAwait(false));
    }

    /// <inheritdoc />
    public async Task SetRevisionLimitAsync(int limit, CancellationToken cancellationToken = default)
    {
        using var content = new StringContent(limit.ToString());

        OperationResult result = await NewRequest()
            .AppendPathSegment("_revs_limit")
            .PutAsync(content, cancellationToken: cancellationToken)
            .ReceiveJson<OperationResult>()
            .SendRequestAsync()
            .ConfigureAwait(false);

        if (!result.Ok)
        {
            throw new CouchException("Something wrong happened while updating the revision limit.");
        }
    }

    #endregion

    #region Override

    IEnumerator IEnumerable.GetEnumerator() => AsQueryable().GetEnumerator();
    public IEnumerator<TSource> GetEnumerator() => AsQueryable().GetEnumerator();
    public Type ElementType => typeof(TSource);
    public Expression Expression => Expression.Constant(this);
    public IQueryProvider Provider => _queryProvider;

    /// <summary>
    /// Converts the request to a Mango query.
    /// </summary>
    /// <returns>The JSON containing the Mango query.</returns>
    public override string ToString()
    {
        return AsQueryable().ToString();
    }

    #endregion

    #region Helper

    /// <inheritdoc />
    public HttpRequestBuilder NewRequest()
    {
        return _httpClient
            .Request(_queryContext.Endpoint, _options.Credentials)
            .AppendPathSegment(_queryContext.EscapedDatabaseName);
    }

    internal CouchQueryable<TSource> AsQueryable()
    {
        return new CouchQueryable<TSource>(_queryProvider);
    }

    internal IndexBuilder<TSource> NewIndexBuilder(
        Action<IIndexBuilder<TSource>> indexBuilderAction)
    {
        var builder = new IndexBuilder<TSource>(_options, _queryProvider);
        indexBuilderAction(builder);
        return builder;
    }

    private static HttpRequestBuilder ApplyChangesFeedOptions(HttpRequestBuilder request, ChangesFeedOptions? options)
    {
        if (options == null)
        {
            return request;
        }

        request = request.ApplyQueryParametersOptions(options);

        // Apply custom query parameters for design document filters
        if (options.QueryParameters != null)
        {
            foreach (KeyValuePair<string, string> param in options.QueryParameters)
            {
                request = request.SetQueryParam(param.Key, param.Value);
            }
        }

        return request;
    }

    private static HttpRequestBuilder SetFindOptions(HttpRequestBuilder request,
        ReadItemOptions? options)
    {
        if (options == null)
        {
            return request;
        }

        if (options.Attachments)
        {
            request = request.SetQueryParam("attachments", "true");
        }

        if (options.AttachmentsEncodingInfo)
        {
            request = request.SetQueryParam("att_encoding_info", "true");
        }

        if (options.AttachmentsSince != null && options.AttachmentsSince.Any())
        {
            request = request.SetQueryParam("att_encoding_info", options.AttachmentsSince);
        }

        if (options.Conflicts)
        {
            request = request.SetQueryParam("conflicts", "true");
        }

        if (options.DeleteConflicts)
        {
            request = request.SetQueryParam("deleted_conflicts", "true");
        }

        if (options.DeleteConflicts)
        {
            request = request.SetQueryParam("deleted_conflicts", "true");
        }

        if (options.Latest)
        {
            request = request.SetQueryParam("latest", "true");
        }

        if (options.LocalSequence)
        {
            request = request.SetQueryParam("local_seq", "true");
        }

        if (options.Meta)
        {
            request = request.SetQueryParam("meta", "true");
        }

        if (options.OpenRevisions != null && options.OpenRevisions.Any())
        {
            request = request.SetQueryParam("open_revs", options.AttachmentsSince);
        }

        if (options.Revision != null)
        {
            request = request.SetQueryParam("rev", options.Revision);
        }

        if (options.Revisions)
        {
            request = request.SetQueryParam("revs", "true");
        }

        if (options.RevisionsInfo)
        {
            request = request.SetQueryParam("revs_info", "true");
        }

        return request;
    }

    [GeneratedRegex(@"{""seq")]
    private static partial Regex FeedChangeStartLinePattern();

    #endregion
}