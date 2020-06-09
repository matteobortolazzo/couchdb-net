using CouchDB.Driver.DTOs;
using CouchDB.Driver.Exceptions;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.Helpers;
using CouchDB.Driver.Security;
using CouchDB.Driver.Settings;
using CouchDB.Driver.Types;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly QueryProvider _queryProvider;
        private readonly IFlurlClient _flurlClient;
        private readonly CouchSettings _settings;
        private readonly Uri _databaseUri;
        private readonly string _database;

        /// <inheritdoc />
        public string Database { get; }

        /// <inheritdoc />
        public ICouchSecurity Security { get; }

        internal CouchDatabase(IFlurlClient flurlClient, CouchSettings settings, Uri databaseUri, string database)
        {
            _flurlClient = flurlClient;
            _settings = settings;
            _databaseUri = databaseUri;
            _database = database;
            _queryProvider = new CouchQueryProvider(flurlClient, _settings, databaseUri, _database);

            Database = Uri.UnescapeDataString(_database);
            Security = new CouchSecurity(NewRequest);
        }

        /// <inheritdoc />
        public IQueryable<TSource> AsQueryable()
        {
            return new CouchQuery<TSource>(_queryProvider);
        }

        #region Query
        
        /// <inheritdoc />
        public List<TSource> ToList()
        {
            return AsQueryable().ToList();
        }

        /// <inheritdoc />
        public Task<List<TSource>> ToListAsync()
        {
            return AsQueryable().ToListAsync();
        }

        /// <inheritdoc />
        public CouchList<TSource> ToCouchList()
        {
            return AsQueryable().ToCouchList();
        }

        /// <inheritdoc />
        public Task<CouchList<TSource>> ToCouchListAsync()
        {
            return AsQueryable().ToCouchListAsync();
        }

        /// <inheritdoc />
        public IQueryable<TSource> Where(Expression<Func<TSource, bool>> predicate)
        {
            return AsQueryable().Where(predicate);
        }

        /// <inheritdoc />
        public IOrderedQueryable<TSource> OrderBy<TKey>(Expression<Func<TSource, TKey>> keySelector)
        {
            return AsQueryable().OrderBy(keySelector);
        }

        /// <inheritdoc />
        public IOrderedQueryable<TSource> OrderByDescending<TKey>(Expression<Func<TSource, TKey>> keySelector)
        {
            return AsQueryable().OrderByDescending(keySelector);
        }

        /// <inheritdoc />
        public IQueryable<TResult> Select<TResult>(Expression<Func<TSource, TResult>> selector)
        {
            return AsQueryable().Select(selector);
        }

        /// <inheritdoc />
        public IQueryable<TSource> Skip(int count)
        {
            return AsQueryable().Skip(count);
        }

        /// <inheritdoc />
        public IQueryable<TSource> Take(int count)
        {
            return AsQueryable().Take(count);
        }

        /// <inheritdoc />
        public IQueryable<TSource> UseBookmark(string bookmark)
        {
            return AsQueryable().UseBookmark(bookmark);
        }

        /// <inheritdoc />
        public IQueryable<TSource> WithReadQuorum(int quorum)
        {
            return AsQueryable().WithReadQuorum(quorum);
        }

        /// <inheritdoc />
        public IQueryable<TSource> WithoutIndexUpdate()
        {
            return AsQueryable().WithoutIndexUpdate();
        }

        /// <inheritdoc />
        public IQueryable<TSource> FromStable()
        {
            return AsQueryable().FromStable();
        }

        /// <inheritdoc />
        public IQueryable<TSource> UseIndex(params string[] indexes)
        {
            return AsQueryable().UseIndex(indexes);
        }

        /// <inheritdoc />
        public IQueryable<TSource> IncludeExecutionStats()
        {
            return AsQueryable().IncludeExecutionStats();
        }

        /// <inheritdoc />
        public IQueryable<TSource> IncludeConflicts()
        {
            return AsQueryable().IncludeConflicts();
        }

        #endregion

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
                var path = $"{_database}/{document.Id}/{Uri.EscapeUriString(attachment.Name)}";
                attachment.Uri = new Uri(_databaseUri, path);
            }
        }

        #endregion

        #region Writing

        /// <inheritdoc />
        public async Task<TSource> CreateAsync(TSource document, bool batch = false)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

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
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

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
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

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
            if (documents == null)
            {
                throw new ArgumentNullException(nameof(documents));
            }

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
                if (cancellationToken.IsCancellationRequested)
                {
                    continue;
                }

                var line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (!string.IsNullOrEmpty(line))
                {
                    yield return JsonConvert.DeserializeObject<ChangesFeedResponseResult<TSource>>(line);
                }
            }
        }

        #endregion

        #region Utils

        /// <inheritdoc />
        public async Task<string> DownloadAttachment(CouchAttachment attachment, string localFolderPath, string? localFileName = null, int bufferSize = 4096)
        {
            if (attachment == null)
            {
                throw new ArgumentNullException(nameof(attachment));
            }

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
                throw new CouchException("Something wrong happende while compacting.");
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

        private IFlurlRequest NewRequest()
        {
            return _flurlClient.Request(_databaseUri).AppendPathSegment(_database);
        }

        #endregion
    }
}
