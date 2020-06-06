using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CouchDB.Driver.DTOs;
using CouchDB.Driver.Security;
using CouchDB.Driver.Types;

namespace CouchDB.Driver
{
    public interface ICouchDatabase<TSource> where TSource : CouchDocument
    {
        /// <summary>
        /// Converts the database to an IQueryable.
        /// </summary>
        /// <returns>An IQueryable that represents the database.</returns>
        IQueryable<TSource> AsQueryable();

        /// <summary>
        /// Creates a <see cref="List{TSource}"/> from the database.
        /// </summary>
        /// <returns>A <see cref="List{TSource}"/> that contains elements from the database.</returns>
        List<TSource> ToList();

        /// <summary>
        /// Creates a <see cref="List{TSource}"/> from a database by enumerating it asynchronously.
        /// </summary>
        /// <retuns>A task that represents the asynchronous operation. The task result contains a <see cref="List{TSource}"/>  that contains elements from the database.</retuns>
        Task<List<TSource>> ToListAsync();

        /// <summary>
        /// Creates a <see cref="CouchList{TSource}"/> from the database.
        /// </summary>
        /// <returns>A <see cref="CouchList{TSource}"/> that contains elements from the database.</returns>
        CouchList<TSource> ToCouchList();

        /// <summary>
        /// Creates a <see cref="CouchList{TSource}"/>  from a database by enumerating it asynchronously.
        /// </summary>
        /// <retuns>A task that represents the asynchronous operation. The task result contains a <see cref="CouchList{TSource}"/>  that contains elements from the database.</retuns>
        Task<CouchList<TSource>> ToCouchListAsync();

        /// <summary>
        /// Filters the database based on a predicate. Each element's index is used in the logic of the predicate function.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>An <see cref="IQueryable{TSource}"/> that contains elements from the database that satisfy the condition specified by predicate.</returns>
        IQueryable<TSource> Where(Expression<Func<TSource, bool>> predicate);

        /// <summary>
        /// Sorts the elements of the database in ascending order according to a key.
        /// </summary>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        /// <returns>An <see cref="IQueryable{TSource}"/> whose elements are sorted according to a key.</returns>
        IOrderedQueryable<TSource> OrderBy<TKey>(Expression<Func<TSource, TKey>> keySelector);

        /// <summary>
        /// Sorts the elements of the database in descending order according to a key.
        /// </summary>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        /// <returns>An <see cref="IOrderedQueryable{TSource}"/> whose elements are sorted according to a key.</returns>
        IOrderedQueryable<TSource> OrderByDescending<TKey>(Expression<Func<TSource, TKey>> keySelector);

#pragma warning disable CA1716 // Identifiers should not match keywords
        /// <summary>
        /// Projects each element of the database into a new form.
        /// </summary>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <returns>An <see cref="IQueryable{TSource}"/> whose elements are the result of invoking a projection function on each element the database.</returns>
        IQueryable<TResult> Select<TResult>(Expression<Func<TSource, TResult>> selector);
#pragma warning restore CA1716 // Identifiers should not match keywords

        /// <summary>
        /// Bypasses a specific number of elements in the database and then returns the remaining elements.
        /// </summary>
        /// <param name="count">The number of elements to skip before returning the remaining elements.</param>
        /// <return>An <see cref="IQueryable{TSource}"/> that contains elements that occur after the specified index in the database.</return>
        IQueryable<TSource> Skip(int count);

        /// <summary>
        /// Returns a specified number of contiguous elements from the start of the database.
        /// </summary>
        /// <param name="count">The number of elements to return.</param>
        /// <return>An <see cref="IQueryable{TSource}"/> that contains the specified number of elements from the start of the database.</return>
        IQueryable<TSource> Take(int count);

        /// <summary>
        /// Paginates elements in the database using a bookmark.
        /// </summary>
        /// <param name="bookmark">A string that enables you to specify which page of results you require.</param>
        /// <return>An <see cref="IQueryable{TSource}"/> that contains the paginated of elements of the database.</return>
        IQueryable<TSource> UseBookmark(string bookmark);

        /// <summary>
        /// Ensures that elements from the database will be read from at least that many replicas.
        /// </summary>
        /// <param name="quorum">Read quorum needed for the result.</param>
        /// <return>An <see cref="IQueryable{TSource}"/> that contains the elements of the database after had been read from at least that many replicas.</return>
        IQueryable<TSource> WithReadQuorum(int quorum);

        /// <summary>
        /// Disables the index update in the database.
        /// </summary>
        /// <return>An <see cref="IQueryable{TSource}"/> that contains the instruction to disable index updates in the database.</return>
        IQueryable<TSource> WithoutIndexUpdate();

        /// <summary>
        /// Ensures that elements returned is from a "stable" set of shards in the database.
        /// </summary>
        /// <return>An <see cref="IQueryable{TSource}"/> that contains the instruction to request elements from a "stable" set of shards in the database.</return>
        IQueryable<TSource> FromStable();

        /// <summary>
        /// Applies an index when requesting elements from the database.
        /// </summary>
        /// <param name="indexes">Array representing the design document and, optionally, the index name.</param>
        /// <return>An <see cref="IQueryable{TSource}"/> that contains the index to use when requesting elements from the database.</return>
        IQueryable<TSource> UseIndex(params string[] indexes);

        /// <summary>
        /// Asks for execution stats when requesting elements from the database.
        /// </summary>
        /// <return>An <see cref="IQueryable{TSource}"/> that contains the request to ask for execution stats when requesting elements from the database.</return>
        IQueryable<TSource> IncludeExecutionStats();

        /// <summary>
        /// Asks for conflicts when requesting elements from the database.
        /// </summary>
        /// <return>An <see cref="IQueryable{TSource}"/> that contains the request to ask for conflicts when requesting elements from the database.</return>
        IQueryable<TSource> IncludeConflicts();

        /// <summary>
        /// Finds the document with the given ID. If no document is found, then null is returned.
        /// </summary>
        /// <param name="docId">The document ID.</param>
        /// <param name="withConflicts">Set if conflicts array should be included.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the element found, or null.</returns>
        Task<TSource?> FindAsync(string docId, bool withConflicts = false);

        /// <summary>
        /// Finds all documents matching the MangoQuery.
        /// </summary>
        /// <param name="mangoQueryJson">The JSON representing the Mango query.</param>
        /// <retuns>A task that represents the asynchronous operation. The task result contains a <see cref="List{TSource}"/> that contains elements from the database.</retuns>
        Task<List<TSource>> QueryAsync(string mangoQueryJson);

        /// <summary>
        /// Finds all documents matching the MangoQuery.
        /// </summary>
        /// <param name="mangoQuery">The object representing the Mango query.</param>
        /// <retuns>A task that represents the asynchronous operation. The task result contains a <see cref="List{TSource}"/> that contains elements from the database.</retuns>
        Task<List<TSource>> QueryAsync(object mangoQuery);

        /// <summary>
        /// Finds all documents with given IDs.
        /// </summary>
        /// <param name="docIds">The collection of documents IDs.</param>
        /// <returns></returns>
        Task<List<TSource>> FindManyAsync(IEnumerable<string> docIds);

        /// <summary>
        /// Creates a new document and returns it.
        /// </summary>
        /// <param name="document">The document to create.</param>
        /// <param name="batch">Stores document in batch mode.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the element created.</returns>
        Task<TSource> CreateAsync(TSource document, bool batch = false);

        /// <summary>
        /// Creates or updates the document with the given ID.
        /// </summary>
        /// <param name="document">The document to create or update</param>
        /// <param name="batch">Stores document in batch mode.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the element created or updated.</returns>
        Task<TSource> CreateOrUpdateAsync(TSource document, bool batch = false);

        /// <summary>
        /// Deletes the document with the given ID.
        /// </summary>
        /// <param name="document">The document to delete.</param>
        /// <param name="batch">Stores document in batch mode.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DeleteAsync(TSource document, bool batch = false);

        /// <summary>
        /// Creates or updates a sequence of documents based on their IDs.
        /// </summary>
        /// <param name="documents">Documents to create or update</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the elements created or updated.</returns>
        Task<IEnumerable<TSource>> CreateOrUpdateRangeAsync(IList<TSource> documents);

        /// <summary>
        /// Commits any recent changes to the specified database to disk. You should call this if you want to ensure that recent changes have been flushed.
        /// This function is likely not required, assuming you have the recommended configuration setting of delayed_commits=false, which requires CouchDB to ensure changes are written to disk before a 200 or similar result is returned.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task EnsureFullCommitAsync();

        /// <summary>
        /// Returns a sorted list of changes made to documents in the database.
        /// </summary>
        /// <remarks>
        /// Only the most recent change for a given document is guaranteed to be provided.
        /// </remarks>
        /// <param name="options">Options to apply to the request.</param>
        /// <param name="filter">A filter to apply to the result.</param>
        /// <returns></returns>
        Task<ChangesFeedResponse<TSource>> GetChangesAsync(ChangesFeedOptions? options = null,
            ChangesFeedFilter? filter = null);

        /// <summary>
        /// Returns changes as they happen. A continuous feed stays open and connected to the database until explicitly closed.
        /// </summary>
        /// <remarks>
        /// To stop receiving changes call <c>Cancel()</c> on the <c>CancellationTokenSource</c> used to create the <c>CancellationToken</c>.
        /// </remarks>
        /// <param name="options">Options to apply to the request.</param>
        /// <param name="filter">A filter to apply to the result.</param>
        /// <param name="cancellationToken">A cancellation token to stop receiving changes.</param>
        /// <returns></returns>
        IAsyncEnumerable<ChangesFeedResponseResult<TSource>> GetContinuousChangesAsync(
            ChangesFeedOptions options, ChangesFeedFilter filter,
            CancellationToken cancellationToken);

        /// <summary>
        ///  Asynchronously downloads a specific attachment.
        /// </summary>
        /// <param name="attachment">The attachment to download.</param>
        /// <param name="localFolderPath">Path of local folder where file is to be downloaded.</param>
        /// <param name="localFileName">Name of local file. If not specified, the source filename (from Content-Dispostion header, or last segment of the URL) is used.</param>
        /// <param name="bufferSize">Buffer size in bytes. Default is 4096.</param>
        /// <returns>The path of the downloaded file.</returns>
        Task<string> DownloadAttachment(CouchAttachment attachment, string localFolderPath,
            string? localFileName = null, int bufferSize = 4096);

        /// <summary>
        /// Requests compaction of the specified database.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task CompactAsync();

        /// <summary>
        /// Gets information about the specified database.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the database information.</returns>
        Task<CouchDatabaseInfo> GetInfoAsync();

        /// <summary>
        /// The database name.
        /// </summary>
        string Database { get; }
        
        /// <summary>
        /// Section to handle security operations.
        /// </summary>
        public ICouchSecurity Security { get; }
    }
}