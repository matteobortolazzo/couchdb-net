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
using System.Threading.Tasks;

namespace CouchDB.Driver
{
    /// <summary>
    /// Represents a CouchDB database.
    /// </summary>
    /// <typeparam name="TSource">The type of database documents.</typeparam>
    public class CouchDatabase<TSource> where TSource : CouchDocument
    {
        private readonly QueryProvider _queryProvider;
        private readonly IFlurlClient _flurlClient;
        private readonly CouchSettings _settings;
        private readonly string _connectionString;
        private readonly string _database;

        /// <summary>
        /// The database name.
        /// </summary>
        public string Database { get; }

        /// <summary>
        /// Section to handle security operations.
        /// </summary>
        public CouchSecurity Security { get; }

        internal CouchDatabase(IFlurlClient flurlClient, CouchSettings settings, string connectionString, string db)
        {
            _flurlClient = flurlClient ?? throw new ArgumentNullException(nameof(flurlClient));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _database = db ?? throw new ArgumentNullException(nameof(db));
            _queryProvider = new CouchQueryProvider(flurlClient, _settings, connectionString, _database);

            Database = Uri.UnescapeDataString(_database);
            Security = new CouchSecurity(NewRequest);
        }

        /// <summary>
        /// Converts the database to an IQueryable.
        /// </summary>
        /// <returns>An IQueryable that represents the database.</returns>
        public IQueryable<TSource> AsQueryable()
        {
            return new CouchQuery<TSource>(_queryProvider);
        }

        #region Query

        /// <summary>
        /// Creates a List<T> from the database.
        /// </summary>
        /// <returns>A List<T> that contains elements from the database.</returns>
        public List<TSource> ToList()
        {
            return AsQueryable().ToList();
        }
        /// <summary>
        /// Creates a List<T> from a database by enumerating it asynchronously.
        /// </summary>
        /// <retuns>A task that represents the asynchronous operation. The task result contains a List<T> that contains elements from the database.</retuns>
        public Task<List<TSource>> ToListAsync()
        {
            return AsQueryable().ToListAsync();
        }
        /// <summary>
        /// Creates a CouchList<T> from the database.
        /// </summary>
        /// <returns>A CouchList<T> that contains elements from the database.</returns>
        public CouchList<TSource> ToCouchList()
        {
            return AsQueryable().ToCouchList();
        }
        /// <summary>
        /// Creates a CouchList<T> from a database by enumerating it asynchronously.
        /// </summary>
        /// <retuns>A task that represents the asynchronous operation. The task result contains a CouchList<T> that contains elements from the database.</retuns>
        public Task<CouchList<TSource>> ToCouchListAsync()
        {
            return AsQueryable().ToCouchListAsync();
        }
        /// <summary>
        /// Filters the database based on a predicate. Each element's index is used in the logic of the predicate function.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>An IQueryable<T> that contains elements from the database that satisfy the condition specified by predicate.</returns>
        public IQueryable<TSource> Where(Expression<Func<TSource, bool>> predicate)
        {
            return AsQueryable().Where(predicate);
        }
        /// <summary>
        /// Sorts the elements of the database in ascending order according to a key.
        /// </summary>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        /// <returns>An IOrderedQueryable<T> whose elements are sorted according to a key.</returns>
        public IOrderedQueryable<TSource> OrderBy<TKey>(Expression<Func<TSource, TKey>> keySelector)
        {
            return AsQueryable().OrderBy(keySelector);
        }
        /// <summary>
        /// Sorts the elements of the database in descending order according to a key.
        /// </summary>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        /// <returns>An IOrderedQueryable<T> whose elements are sorted according to a key.</returns>
        public IOrderedQueryable<TSource> OrderByDescending<TKey>(Expression<Func<TSource, TKey>> keySelector)
        {
            return AsQueryable().OrderByDescending(keySelector);
        }
        /// <summary>
        /// Projects each element of the database into a new form.
        /// </summary>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <returns>An IQueryable<T> whose elements are the result of invoking a projection function on each element the database.</returns>
        public IQueryable<TResult> Select<TResult>(Expression<Func<TSource, TResult>> selector)
        {
            return AsQueryable().Select(selector);
        }
        /// <summary>
        /// Bypasses a specific number of elements in the database and then returns the remaining elements.
        /// </summary>
        /// <param name="count">The number of elements to skip before returning the remaining elements.</param>
        /// <return>An IQueryable<T> that contains elements that occur after the specified index in the database.</return>
        public IQueryable<TSource> Skip(int count)
        {
            return AsQueryable().Skip(count);
        }
        /// <summary>
        /// Returns a specified number of contiguous elements from the start of the database.
        /// </summary>
        /// <param name="count">The number of elements to return.</param>
        /// <return>An IQueryable<T> that contains the specified number of elements from the start of the database.</return>
        public IQueryable<TSource> Take(int count)
        {
            return AsQueryable().Take(count);
        }
        /// <summary>
        /// Paginates elements in the database using a bookmark.
        /// </summary>
        /// <param name="bookmark">A string that enables you to specify which page of results you require.</param>
        /// <return>An IQueryable<T> that contains the paginated of elements of the database.</return>
        public IQueryable<TSource> UseBookmark(string bookmark)
        {
            return AsQueryable().UseBookmark(bookmark);
        }
        /// <summary>
        /// Ensures that elements from the database will be read from at least that many replicas.
        /// </summary>
        /// <param name="quorum">Read quorum needed for the result.</param>
        /// <return>An IQueryable<T> that contains the elements of the database after had been read from at least that many replicas.</return>
        public IQueryable<TSource> WithReadQuorum(int quorum)
        {
            return AsQueryable().WithReadQuorum(quorum);
        }
        /// <summary>
        /// Disables the index update in the database.
        /// </summary>
        /// <return>An IQueryable<T> that contains the instruction to disable index updates in the database.</return>
        public IQueryable<TSource> WithoutIndexUpdate()
        {
            return AsQueryable().WithoutIndexUpdate();
        }
        /// <summary>
        /// Ensures that elements returned is from a "stable" set of shards in the database.
        /// </summary>
        /// <return>An IQueryable<T> that contains the instruction to request elements from a "stable" set of shards in the database.</return>
        public IQueryable<TSource> FromStable()
        {
            return AsQueryable().FromStable();
        }
        /// <summary>
        /// Applies an index when requesting elements from the database.
        /// </summary>
        /// <param name="indexes">Array representing the design document and, optionally, the index name.</param>
        /// <return>An IQueryable<T> that contains the index to use when requesting elements from the database.</return>
        public IQueryable<TSource> UseIndex(params string[] indexes)
        {
            return AsQueryable().UseIndex(indexes);
        }
        /// <summary>
        /// Asks for exection stats when requesting elements from the database.
        /// </summary>
        /// <return>An IQueryable<T> that contains the request to ask for execution stats when requesting elements from the database.</return>
        public IQueryable<TSource> IncludeExecutionStats()
        {
            return AsQueryable().IncludeExecutionStats();
        }
        /// <summary>
        /// Asks for conflicts when requesting elements from the database.
        /// </summary>
        /// <return>An IQueryable<T> that contains the request to ask for conflicts when requesting elements from the database.</return>
        public IQueryable<TSource> IncludeConflicts()
        {
            return AsQueryable().IncludeConflicts();
        }

        #endregion

        #region Find

        /// <summary>
        /// Finds the document with the given ID. If no document is found, then null is returned.
        /// </summary>
        /// <param name="docId">The document ID.</param>
        /// <param name="withConflicts">Set if conflicts array should be included.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the element found, or null.</returns>
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

        /// <summary>
        /// Finds all documents matching the MangoQuery.
        /// </summary>
        /// <param name="mangoQueryJson">The JSON representing the Mango query.</param>
        /// <retuns>A task that represents the asynchronous operation. The task result contains a List<T> that contains elements from the database.</retuns>
        public Task<List<TSource>> QueryAsync(string mangoQueryJson)
        {
            return SendQueryAsync(r => r
                .WithHeader("Content-Type", "application/json")
                .PostStringAsync(mangoQueryJson));
        }

        /// <summary>
        /// Finds all documents matching the MangoQuery.
        /// </summary>
        /// <param name="mangoQuery">The object representing the Mango query.</param>
        /// <retuns>A task that represents the asynchronous operation. The task result contains a List<T> that contains elements from the database.</retuns>
        public Task<List<TSource>> QueryAsync(object mangoQuery)
        {
            return SendQueryAsync(r => r
                .PostJsonAsync(mangoQuery));
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

        /// <summary>
        /// Finds all documents with given IDs.
        /// </summary>
        /// <param name="docIds">The collection of documents IDs.</param>
        /// <returns></returns>
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
                var path = $"{_connectionString}/{_database}/{document.Id}/{Uri.EscapeUriString(attachment.Name)}";
                attachment.Uri = new Uri(path);
            }
        }

        #endregion

        #region Writing

        /// <summary>
        /// Creates a new document and returns it.
        /// </summary>
        /// <param name="document">The document to create.</param>
        /// <param name="batch">Stores document in batch mode.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the element created.</returns>
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

        /// <summary>
        /// Creates or updates the document with the given ID.
        /// </summary>
        /// <param name="document">The document to create or update</param>
        /// <param name="batch">Stores document in batch mode.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the element created or updated.</returns>
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

        /// <summary>
        /// Deletes the document with the given ID.
        /// </summary>
        /// <param name="document">The document to delete.</param>
        /// <param name="batch">Stores document in batch mode.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
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

        /// <summary>
        /// Creates or updates a sequence of documents based on their IDs.
        /// </summary>
        /// <param name="documents">Documents to create or update</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the elements created or updated.</returns>
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

        /// <summary>
        /// Commits any recent changes to the specified database to disk. You should call this if you want to ensure that recent changes have been flushed.
        /// This function is likely not required, assuming you have the recommended configuration setting of delayed_commits=false, which requires CouchDB to ensure changes are written to disk before a 200 or similar result is returned.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
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

        #region Utils

        /// <summary>
        ///  Asynchronously downloads a specific attachment.
        /// </summary>
        /// <param name="attachment">The attachment to download.</param>
        /// <param name="localFolderPath">Path of local folder where file is to be downloaded.</param>
        /// <param name="localFileName">Name of local file. If not specified, the source filename (from Content-Dispostion header, or last segment of the URL) is used.</param>
        /// <param name="bufferSize">Buffer size in bytes. Default is 4096.</param>
        /// <returns>The path of the downloaded file.</returns>
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

        /// <summary>
        /// Requests compaction of the specified database.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
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

        /// <summary>
        /// Gets information about the specified database.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the database information.</returns>
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
            return _flurlClient.Request(_connectionString).AppendPathSegment(_database);
        }

        #endregion
    }
}
