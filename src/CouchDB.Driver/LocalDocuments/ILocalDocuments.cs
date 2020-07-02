using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CouchDB.Driver.Types;

namespace CouchDB.Driver.LocalDocuments
{
    /// <summary>
    /// Perform operations on local (non-replicating) documents.
    /// </summary>
    public interface ILocalDocuments
    {
        /// <summary>
        /// Return all local documents.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains basic info about the documents.
        /// </returns>
        Task<IList<CouchDocument>> GetAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Return all documents with the supplied keys.
        /// </summary>
        /// <param name="keys">The IDs to use as filter.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains basic info about the documents.
        /// </returns>
        Task<IList<CouchDocument>> GetAsync(IReadOnlyCollection<string> keys, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the specified local document. 
        /// </summary>
        /// <typeparam name="TSource">The type of the document.</typeparam>
        /// <param name="id">The ID of the document.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the document content.
        /// </returns>
        Task<TSource> GetAsync<TSource>(string id, CancellationToken cancellationToken = default)
            where TSource: CouchDocument;

        /// <summary>
        /// Stores the specified local document. 
        /// </summary>
        /// <typeparam name="TSource">The type of the document.</typeparam>
        /// <param name="document"></param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        Task AddAsync<TSource>(TSource document, CancellationToken cancellationToken = default)
            where TSource : CouchDocument;

        /// <summary>
        /// Deletes the specified local document. 
        /// </summary>
        /// <param name="id">The ID of the document.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        Task DeleteAsync(string id, CancellationToken cancellationToken = default);
    }
}
