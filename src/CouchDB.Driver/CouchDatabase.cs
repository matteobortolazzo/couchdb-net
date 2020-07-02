using CouchDB.Driver.DTOs;
using CouchDB.Driver.Exceptions;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.Helpers;
using CouchDB.Driver.Security;
using CouchDB.Driver.Settings;
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
using System.Threading;
using System.Threading.Tasks;
using CouchDB.Driver.Query;
using Newtonsoft.Json;

namespace CouchDB.Driver
{
    /// <summary>
    /// Represents a CouchDB database.
    /// </summary>
    /// <typeparam name="TSource">The type of database documents.</typeparam>
    public class CouchDatabase<TSource>: ICouchDatabase<TSource>
        where TSource : CouchDocument
    {
        private readonly IAsyncQueryProvider _queryProvider;
        private readonly IFlurlClient _flurlClient;
        private readonly CouchSettings _settings;
        private readonly QueryContext _queryContext;

        /// <inheritdoc />
        public string Database => _queryContext.DatabaseName;

        /// <inheritdoc />
        public ICouchSecurity Security { get; }

        internal CouchDatabase(IFlurlClient flurlClient, CouchSettings settings, QueryContext queryContext)
        {
            _flurlClient = flurlClient;
            _settings = settings;
            _queryContext = queryContext;

            var queryOptimizer = new QueryOptimizer();
            var queryTranslator = new QueryTranslator(settings);
            var querySender = new QuerySender(flurlClient, queryContext);
            var queryCompiler = new QueryCompiler(queryOptimizer, queryTranslator, querySender);
            _queryProvider = new CouchQueryProvider(queryCompiler);

            Security = new CouchSecurity(NewRequest);
        }

        #region Find

        /// <inheritdoc />
        public async Task<TSource?> FindAsync(string docId, bool withConflicts = false)
        {
            try
            {
                IFlurlRequest request = NewRequest()
                        .AppendPathSegment(docId);

                if (withConflicts)
                {
                    request = request.SetQueryParam("conflicts", true);
                }

                TSource document = await request
                    .GetJsonAsync<TSource>()
                    .SendRequestAsync()
                    .ConfigureAwait(false);

                InitAttachments(document);
                return document;
            }
            catch (CouchNotFoundException)
            {
                return null;
            }
        }

        /// <inheritdoc />
        public Task<List<TSource>> QueryAsync(string mangoQueryJson)
        {
            return SendQueryAsync(r => r
                .WithHeader("Content-Type", "application/json")
                .PostStringAsync(mangoQueryJson));
        }

        /// <inheritdoc />
        public Task<List<TSource>> QueryAsync(object mangoQuery)
        {
            return SendQueryAsync(r => r
                .PostJsonAsync(mangoQuery));
        }

        /// <inheritdoc />
        public async Task<List<TSource>> FindManyAsync(IEnumerable<string> docIds)
        {
            BulkGetResult<TSource> bulkGetResult = await NewRequest()
                .AppendPathSegment("_bulk_get")
                .PostJsonAsync(new
                {
                    docs = docIds.Select(id => new { id })
                }).ReceiveJson<BulkGetResult<TSource>>()
                .SendRequestAsync()
                .ConfigureAwait(false);
                       
            var documents = bulkGetResult.Results
                .SelectMany(r => r.Docs)
                .Select(d => d.Item)
                .ToList();

            foreach (TSource document in documents)
            {
                if (document != null)
                    InitAttachments(document);
            }

            return documents;
        }

        private async Task<List<TSource>> SendQueryAsync(Func<IFlurlRequest, Task<HttpResponseMessage>> requestFunc)
        {
            IFlurlRequest request = NewRequest()
                .AppendPathSegment("_find");

            Task<HttpResponseMessage> message = requestFunc(request);

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
        public async Task<TSource> CreateAsync(TSource document, bool batch = false)
        {
            Check.NotNull(document, nameof(document));

            if (!string.IsNullOrEmpty(document.Id))
            {
                return await CreateOrUpdateAsync(document)
                    .ConfigureAwait(false);
            }

            IFlurlRequest request = NewRequest();

            if (batch)
            {
                request = request.SetQueryParam("batch", "ok");
            }

            DocumentSaveResponse response = await request
                .PostJsonAsync(document)
                .ReceiveJson<DocumentSaveResponse>()
                .SendRequestAsync()
                .ConfigureAwait(false);
            document.ProcessSaveResponse(response);

            await UpdateAttachments(document)
                .ConfigureAwait(false);

            return document;
        }

        /// <inheritdoc />
        public async Task<TSource> CreateOrUpdateAsync(TSource document, bool batch = false)
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

            DocumentSaveResponse response = await request
                .PutJsonAsync(document)
                .ReceiveJson<DocumentSaveResponse>()
                .SendRequestAsync()
                .ConfigureAwait(false);
            document.ProcessSaveResponse(response);

            await UpdateAttachments(document)
                .ConfigureAwait(false);

            return document;
        }

        /// <inheritdoc />
        public async Task DeleteAsync(TSource document, bool batch = false)
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
                .DeleteAsync()
                .SendRequestAsync()
                .ReceiveJson<OperationResult>()
                .ConfigureAwait(false);

            if (!result.Ok)
            {
                throw new CouchDeleteException();
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TSource>> CreateOrUpdateRangeAsync(IList<TSource> documents)
        {
            Check.NotNull(documents, nameof(documents));

            DocumentSaveResponse[] response = await NewRequest()
                .AppendPathSegment("_bulk_docs")
                .PostJsonAsync(new { docs = documents })
                .ReceiveJson<DocumentSaveResponse[]>()
                .SendRequestAsync()
                .ConfigureAwait(false);

            IEnumerable<(TSource Document, DocumentSaveResponse SaveResponse)> zipped =
                documents.Zip(response, (doc, saveResponse) => (Document: doc, SaveResponse: saveResponse));

            foreach ((TSource document, DocumentSaveResponse saveResponse) in zipped)
            {
                document.ProcessSaveResponse(saveResponse);

                await UpdateAttachments(document)
                    .ConfigureAwait(false);
            }

            return documents;
        }

        /// <inheritdoc />
        public async Task EnsureFullCommitAsync()
        {
            OperationResult result = await NewRequest()
                .AppendPathSegment("_ensure_full_commit")
                .PostAsync(null)
                .ReceiveJson<OperationResult>()
                .SendRequestAsync()
                .ConfigureAwait(false);

            if (!result.Ok)
            {
                throw new CouchException("Something wrong happened while ensuring full commits.");
            }
        }

        private async Task UpdateAttachments(TSource document)
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
                    .PutAsync(stream)
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
                    .DeleteAsync()
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
        public async Task<ChangesFeedResponse<TSource>> GetChangesAsync(ChangesFeedOptions? options = null, ChangesFeedFilter? filter = null)
        {
            IFlurlRequest request = NewRequest()
                .AppendPathSegment("_changes");

            if (options?.LongPoll == true)
            {
                _ = request.SetQueryParam("feed", "longpoll");
            }

            if (options != null)
            {
                request.SetChangesFeedOptions(options);
            }

            return filter == null
                ? await request.GetJsonAsync<ChangesFeedResponse<TSource>>()
                    .ConfigureAwait(false)
                : await request.QueryWithFilterAsync<TSource>(_settings, filter)
                    .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<ChangesFeedResponseResult<TSource>> GetContinuousChangesAsync(ChangesFeedOptions options, ChangesFeedFilter filter,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var infiniteTimeout = TimeSpan.FromMilliseconds(Timeout.Infinite);
            IFlurlRequest request = NewRequest()
                .WithTimeout(infiniteTimeout)
                .AppendPathSegment("_changes")
                .SetQueryParam("feed", "continuous");

            if (options != null)
            {
                request.SetChangesFeedOptions(options);
            }

            await using Stream stream = filter == null
                ? await request.GetStreamAsync(cancellationToken, HttpCompletionOption.ResponseHeadersRead)
                    .ConfigureAwait(false)
                : await request.QueryContinuousWithFilterAsync<TSource>(_settings, filter, cancellationToken)
                    .ConfigureAwait(false);

            using var reader = new StreamReader(stream);
            while (!cancellationToken.IsCancellationRequested && !reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(line))
                {
                    ChangesFeedResponseResult<TSource>? result = null;
                    try
                    {
                        result = JsonConvert.DeserializeObject<ChangesFeedResponseResult<TSource>>(line);
                    }
                    // If the token is cancelled before the full JSON is read
                    catch (JsonSerializationException) { }

                    if (result != null)
                    {
                        yield return result;
                    }
                }
            }
        }

        #endregion

        #region Utils

        /// <inheritdoc />
        public async Task<string> DownloadAttachment(CouchAttachment attachment, string localFolderPath, string? localFileName = null, int bufferSize = 4096)
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
                .DownloadFileAsync(localFolderPath, localFileName, bufferSize)
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task CompactAsync()
        {
            OperationResult result = await NewRequest()
                .AppendPathSegment("_compact")
                .PostJsonAsync(null)
                .ReceiveJson<OperationResult>()
                .SendRequestAsync()
                .ConfigureAwait(false);

            if (!result.Ok)
            {
                throw new CouchException("Something wrong happened while compacting.");
            }
        }

        /// <inheritdoc />
        public async Task<CouchDatabaseInfo> GetInfoAsync()
        {
            return await NewRequest()
                .GetJsonAsync<CouchDatabaseInfo>()
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

        #endregion

    }
}
