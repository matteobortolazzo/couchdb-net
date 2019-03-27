using CouchDB.Driver.DTOs;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.Helpers;
using CouchDB.Driver.Types;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CouchDB.Driver
{
    /// <summary>
    /// Represents a CouchDB database.
    /// </summary>
    /// <typeparam name="TSource">Document type</typeparam>
    public class CouchDatabase<TSource> where TSource : CouchEntity
    {
        private readonly QueryProvider _queryProvider;
        private readonly FlurlClient _flurlClient;
        private readonly CouchSettings _settings;
        private readonly string _connectionString;
        public string Database { get; }

        internal CouchDatabase(FlurlClient flurlClient, CouchSettings settings, string connectionString, string db)
        {
            _flurlClient = flurlClient ?? throw new ArgumentNullException(nameof(flurlClient));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            Database = db ?? throw new ArgumentNullException(nameof(db));
            _queryProvider = new CouchQueryProvider(flurlClient, _settings, connectionString, Database);
        }

        /// <summary>
        /// Converts the database to an IQueryable.
        /// </summary>
        public IQueryable<TSource> AsQueryable()
        {
            return new CouchQuery<TSource>(_queryProvider);
        }
        
        #region Query

        /// <summary>
        /// Creates a List<T> from the database.
        /// </summary>
        public List<TSource> ToList()
        {
            return AsQueryable().ToList();
        }
        /// <summary>
        /// Creates a List<T> from the database asyncronsly.
        /// </summary>
        public Task<List<TSource>> ToListAsync()
        {
            return AsQueryable().ToListAsync();
        }
        /// <summary>
        /// Creates a ICouchList<T> from the database.
        /// </summary>
        public CouchList<TSource> ToCouchList()
        {
            return AsQueryable().ToCouchList();
        }
        /// <summary>
        /// Creates a ICouchList<T> from the database asyncronsly.
        /// </summary>
        public Task<CouchList<TSource>> ToCouchListAsync()
        {
            return AsQueryable().ToCouchListAsync();
        }
        /// <summary>
        /// Filters a sequence of values based on the predicate.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition.</param>
        public IQueryable<TSource> Where(Expression<Func<TSource, bool>> predicate)
        {
            return AsQueryable().Where(predicate);
        }
        /// <summary>
        /// Sorts the elements of a sequence in ascending order according to a key.
        /// </summary>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        public IOrderedQueryable<TSource> OrderBy<TKey>(Expression<Func<TSource, TKey>> keySelector)
        {
            return AsQueryable().OrderBy(keySelector);
        }
        /// <summary>
        /// Sorts the elements of a sequence in descending order according to a key.
        /// </summary>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        public IOrderedQueryable<TSource> OrderByDescending<TKey>(Expression<Func<TSource, TKey>> keySelector)
        {
            return AsQueryable().OrderByDescending(keySelector);
        }
        /// <summary>
        /// Projects each element of a sequence into a new form.
        /// </summary>
        /// <param name="selector">A projection function to apply to each element.</param>
        public IQueryable<TResult> Select<TResult>(Expression<Func<TSource, TResult>> selector)
        {
            return AsQueryable().Select(selector);
        }
        /// <summary>
        /// Bypasses a specific number of elements in a sequence and then returns the remaining elements.
        /// </summary>
        /// <param name="count">The number of elements to skip before returning the remaining elements.</param>
        public IQueryable<TSource> Skip(int count)
        {
            return AsQueryable().Skip(count);
        }
        /// <summary>
        /// Returns a specified number of contiguous elements from the start of a sequence.
        /// </summary>
        /// <param name="count">The number of elements to return.</param>
        public IQueryable<TSource> Take(int count)
        {
            return AsQueryable().Take(count);
        }
        /// <summary>
        /// Returns a sequence paginated using a bookmark.
        /// </summary>
        /// <param name="bookmark">A string that enables you to specify which page of results you require.</param>
        public IQueryable<TSource> UseBookmark(string bookmark)
        {
            return AsQueryable().UseBookmark(bookmark);
        }
        /// <summary>
        /// Returns a sequence after the element is read from at least that many replicas.
        /// </summary>
        /// <param name="quorum">Read quorum needed for the result.</param>
        public IQueryable<TSource> WithReadQuorum(int quorum)
        {
            return AsQueryable().WithReadQuorum(quorum);
        }
        /// <summary>
        /// Returns a sequence that do not update the index.
        /// </summary>
        public IQueryable<TSource> WithoutIndexUpdate()
        {
            return AsQueryable().WithoutIndexUpdate();
        }
        /// <summary>
        /// Returns a sequence returned from a "stable" set of shards.
        /// </summary>
        public IQueryable<TSource> FromStable()
        {
            return AsQueryable().FromStable();
        }
        /// <summary>
        /// Returns a sequence that use the specific index.
        /// </summary>
        /// <param name="indexes">Array representing the design document and, optionally, the index name.</param>
        public IQueryable<TSource> UseIndex(params string[] indexes)
        {
            return AsQueryable().UseIndex(indexes);
        }
        /// <summary>
        /// Retutns a sequence that includes execution statistics.
        /// </summary>
        public IQueryable<TSource> IncludeExecutionStats()
        {
            return AsQueryable().IncludeExecutionStats();
        }

        #endregion

        #region Find

        /// <summary>
        /// Returns the document with the given ID.
        /// </summary>
        /// <param name="docId">The document ID.</param>
        public async Task<TSource> FindAsync(string docId)
        {
            return await NewRequest()
                .AppendPathSegment("doc")
                .AppendPathSegment(docId)
                .GetJsonAsync<TSource>()
                .SendRequestAsync();
        }

        #endregion

        #region Writing

        /// <summary>
        /// Creates a new document and returns it.
        /// </summary>
        /// <param name="item">The document to create.</param>
        public async Task<TSource> CreateAsync(TSource item)
        {
            var response = await NewRequest()
                .PostJsonAsync(item)
                .ReceiveJson<DocumentSaveResponse>()
                .SendRequestAsync();
            return (TSource)item.ProcessSaveResponse(response);
        }
        /// <summary>
        /// Creates or updates the document with the given ID.
        /// </summary>
        /// <param name="item">The document to create or update</param>
        public async Task<TSource> CreateOrUpdateAsync(TSource item)
        {
            if (string.IsNullOrEmpty(item.Id))
                throw new InvalidOperationException("Cannot add or update an entity without an ID.");

            var response = await NewRequest()
                .AppendPathSegment("doc")
                .AppendPathSegment(item.Id)
                .PutJsonAsync(item)
                .ReceiveJson<DocumentSaveResponse>()
                .SendRequestAsync();

            return (TSource)item.ProcessSaveResponse(response);
        }
        /// <summary>
        /// Deletes the document with the given ID.
        /// </summary>
        /// <param name="document">The document to delete.</param>
        public async Task DeleteAsync(TSource document)
        {
            await NewRequest()
                .AppendPathSegment("doc")
                .AppendPathSegment(document.Id)
                .SetQueryParam("rev", document.Rev)
                .DeleteAsync()
                .SendRequestAsync();
        }
        /// <summary>
        /// Creates or updates a sequence of documents based on their IDs.
        /// </summary>
        /// <param name="documents">Documents to create or update</param>
        public async Task<IEnumerable<TSource>> CreateOrUpdateRangeAsync(IEnumerable<TSource> documents)
        {
            var response = await NewRequest()
                .AppendPathSegment("_bulk_docs")
                .PostJsonAsync(new { Docs = documents })
                .ReceiveJson<DocumentSaveResponse[]>()
                .SendRequestAsync();

            var zipped = documents.Zip(response, (doc, saveResponse) => (Document: doc, SaveResponse: saveResponse));
            foreach (var (document, saveResponse) in zipped)
                document.ProcessSaveResponse(saveResponse);
            return documents;
        }

        #endregion

        #region Utils

        /// <summary>
        /// Requests compaction of the specified database.
        /// </summary>
        public async Task CompactAsync()
        {
            await NewRequest()
                .AppendPathSegment("_compact")
                .PostJsonAsync(null)
                .SendRequestAsync();
        }
        /// <summary>
        /// Gets information about the specified database.
        /// </summary>
        public async Task<CouchDatabaseInfo> GetInfoAsync()
        {
            return await NewRequest()
                .GetJsonAsync<CouchDatabaseInfo>()
                .SendRequestAsync();
        }

        #endregion

        #region Override

        /// <summary>
        /// Returns the JSON body of the request.
        /// </summary>
        public override string ToString()
        {
            return AsQueryable().ToString();
        }

        #endregion

        #region Helper

        private IFlurlRequest NewRequest()
        {
            return _flurlClient.Request(_connectionString).AppendPathSegment(Database);
        }

        #endregion
    }
}
