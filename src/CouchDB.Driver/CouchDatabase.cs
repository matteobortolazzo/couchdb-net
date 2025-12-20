using CouchDB.Driver.DTOs;
using CouchDB.Driver.Exceptions;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.Helpers;
using CouchDB.Driver.Security;
using Flurl.Http;
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
    private readonly IFlurlClient _flurlClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly CouchOptions _options;
    private readonly QueryContext _queryContext;
    private readonly string? _discriminator;
    private const string IfMatchHeader = "If-Match";

    /// <inheritdoc />
    public string Database => _queryContext.DatabaseName;

    /// <inheritdoc />
    public ICouchSecurity Security { get; }

    /// <inheritdoc />
    public ILocalDocuments LocalDocuments { get; }

    internal CouchDatabase(IFlurlClient flurlClient, JsonSerializerOptions jsonSerializerOptions, CouchOptions options,
        QueryContext queryContext,
        string? discriminator)
    {
        _feedChangeLineStartPattern = FeedChangeStartLinePattern();
        _flurlClient = flurlClient;
        _jsonSerializerOptions = jsonSerializerOptions;
        _options = options;
        _queryContext = queryContext;
        _discriminator = discriminator;

        var queryOptimizer = new QueryOptimizer();
        var queryTranslator = new QueryTranslator(options);
        var querySender = new QuerySender(flurlClient, queryContext);
        var queryCompiler = new QueryCompiler(queryOptimizer, queryTranslator, querySender, _discriminator);
        _queryProvider = new CouchQueryProvider(queryCompiler);

        Security = new CouchSecurity(NewRequest);
        LocalDocuments = new LocalDocuments(flurlClient, queryContext);
    }

    #region Find

    /// <inheritdoc />
    public async Task<FindResponse<TSource>?> FindAsync(string docId, FindDocumentRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        IFlurlRequest request = NewRequest()
            .WithHeader("Accept", "application/json")
            .AppendPathSegment(Uri.EscapeDataString(docId));

        IFlurlResponse? response = await SetFindOptions(request, options)
            .AllowHttpStatus((int)HttpStatusCode.NotFound)
            .GetAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (response is not { StatusCode: (int)HttpStatusCode.OK })
        {
            return null;
        }

        var json = await response.GetStringAsync().ConfigureAwait(false);
        return JsonSerializer.Deserialize<FindResponse<TSource>>(json, _jsonSerializerOptions);
    }

    /// <inheritdoc />
    public Task<List<TSource>> QueryAsync(string mangoQueryJson, CancellationToken cancellationToken = default)
    {
        return SendQueryAsync(r => r
            .WithHeader("Content-Type", "application/json")
            .PostStringAsync(mangoQueryJson, cancellationToken: cancellationToken));
    }

    /// <inheritdoc />
    public Task<List<TSource>> QueryAsync(object mangoQuery, CancellationToken cancellationToken = default)
    {
        return SendQueryAsync(r => r
            .PostJsonAsync(mangoQuery, cancellationToken: cancellationToken));
    }

    /// <inheritdoc />
    public async Task<List<TSource>> FindManyAsync(IReadOnlyCollection<string> docIds,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        BulkGetResult<TSource> bulkGetResult = await NewRequest()
            .AppendPathSegment("_bulk_get")
            .PostJsonAsync(new { docs = docIds.Select(id => new { id }) }, cancellationToken: cancellationToken)
            .ReceiveJson<BulkGetResult<TSource>>()
            .SendRequestAsync()
            .ConfigureAwait(false);

        var documents = bulkGetResult.Results
            .SelectMany(r => r.Docs)
            .Select(d => d.Item)
            // .Where(i => i != null && (includeDeleted || !i.Deleted)) TODO: Review
            .Where(i => i != null)
            .Cast<TSource>()
            .ToList();

        foreach (TSource document in documents.OfType<TSource>())
        {
            InitAttachments(document);
        }

        return documents;
    }

    private async Task<List<TSource>> SendQueryAsync(Func<IFlurlRequest, Task<IFlurlResponse>> requestFunc)
    {
        IFlurlRequest request = NewRequest()
            .AppendPathSegment("_find");

        Task<IFlurlResponse> message = requestFunc(request);

        FindResult<TSource> findResult = await message
            .ReceiveJson<FindResult<TSource>>()
            .SendRequestAsync()
            .ConfigureAwait(false);

        if (this._options.ThrowOnQueryWarning && !string.IsNullOrEmpty(findResult.Warning))
        {
            throw new CouchQueryWarningException(findResult.Warning);
        }

        var documents = findResult.Docs.ToList();

        foreach (TSource document in documents)
        {
            InitAttachments(document);
        }

        return documents;
    }

    private void InitAttachments(TSource document)
    {
        return;
        // TODO: Real value
        /*
        string id = "asdasda";
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(document.Rev);

        foreach (CouchAttachment attachment in document.Attachments)
        {
            attachment.DocumentId = id;
            attachment.DocumentRev = document.Rev;
            var path = $"{_queryContext.EscapedDatabaseName}/{id}/{Uri.EscapeDataString(attachment.Name)}";
            attachment.Uri = new Uri(_queryContext.Endpoint, path);
        }
        */
    }

    #endregion

    #region Writing

    /// <inheritdoc />
    public async Task<DocumentRequestResponse> AddAsync(TSource document, DocumentRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);

        IFlurlRequest request = NewRequest();

        if (options?.Batch == true)
        {
            request = request.SetQueryParam("batch", "ok");
        }

        JsonObject jsonObject = GetTransformedJsonObject(document);
        var jsonContent = JsonContent.Create(jsonObject, options: _jsonSerializerOptions);
        DocumentSaveResponse response = await request
            .PostJsonAsync(jsonContent, cancellationToken: cancellationToken)
            .ReceiveJson<DocumentSaveResponse>()
            .SendRequestAsync()
            .ConfigureAwait(false);

        if (!response.Ok)
        {
            throw new CouchException(response.Error, null, response.Reason);
        }

        await UpdateAttachments(document, response.Id, response.Rev!, cancellationToken)
            .ConfigureAwait(false);
        return new DocumentRequestResponse(response.Id, response.Rev!);
    }

    /// <inheritdoc />
    public async Task<DocumentRequestResponse> ReplaceAsync(TSource document, string id, string rev,
        DocumentRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(document);

        IFlurlRequest request = NewRequest()
            .AppendPathSegment(Uri.EscapeDataString(id));

        if (options?.Batch == true)
        {
            request = request.SetQueryParam("batch", "ok");
        }

        JsonObject jsonObject = GetTransformedJsonObject(document);
        var jsonContent = JsonContent.Create(jsonObject, options: _jsonSerializerOptions);
        DocumentSaveResponse response = await request
            .WithHeader(IfMatchHeader, rev)
            .PutJsonAsync(jsonContent, cancellationToken: cancellationToken)
            .ReceiveJson<DocumentSaveResponse>()
            .SendRequestAsync()
            .ConfigureAwait(false);

        if (!response.Ok)
        {
            throw new CouchException(response.Error, null, response.Reason);
        }

        await UpdateAttachments(document, id, rev, cancellationToken)
            .ConfigureAwait(false);
        return new DocumentRequestResponse(response.Id, response.Rev!);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string id, string rev, DocumentRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        IFlurlRequest request = NewRequest()
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

    public async Task<DocumentBulkRequestResponse[]> BulkAsync(IList<BulkOperation> operations,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operations);

        List<JsonNode> docs = [];

        foreach (BulkOperation operation in operations)
        {
            switch (operation)
            {
                case AddOperation add:
                    {
                        JsonObject addNode = GetTransformedJsonObject((TSource)add.Document);
                        docs.Add(addNode);
                        break;
                    }
                case UpdateOperation update:
                    {
                        JsonObject updateNode = GetTransformedJsonObject((TSource)update.Document);
                        updateNode["_id"] = update.Id;
                        updateNode["_rev"] = update.Rev;
                        docs.Add(updateNode);
                        break;
                    }
                case DeleteOperation delete:
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

        DocumentSaveResponse[] responses = await NewRequest()
            .AppendPathSegment("_bulk_docs")
            .PostJsonAsync(new { docs }, cancellationToken: cancellationToken)
            .ReceiveJson<DocumentSaveResponse[]>()
            .SendRequestAsync()
            .ConfigureAwait(false);

        return responses
            .Select(response => new DocumentBulkRequestResponse(
                response.Ok,
                response.Id,
                response.Rev,
                response.Error,
                response.Reason))
            .ToArray();
    }

    private JsonObject GetTransformedJsonObject(TSource document)
    {
        var jsonObject = (JsonSerializer.SerializeToNode(document, _jsonSerializerOptions) as JsonObject)!;

        // Set discriminator if needed
        if (!string.IsNullOrWhiteSpace(_discriminator))
        {
            jsonObject[CouchClient.DefaultDatabaseSplitDiscriminator] = _discriminator;
        }

        // Remove rev
        jsonObject.Remove("rev");

        // Transform id to _id for writes
        var currentId = jsonObject["id"]?.GetValue<string>();
        if (currentId != null)
        {
            jsonObject["_id"] = currentId;
            jsonObject.Remove("id");
        }

        return jsonObject;
    }

    private async Task UpdateAttachments(TSource document, string id, string rev,
        CancellationToken cancellationToken = default)
    {
        foreach (CouchAttachment attachment in document.Attachments.GetAddedAttachments())
        {
            if (attachment.FileInfo == null)
            {
                continue;
            }

            using var stream = new StreamContent(
                new FileStream(attachment.FileInfo.FullName, FileMode.Open));

            AttachmentResult response = await NewRequest()
                .AppendPathSegment(Uri.EscapeDataString(id))
                .AppendPathSegment(Uri.EscapeDataString(attachment.Name))
                .WithHeader("Content-Type", attachment.ContentType)
                .WithHeader("If-Match", rev)
                .PutAsync(stream, cancellationToken: cancellationToken)
                .ReceiveJson<AttachmentResult>()
                .ConfigureAwait(false);

            if (!response.Ok)
            {
                continue;
            }

            // document.Rev = response.Rev;
            attachment.FileInfo = null;
        }

        foreach (CouchAttachment attachment in document.Attachments.GetDeletedAttachments())
        {
            AttachmentResult response = await NewRequest()
                .AppendPathSegment(Uri.EscapeDataString(id))
                .AppendPathSegment(Uri.EscapeDataString(attachment.Name))
                .WithHeader("If-Match", rev)
                .DeleteAsync(cancellationToken: cancellationToken)
                .ReceiveJson<AttachmentResult>()
                .ConfigureAwait(false);

            if (response.Ok)
            {
                // document.Rev = response.Rev;
                document.Attachments.RemoveAttachment(attachment);
            }
        }

        InitAttachments(document);
    }

    #endregion

    #region Feed

    /// <inheritdoc />
    public async Task<ChangesFeedResponse<TSource>> GetChangesAsync(ChangesFeedOptions? options = null,
        ChangesFeedFilter? filter = null, CancellationToken cancellationToken = default)
    {
        IFlurlRequest request = NewRequest()
            .WithHeader("Accept", "application/json")
            .AppendPathSegment("_changes");

        if (options?.LongPoll == true)
        {
            _ = request.SetQueryParam("feed", "longpoll");
        }

        request = ApplyChangesFeedOptions(request, options);

        ChangesFeedResponse<JsonObject>? response = filter == null
            ? await request.GetJsonAsync<ChangesFeedResponse<JsonObject>>(cancellationToken: cancellationToken)
                .ConfigureAwait(false)
            : await request.QueryWithFilterAsync<JsonObject>(_queryProvider, filter, cancellationToken)
                .ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(_discriminator))
        {
            response.Results = response.Results
                .Where(result => result.Document![CouchClient.DefaultDatabaseSplitDiscriminator]!.GetValue<string>() ==
                                 _discriminator)
                .ToArray();
        }

        var convertedResults = response.Results
            .Select(result => result.Document.Deserialize<TSource>())
            .ToList();

        return new ChangesFeedResponse<TSource>
        {
            LastSequence = response.LastSequence, Pending = response.Pending, Results = convertedResults
        };
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChangesFeedResponseResult<TSource>> GetContinuousChangesAsync(
        ChangesFeedOptions? options, ChangesFeedFilter? filter,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var infiniteTimeout = TimeSpan.FromMilliseconds(Timeout.Infinite);
        IFlurlRequest request = NewRequest()
            .WithTimeout(infiniteTimeout)
            .WithHeader("Accept", "application/json")
            .AppendPathSegment("_changes")
            .SetQueryParam("feed", "continuous");

        request = ApplyChangesFeedOptions(request, options);

        var lastSequence = options?.Since ?? "0";

        do
        {
            await using Stream stream = filter == null
                ? await request.GetStreamAsync(cancellationToken: cancellationToken)
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
                        JsonSerializer.Deserialize<ChangesFeedResponseResult<TSource>>(substring);
                    if (string.IsNullOrWhiteSpace(_discriminator) ||
                        result?.Document?.SplitDiscriminator == _discriminator)
                    {
                        lastSequence = result!.Seq;
                        yield return result;
                    }
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

    internal async Task<string> CreateIndexAsync(string name,
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
    public async Task<List<CouchView<TKey, TValue, TSource>>> GetViewAsync<TKey, TValue>(string design, string view,
        CouchViewOptions<TKey>? options = null, CancellationToken cancellationToken = default)
    {
        CouchViewList<TKey, TValue, TSource> result =
            await GetDetailedViewAsync<TKey, TValue>(design, view, options, cancellationToken)
                .ConfigureAwait(false);
        return result.Rows;
    }

    /// <inheritdoc/>
    public Task<CouchViewList<TKey, TValue, TSource>> GetDetailedViewAsync<TKey, TValue>(string design, string view,
        CouchViewOptions<TKey>? options = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(design);
        ArgumentNullException.ThrowIfNull(view);

        IFlurlRequest request = NewRequest()
            .AppendPathSegments("_design", design, "_view", view);

        Task<CouchViewList<TKey, TValue, TSource>>? requestTask = options == null
            ? request.GetJsonAsync<CouchViewList<TKey, TValue, TSource>>(cancellationToken: cancellationToken)
            : request
                .PostJsonAsync(options, cancellationToken: cancellationToken)
                .ReceiveJson<CouchViewList<TKey, TValue, TSource>>();

        return requestTask.SendRequestAsync();
    }

    /// <inheritdoc/>
    public async Task<List<CouchView<TKey, TValue, TSource>>[]> GetViewQueryAsync<TKey, TValue>(string design,
        string view,
        IList<CouchViewOptions<TKey>> queries, CancellationToken cancellationToken = default)
    {
        CouchViewList<TKey, TValue, TSource>[] result =
            await GetDetailedViewQueryAsync<TKey, TValue>(design, view, queries, cancellationToken)
                .ConfigureAwait(false);

        return result.Select(x => x.Rows).ToArray();
    }

    /// <inheritdoc/>
    public async Task<CouchViewList<TKey, TValue, TSource>[]> GetDetailedViewQueryAsync<TKey, TValue>(string design,
        string view,
        IList<CouchViewOptions<TKey>> queries, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(design);
        ArgumentNullException.ThrowIfNull(view);
        ArgumentNullException.ThrowIfNull(queries);

        IFlurlRequest request = NewRequest()
            .AppendPathSegments("_design", design, "_view", view, "queries");

        CouchViewQueryResult<TKey, TValue, TSource> result =
            await request
                .PostJsonAsync(new { queries }, cancellationToken: cancellationToken)
                .ReceiveJson<CouchViewQueryResult<TKey, TValue, TSource>>()
                .SendRequestAsync()
                .ConfigureAwait(false);

        return result.Results;
    }

    #endregion

    #region Utils

    /// <inheritdoc />
    public async Task<string> DownloadAttachmentAsync(CouchAttachment attachment, string localFolderPath,
        string? localFileName = null, int bufferSize = 4096,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(attachment);

        if (attachment.Uri == null)
        {
            throw new InvalidOperationException("The attachment is not uploaded yet.");
        }

        return await NewRequest()
            .AppendPathSegment(attachment.DocumentId)
            .AppendPathSegment(Uri.EscapeDataString(attachment.Name))
            .WithHeader("If-Match", attachment.DocumentRev)
            .DownloadFileAsync(localFolderPath, localFileName, bufferSize, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Stream> DownloadAttachmentAsStreamAsync(CouchAttachment attachment,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(attachment);

        if (attachment.Uri == null)
        {
            throw new InvalidOperationException("The attachment is not uploaded yet.");
        }

        return await NewRequest()
            .AppendPathSegment(Uri.EscapeDataString(attachment.DocumentId))
            .AppendPathSegment(Uri.EscapeDataString(attachment.Name))
            .WithHeader("If-Match", attachment.DocumentRev)
            .GetStreamAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task CompactAsync(CancellationToken cancellationToken = default)
    {
        OperationResult result = await NewRequest()
            .AppendPathSegment("_compact")
            .PostJsonAsync(null, cancellationToken: cancellationToken)
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
    public Task<List<TSource>> QueryPartitionAsync(string partitionKey, string mangoQueryJson,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(partitionKey);
        ArgumentNullException.ThrowIfNull(mangoQueryJson);

        return QueryPartitionInternalAsync(partitionKey, r => r
            .WithHeader("Content-Type", "application/json")
            .PostStringAsync(mangoQueryJson, cancellationToken: cancellationToken));
    }

    /// <inheritdoc />
    public Task<List<TSource>> QueryPartitionAsync(string partitionKey, object mangoQuery,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(partitionKey);
        ArgumentNullException.ThrowIfNull(mangoQuery);

        return QueryPartitionInternalAsync(partitionKey, r => r
            .PostJsonAsync(mangoQuery, cancellationToken: cancellationToken));
    }

    /// <inheritdoc />
    public async Task<List<TSource>> GetPartitionAllDocsAsync(string partitionKey,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(partitionKey);

        AllDocsResult<TSource>? result = await NewRequest()
            .AppendPathSegment("_partition")
            .AppendPathSegment(Uri.EscapeDataString(partitionKey))
            .AppendPathSegment("_all_docs")
            .SetQueryParam("include_docs", "true")
            .GetJsonAsync<AllDocsResult<TSource>>(cancellationToken: cancellationToken)
            .SendRequestAsync()
            .ConfigureAwait(false);

        var documents = result.Rows
            .Where(r => r.Doc != null)
            .Select(r => r.Doc!)
            .ToList();

        foreach (TSource document in documents)
        {
            InitAttachments(document);
        }

        return documents;
    }

    private async Task<List<TSource>> QueryPartitionInternalAsync(string partitionKey,
        Func<IFlurlRequest, Task<IFlurlResponse>> requestFunc)
    {
        IFlurlRequest request = NewRequest()
            .AppendPathSegment("_partition")
            .AppendPathSegment(Uri.EscapeDataString(partitionKey))
            .AppendPathSegment("_find");

        Task<IFlurlResponse> message = requestFunc(request);

        FindResult<TSource> findResult = await message
            .ReceiveJson<FindResult<TSource>>()
            .SendRequestAsync()
            .ConfigureAwait(false);

        var documents = findResult.Docs.ToList();

        foreach (TSource document in documents)
        {
            InitAttachments(document);
        }

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
    public IFlurlRequest NewRequest()
    {
        return _flurlClient
            .Request(_queryContext.Endpoint)
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

    private static IFlurlRequest ApplyChangesFeedOptions(IFlurlRequest request, ChangesFeedOptions? options)
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

    private static IFlurlRequest SetFindOptions(IFlurlRequest request,
        FindDocumentRequestOptions? options)
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