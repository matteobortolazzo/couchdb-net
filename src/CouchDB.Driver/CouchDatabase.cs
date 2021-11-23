using CouchDB.Driver.DTOs;
using CouchDB.Driver.Exceptions;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.Helpers;
using CouchDB.Driver.Security;
using CouchDB.Driver.Types;
using Flurl.Http;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using Newtonsoft.Json;
using System.Net;
using System.Text.RegularExpressions;
using CouchDB.Driver.Views;

namespace CouchDB.Driver
{
    /// <summary>
    /// Represents a CouchDB database.
    /// </summary>
    /// <typeparam name="TSource">The type of database documents.</typeparam>
    public class CouchDatabase<TSource> : ICouchDatabase<TSource>
        where TSource : CouchDocument
    {
        private readonly Regex _feedChangeLineStartPattern;
        private readonly IAsyncQueryProvider _queryProvider;
        private readonly IFlurlClient _flurlClient;
        private readonly CouchOptions _options;
        private readonly QueryContext _queryContext;
        private readonly string? _discriminator;

        /// <inheritdoc />
        public string Database => _queryContext.DatabaseName;

        /// <inheritdoc />
        public ICouchSecurity Security { get; }

        /// <inheritdoc />
        public ILocalDocuments LocalDocuments { get; }

        internal CouchDatabase(IFlurlClient flurlClient, CouchOptions options, QueryContext queryContext, string? discriminator)
        {
            _feedChangeLineStartPattern = new Regex(@"{""seq");
            _flurlClient = flurlClient;
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
        public async Task<TSource?> FindAsync(string docId, bool withConflicts = false,
            CancellationToken cancellationToken = default)
        {
            IFlurlRequest request = NewRequest()
                    .AppendPathSegment(docId);

            if (withConflicts)
            {
                request = request.SetQueryParam("conflicts", "true");
            }

            IFlurlResponse? response = await request
                .AllowHttpStatus(HttpStatusCode.NotFound)
                .GetAsync(cancellationToken)
                .ConfigureAwait(false);

            return response != null && response.StatusCode == (int)HttpStatusCode.OK
                ? await response.GetJsonAsync<TSource>().ConfigureAwait(false)
                : null;
        }

        /// <inheritdoc />
        public Task<List<TSource>> QueryAsync(string mangoQueryJson, CancellationToken cancellationToken = default)
        {
            return SendQueryAsync(r => r
                .WithHeader("Content-Type", "application/json")
                .PostStringAsync(mangoQueryJson, cancellationToken));
        }

        /// <inheritdoc />
        public Task<List<TSource>> QueryAsync(object mangoQuery, CancellationToken cancellationToken = default)
        {
            return SendQueryAsync(r => r
                .PostJsonAsync(mangoQuery, cancellationToken));
        }

        /// <inheritdoc />
        public async Task<List<TSource>> FindManyAsync(IReadOnlyCollection<string> docIds, CancellationToken cancellationToken = default)
        {
            BulkGetResult<TSource> bulkGetResult = await NewRequest()
                .AppendPathSegment("_bulk_get")
                .PostJsonAsync(new
                {
                    docs = docIds.Select(id => new { id })
                }, cancellationToken)
                .ReceiveJson<BulkGetResult<TSource>>()
                .SendRequestAsync()
                .ConfigureAwait(false);

            var documents = bulkGetResult.Results
                .SelectMany(r => r.Docs)
                .Select(d => d.Item)
                .Where(i => i != null)
                .ToList();

            foreach (TSource document in documents)
            {
                if (document != null)
                {
                    InitAttachments(document);
                }
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

            var documents = findResult.Docs.ToList();

            foreach (TSource document in documents)
            {
                InitAttachments(document);
            }

            return documents;
        }

        private void InitAttachments(TSource document)
        {
            foreach (CouchAttachment attachment in document.Attachments)
            {
                attachment.DocumentId = document.Id;
                attachment.DocumentRev = document.Rev;
                var path = $"{_queryContext.EscapedDatabaseName}/{document.Id}/{Uri.EscapeUriString(attachment.Name)}";
                attachment.Uri = new Uri(_queryContext.Endpoint, path);
            }
        }

        #endregion

        #region Writing

        /// <inheritdoc />
        public async Task<TSource> AddAsync(TSource document, bool batch = false, CancellationToken cancellationToken = default)
        {
            Check.NotNull(document, nameof(document));

            if (!string.IsNullOrEmpty(document.Id))
            {
                return await AddOrUpdateAsync(document, batch, cancellationToken)
                    .ConfigureAwait(false);
            }

            IFlurlRequest request = NewRequest();

            if (batch)
            {
                request = request.SetQueryParam("batch", "ok");
            }

            document.SplitDiscriminator = _discriminator;
            DocumentSaveResponse response = await request
                .PostJsonAsync(document, cancellationToken)
                .ReceiveJson<DocumentSaveResponse>()
                .SendRequestAsync()
                .ConfigureAwait(false);
            document.ProcessSaveResponse(response);

            await UpdateAttachments(document, cancellationToken)
                .ConfigureAwait(false);

            return document;
        }

        /// <inheritdoc />
        public async Task<TSource> AddOrUpdateAsync(TSource document, bool batch = false, CancellationToken cancellationToken = default)
        {
            Check.NotNull(document, nameof(document));

            if (string.IsNullOrEmpty(document.Id))
            {
                throw new InvalidOperationException("Cannot add or update a document without an ID.");
            }

            IFlurlRequest request = NewRequest()
                .AppendPathSegment(document.Id);

            if (batch)
            {
                request = request.SetQueryParam("batch", "ok");
            }

            document.SplitDiscriminator = _discriminator;
            DocumentSaveResponse response = await request
                .PutJsonAsync(document, cancellationToken)
                .ReceiveJson<DocumentSaveResponse>()
                .SendRequestAsync()
                .ConfigureAwait(false);
            document.ProcessSaveResponse(response);

            await UpdateAttachments(document, cancellationToken)
                .ConfigureAwait(false);

            return document;
        }

        /// <inheritdoc />
        public async Task RemoveAsync(TSource document, bool batch = false, CancellationToken cancellationToken = default)
        {
            Check.NotNull(document, nameof(document));

            IFlurlRequest request = NewRequest()
                .AppendPathSegment(document.Id);

            if (batch)
            {
                request = request.SetQueryParam("batch", "ok");
            }

            OperationResult result = await request
                .SetQueryParam("rev", document.Rev)
                .DeleteAsync(cancellationToken)
                .SendRequestAsync()
                .ReceiveJson<OperationResult>()
                .ConfigureAwait(false);

            if (!result.Ok)
            {
                throw new CouchDeleteException();
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TSource>> AddOrUpdateRangeAsync(IList<TSource> documents, CancellationToken cancellationToken = default)
        {
            Check.NotNull(documents, nameof(documents));

            foreach (TSource? document in documents)
            {
                document.SplitDiscriminator = _discriminator;
            }

            DocumentSaveResponse[] response = await NewRequest()
                .AppendPathSegment("_bulk_docs")
                .PostJsonAsync(new { docs = documents }, cancellationToken)
                .ReceiveJson<DocumentSaveResponse[]>()
                .SendRequestAsync()
                .ConfigureAwait(false);

            IEnumerable<(TSource Document, DocumentSaveResponse SaveResponse)> zipped =
                documents.Zip(response, (doc, saveResponse) => (Document: doc, SaveResponse: saveResponse));

            foreach ((TSource document, DocumentSaveResponse saveResponse) in zipped)
            {
                document.ProcessSaveResponse(saveResponse);

                await UpdateAttachments(document, cancellationToken)
                    .ConfigureAwait(false);
            }

            return documents;
        }

        /// <inheritdoc />
        public async Task EnsureFullCommitAsync(CancellationToken cancellationToken = default)
        {
            OperationResult result = await NewRequest()
                .AppendPathSegment("_ensure_full_commit")
                .PostAsync(null, cancellationToken)
                .ReceiveJson<OperationResult>()
                .SendRequestAsync()
                .ConfigureAwait(false);

            if (!result.Ok)
            {
                throw new CouchException("Something wrong happened while ensuring full commits.");
            }
        }

        private async Task UpdateAttachments(TSource document, CancellationToken cancellationToken = default)
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
                    .AppendPathSegment(document.Id)
                    .AppendPathSegment(Uri.EscapeUriString(attachment.Name))
                    .WithHeader("Content-Type", attachment.ContentType)
                    .WithHeader("If-Match", document.Rev)
                    .PutAsync(stream, cancellationToken)
                    .ReceiveJson<AttachmentResult>()
                    .ConfigureAwait(false);

                if (response.Ok)
                {
                    document.Rev = response.Rev;
                    attachment.FileInfo = null;
                }
            }

            foreach (CouchAttachment attachment in document.Attachments.GetDeletedAttachments())
            {
                AttachmentResult response = await NewRequest()
                    .AppendPathSegment(document.Id)
                    .AppendPathSegment(attachment.Name)
                    .WithHeader("If-Match", document.Rev)
                    .DeleteAsync(cancellationToken)
                    .ReceiveJson<AttachmentResult>()
                    .ConfigureAwait(false);

                if (response.Ok)
                {
                    document.Rev = response.Rev;
                    document.Attachments.RemoveAttachment(attachment);
                }
            }

            InitAttachments(document);
        }

        #endregion

        #region Feed

        /// <inheritdoc />
        public async Task<ChangesFeedResponse<TSource>> GetChangesAsync(ChangesFeedOptions? options = null, ChangesFeedFilter? filter = null, CancellationToken cancellationToken = default)
        {
            IFlurlRequest request = NewRequest()
                .AppendPathSegment("_changes");

            if (options?.LongPoll == true)
            {
                _ = request.SetQueryParam("feed", "longpoll");
            }

            if (options != null)
            {
                request = request.ApplyQueryParametersOptions(options);
            }

            return filter == null
                ? await request.GetJsonAsync<ChangesFeedResponse<TSource>>(cancellationToken)
                    .ConfigureAwait(false)
                : await request.QueryWithFilterAsync<TSource>(_queryProvider, filter, cancellationToken)
                    .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<ChangesFeedResponseResult<TSource>> GetContinuousChangesAsync(ChangesFeedOptions? options, ChangesFeedFilter? filter,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var infiniteTimeout = TimeSpan.FromMilliseconds(Timeout.Infinite);
            IFlurlRequest request = NewRequest()
                .WithTimeout(infiniteTimeout)
                .AppendPathSegment("_changes")
                .SetQueryParam("feed", "continuous");

            if (options != null)
            {
                request = request.ApplyQueryParametersOptions(options);
            }

            await using Stream stream = filter == null
                ? await request.GetStreamAsync(cancellationToken, HttpCompletionOption.ResponseHeadersRead)
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
                    yield return JsonConvert.DeserializeObject<ChangesFeedResponseResult<TSource>>(substring);
                }
            }
        }

        #endregion

        #region Index

        /// <inheritdoc />
        public async Task<List<IndexInfo>> GetIndexesAsync(CancellationToken cancellationToken = default)
        {
            GetIndexesResult response = await NewRequest()
                .AppendPathSegment("_index")
                .GetJsonAsync<GetIndexesResult>(cancellationToken)
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
            Check.NotNull(name, nameof(name));
            Check.NotNull(indexBuilderAction, nameof(indexBuilderAction));

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
                sb.Append($",\"partitioned\":{options.Partitioned.ToString().ToLowerInvariant()}");
            }

            sb.Append('}');

            var request = sb.ToString();

            CreateIndexResult result = await NewRequest()
                .WithHeader("Content-Type", "application/json")
                .AppendPathSegment("_index")
                .PostStringAsync(request, cancellationToken)
                .ReceiveJson<CreateIndexResult>()
                .SendRequestAsync()
                .ConfigureAwait(false);

            return result.Id;
        }

        /// <inheritdoc />
        public async Task DeleteIndexAsync(string designDocument, string name, CancellationToken cancellationToken = default)
        {
            Check.NotNull(designDocument, nameof(designDocument));
            Check.NotNull(name, nameof(name));

            _ = await NewRequest()
                .AppendPathSegments("_index", designDocument, "json", name)
                .DeleteAsync(cancellationToken)
                .SendRequestAsync()
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public Task DeleteIndexAsync(IndexInfo indexInfo, CancellationToken cancellationToken = default)
        {
            Check.NotNull(indexInfo, nameof(indexInfo));

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
            Check.NotNull(design, nameof(design));
            Check.NotNull(view, nameof(view));

            IFlurlRequest request = NewRequest()
                .AppendPathSegments("_design", design, "_view", view);

            Task<CouchViewList<TKey, TValue, TSource>>? requestTask = options == null
                ? request.GetJsonAsync<CouchViewList<TKey, TValue, TSource>>(cancellationToken)
                : request
                    .PostJsonAsync(options, cancellationToken)
                    .ReceiveJson<CouchViewList<TKey, TValue, TSource>>();

            return requestTask.SendRequestAsync();
        }

        /// <inheritdoc/>
        public async Task<List<CouchView<TKey, TValue, TSource>>[]> GetViewQueryAsync<TKey, TValue>(string design, string view,
             IList<CouchViewOptions<TKey>> queries, CancellationToken cancellationToken = default)
        {
            CouchViewList<TKey, TValue, TSource>[] result =
                await GetDetailedViewQueryAsync<TKey, TValue>(design, view, queries, cancellationToken)
                    .ConfigureAwait(false);

            return result.Select(x => x.Rows).ToArray();
        }

        /// <inheritdoc/>
        public async Task<CouchViewList<TKey, TValue, TSource>[]> GetDetailedViewQueryAsync<TKey, TValue>(string design, string view,
             IList<CouchViewOptions<TKey>> queries, CancellationToken cancellationToken = default)
        {
            Check.NotNull(design, nameof(design));
            Check.NotNull(view, nameof(view));
            Check.NotNull(queries, nameof(queries));

            IFlurlRequest request = NewRequest()
                .AppendPathSegments("_design", design, "_view", view, "queries");

            CouchViewQueryResult<TKey, TValue, TSource> result =
                await request
                .PostJsonAsync(new { queries }, cancellationToken)
                .ReceiveJson<CouchViewQueryResult<TKey, TValue, TSource>>()
                .SendRequestAsync()
                .ConfigureAwait(false);

            return result.Results;
        }

        #endregion

        #region Utils

        /// <inheritdoc />
        public async Task<string> DownloadAttachmentAsync(CouchAttachment attachment, string localFolderPath, string? localFileName = null, int bufferSize = 4096,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(attachment, nameof(attachment));

            if (attachment.Uri == null)
            {
                throw new InvalidOperationException("The attachment is not uploaded yet.");
            }

            return await NewRequest()
                .AppendPathSegment(attachment.DocumentId)
                .AppendPathSegment(Uri.EscapeUriString(attachment.Name))
                .WithHeader("If-Match", attachment.DocumentRev)
                .DownloadFileAsync(localFolderPath, localFileName, bufferSize, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<Stream> DownloadAttachmentAsStreamAsync(CouchAttachment attachment, CancellationToken cancellationToken = default)
        {
            Check.NotNull(attachment, nameof(attachment));

            if (attachment.Uri == null)
            {
                throw new InvalidOperationException("The attachment is not uploaded yet.");
            }

            return await NewRequest()
                .AppendPathSegment(attachment.DocumentId)
                .AppendPathSegment(Uri.EscapeUriString(attachment.Name))
                .WithHeader("If-Match", attachment.DocumentRev)
                .GetStreamAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task CompactAsync(CancellationToken cancellationToken = default)
        {
            OperationResult result = await NewRequest()
                .AppendPathSegment("_compact")
                .PostJsonAsync(null, cancellationToken)
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
                .GetJsonAsync<CouchDatabaseInfo>(cancellationToken)
                .SendRequestAsync()
                .ConfigureAwait(false);
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
            return _flurlClient.Request(_queryContext.Endpoint).AppendPathSegment(_queryContext.EscapedDatabaseName);
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

        #endregion
    }
}