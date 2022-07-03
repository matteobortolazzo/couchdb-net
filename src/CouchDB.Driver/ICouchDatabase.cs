using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CouchDB.Driver.ChangesFeed;
using CouchDB.Driver.ChangesFeed.Responses;
using CouchDB.Driver.Indexes;
using CouchDB.Driver.Local;
using CouchDB.Driver.Security;
using CouchDB.Driver.Types;
using CouchDB.Driver.Views;
using Flurl.Http;

namespace CouchDB.Driver
{
    /// <summary>
    /// Represent a database.
    /// </summary>
    /// <typeparam name="TSource">The type of the document.</typeparam>
    public interface ICouchDatabase<TSource>: IOrderedQueryable<TSource>
        where TSource : CouchDocument
    {
        /// <summary>
        /// Finds the document with the given ID. If no document is found, then null is returned.
        /// </summary>
        /// <param name="docId">The document ID.</param>
        /// <param name="withConflicts">Set if conflicts array should be included.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the element found, or null.</returns>
        Task<TSource?> FindAsync(string docId, bool withConflicts = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds all documents matching the MangoQuery.
        /// </summary>
        /// <param name="mangoQueryJson">The JSON representing the Mango query.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <retuns>A task that represents the asynchronous operation. The task result contains a <see cref="List{TSource}"/> that contains elements from the database.</retuns>
        Task<List<TSource>> QueryAsync(string mangoQueryJson, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds all documents matching the MangoQuery.
        /// </summary>
        /// <param name="mangoQuery">The object representing the Mango query.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <retuns>A task that represents the asynchronous operation. The task result contains a <see cref="List{TSource}"/> that contains elements from the database.</retuns>
        Task<List<TSource>> QueryAsync(object mangoQuery, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds all documents with given IDs.
        /// </summary>
        /// <param name="docIds">The collection of documents IDs.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <retuns>A task that represents the asynchronous operation. The task result contains a <see cref="List{TSource}"/> that contains elements from the database.</retuns>
        Task<List<TSource>> FindManyAsync(IReadOnlyCollection<string> docIds, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new document and returns it.
        /// </summary>
        /// <param name="document">The document to create.</param>
        /// <param name="batch">Stores document in batch mode.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the element created.</returns>
        Task<TSource> AddAsync(TSource document, bool batch = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates or updates the document with the given ID.
        /// </summary>
        /// <param name="document">The document to create or update</param>
        /// <param name="batch">Stores document in batch mode.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the element created or updated.</returns>
        Task<TSource> AddOrUpdateAsync(TSource document, bool batch = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes the document with the given ID.
        /// </summary>
        /// <param name="document">The document to delete.</param>
        /// <param name="batch">Stores document in batch mode.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task RemoveAsync(TSource document, bool batch = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates or updates a sequence of documents based on their IDs.
        /// </summary>
        /// <param name="documents">Documents to create or update</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the elements created or updated.</returns>
        Task<IEnumerable<TSource>> AddOrUpdateRangeAsync(IList<TSource> documents, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the specified view function from the specified design document.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="design">The design to use.</param>
        /// <param name="view">The view to use.</param>
        /// <param name="options">Options for the request.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="CouchView{TKey, TValue, TSource}"/>.</returns>
        Task<List<CouchView<TKey, TValue, TSource>>> GetViewAsync<TKey, TValue>(string design, string view,
            CouchViewOptions<TKey>? options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the specified view function from the specified design document.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="design">The design to use.</param>
        /// <param name="view">The view to use.</param>
        /// <param name="options">Options for the request.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="CouchViewList{TKey, TSource, TView}"/>.</returns>
        Task<CouchViewList<TKey, TValue, TSource>> GetDetailedViewAsync<TKey, TValue>(string design, string view,
            CouchViewOptions<TKey>? options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the specified view function from the specified design document using
        /// the queries endpoint. This returns one result for each query option in the provided sequence.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="design">The design to use.</param>
        /// <param name="view">The view to use.</param>
        /// <param name="queries">Multiple query options for the request.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an array with a list of <see cref="CouchView{TKey, TValue, TSource}"/> for each query.</returns>
        Task<List<CouchView<TKey, TValue, TSource>>[]> GetViewQueryAsync<TKey, TValue>(string design, string view,
             IList<CouchViewOptions<TKey>> queries, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Executes the specified view function from the specified design document using
        /// the queries endpoint. This returns one result for each query option in the provided sequence.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="design">The design to use.</param>
        /// <param name="view">The view to use.</param>
        /// <param name="queries">Multiple query options for the request.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list with a <see cref="CouchViewList{TKey, TSource, TView}"/> for each query.</returns>
        Task<CouchViewList<TKey, TValue, TSource>[]> GetDetailedViewQueryAsync<TKey, TValue>(string design, string view,
             IList<CouchViewOptions<TKey>> queries, CancellationToken cancellationToken = default);

        /// <summary>
        /// Since CouchDB v3, it is deprecated (a no-op).
        /// 
        /// Commits any recent changes to the specified database to disk. You should call this if you want to ensure that recent changes have been flushed.
        /// This function is likely not required, assuming you have the recommended configuration setting of delayed_commits=false, which requires CouchDB to ensure changes are written to disk before a 200 or similar result is returned.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task EnsureFullCommitAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a sorted list of changes made to documents in the database.
        /// </summary>
        /// <remarks>
        /// Only the most recent change for a given document is guaranteed to be provided.
        /// </remarks>
        /// <param name="options">Options to apply to the request.</param>
        /// <param name="filter">A filter to apply to the result.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the feed change.</returns>
        Task<ChangesFeedResponse<TSource>> GetChangesAsync(ChangesFeedOptions? options = null,
            ChangesFeedFilter? filter = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns changes as they happen. A continuous feed stays open and connected to the database until explicitly closed.
        /// </summary>
        /// <remarks>
        /// To stop receiving changes call <c>Cancel()</c> on the <c>CancellationTokenSource</c> used to create the <c>CancellationToken</c>.
        /// </remarks>
        /// <param name="options">Options to apply to the request.</param>
        /// <param name="filter">A filter to apply to the result.</param>
        /// <param name="cancellationToken">A cancellation token to stop receiving changes.</param>
        /// <returns>A IAsyncEnumerable that represents the asynchronous operation. The task result contains the feed change.</returns>
        IAsyncEnumerable<ChangesFeedResponseResult<TSource>> GetContinuousChangesAsync(
            ChangesFeedOptions options, ChangesFeedFilter filter,
            CancellationToken cancellationToken);

        /// <summary>
        /// Gets the list of indexes in the database.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A Task that represents the asynchronous operation. The task result contains the list of indexes.</returns>
        Task<List<IndexInfo>> GetIndexesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates an index for the current database with the given configuration.
        /// </summary>
        /// <param name="name">The name of the index.</param>
        /// <param name="indexBuilderAction">The action to configure the index builder.</param>
        /// <param name="options">The index options.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the ID of the design document.</returns>
        Task<string> CreateIndexAsync(string name, Action<IIndexBuilder<TSource>> indexBuilderAction,
            IndexOptions? options = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a specific index.
        /// </summary>
        /// <param name="designDocument">The design document name.</param>
        /// <param name="name">The index name.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DeleteIndexAsync(string designDocument, string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a specific index.
        /// </summary>
        /// <param name="indexInfo">The index info.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DeleteIndexAsync(IndexInfo indexInfo, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously downloads a specific attachment.
        /// </summary>
        /// <param name="attachment">The attachment to download.</param>
        /// <param name="localFolderPath">Path of local folder where file is to be downloaded.</param>
        /// <param name="localFileName">Name of local file. If not specified, the source filename (from Content-Dispostion header, or last segment of the URL) is used.</param>
        /// <param name="bufferSize">Buffer size in bytes. Default is 4096.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the path of the download file.</returns>
        Task<string> DownloadAttachmentAsync(CouchAttachment attachment, string localFolderPath,
            string? localFileName = null, int bufferSize = 4096, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously downloads a specific attachment as stream.
        /// </summary>
        /// <param name="attachment">The attachment to download.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains of the file stream.</returns>
        Task<Stream> DownloadAttachmentAsStreamAsync(CouchAttachment attachment, CancellationToken cancellationToken = default);

        /// <summary>
        /// Requests compaction of the specified database.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task CompactAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets information about the specified database.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the database information.</returns>
        Task<CouchDatabaseInfo> GetInfoAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get an empty request that targets the current database.
        /// </summary>
        /// <returns>A Flurl request.</returns>
        IFlurlRequest NewRequest();

        /// <summary>
        /// The database name.
        /// </summary>
        string Database { get; }
        
        /// <summary>
        /// Section to handle security operations.
        /// </summary>
        public ICouchSecurity Security { get; }

        /// <summary>
        /// Access local documents operations.
        /// </summary>
        public ILocalDocuments LocalDocuments { get; }
    }
}