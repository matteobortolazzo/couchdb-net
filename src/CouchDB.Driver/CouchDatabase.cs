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
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CouchDB.Driver
{
    /// <summary>
    /// Represents a CouchDB database.
    /// </summary>
    /// <typeparam name="TSource">The type of database documents.</typeparam>
    public class CouchDatabase<TSource> where TSource : CouchEntity
    {
        private readonly QueryProvider _queryProvider;
        private readonly FlurlClient _flurlClient;
        private readonly CouchSettings _settings;
        private readonly string _connectionString;

        /// <summary>
        /// The database name.
        /// </summary>
        public string Database { get; }

        /// <summary>
        /// Section to handle security operations.
        /// </summary>
        public CouchSecurity Security { get; }

        internal CouchDatabase(FlurlClient flurlClient, CouchSettings settings, string connectionString, string db)
        {
            _flurlClient = flurlClient ?? throw new ArgumentNullException(nameof(flurlClient));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            Database = db ?? throw new ArgumentNullException(nameof(db));
            _queryProvider = new CouchQueryProvider(flurlClient, _settings, connectionString, Database);

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

        #endregion

        #region Find

        /// <summary>
        /// Finds the document with the given ID. If no document is found, then null is returned.
        /// </summary>
        /// <param name="docId">The document ID.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the element found, or null.</returns>
        public async Task<TSource> FindAsync(string docId)
        {
            try
            {
                return await NewRequest()
                    .AppendPathSegment(docId)
                    .GetJsonAsync<TSource>()
                    .SendRequestAsync();
            }
            catch(CouchNotFoundException)
            {
                return null;
            }
        }

        #endregion

        #region Writing

        /// <summary>
        /// Creates a new document and returns it.
        /// </summary>
        /// <param name="item">The document to create.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the element created.</returns>
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
        /// <returns>A task that represents the asynchronous operation. The task result contains the element created or updated.</returns>
        public async Task<TSource> CreateOrUpdateAsync(TSource item)
        {
            if (string.IsNullOrEmpty(item.Id))
                throw new InvalidOperationException("Cannot add or update an entity without an ID.");

            var response = await NewRequest()
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
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task DeleteAsync(TSource document)
        {
            await NewRequest()
                .AppendPathSegment(document.Id)
                .SetQueryParam("rev", document.Rev)
                .DeleteAsync()
                .SendRequestAsync();
        }
        /// <summary>
        /// Creates or updates a sequence of documents based on their IDs.
        /// </summary>
        /// <param name="documents">Documents to create or update</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the elements created or updated.</returns>
        public async Task<IEnumerable<TSource>> CreateOrUpdateRangeAsync(IEnumerable<TSource> documents)
        {
            var response = await NewRequest()
                .AppendPathSegment("_bulk_docs")
                .PostJsonAsync(new { docs = documents })
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
        /// <returns>A task that represents the asynchronous operation.</returns>
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
        /// <returns>A task that represents the asynchronous operation. The task result contains the database information.</returns>
        public async Task<CouchDatabaseInfo> GetInfoAsync()
        {
            return await NewRequest()
                .GetJsonAsync<CouchDatabaseInfo>()
                .SendRequestAsync();
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
            return _flurlClient.Request(_connectionString).AppendPathSegment(Database);
        }

        #endregion
    }
}
