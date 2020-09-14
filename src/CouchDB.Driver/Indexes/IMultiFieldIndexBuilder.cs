using System;
using System.Linq.Expressions;
using CouchDB.Driver.Types;

namespace CouchDB.Driver.Indexes
{
    /// <summary>
    /// Builder to configure CouchDB indexes.
    /// </summary>
    /// <typeparam name="TSource">The type of the document.</typeparam>
    public interface IMultiFieldIndexBuilder<TSource> : IIndexBuilder<TSource>
        where TSource : CouchDocument
    {
        /// <summary>
        /// Select an additional field to use in the index.
        /// </summary>
        /// <typeparam name="TSelector">The type of the selected property.</typeparam>
        /// <param name="selector">Function to select a property.</param>
        /// <returns>Returns the current instance to chain calls.</returns>
        IMultiFieldIndexBuilder<TSource> AlsoBy<TSelector>(Expression<Func<TSource, TSelector>> selector);

        /// <summary>
        /// Filters the documents based on the predicate.
        /// </summary>
        /// <param name="predicate">Function to filter documents.</param>
        /// <returns>Returns the current instance to chain calls.</returns>
        IMultiFieldIndexBuilder<TSource> Where(Expression<Func<TSource, bool>> predicate);

        /// <summary>
        /// Sort the index in ascending order.
        /// </summary>
        /// <typeparam name="TSelector">The type of the selected property.</typeparam>
        /// <param name="selector">Function to select a property.</param>
        /// <returns>Returns the current instance to chain calls.</returns>
        IOrderedIndexBuilder<TSource> OrderBy<TSelector>(Expression<Func<TSource, TSelector>> selector);

        /// <summary>
        /// Sort the index in descending order.
        /// </summary>
        /// <typeparam name="TSelector">The type of the selected property.</typeparam>
        /// <param name="selector">Function to select a property.</param>
        /// <returns>Returns the current instance to chain calls.</returns>
        IOrderedDescendingIndexBuilder<TSource> OrderByDescending<TSelector>(Expression<Func<TSource, TSelector>> selector);

        /// <summary>
        /// Limits the documents to index.
        /// </summary>
        /// <param name="count">The number of document to take.</param>
        /// <returns>Returns the current instance to chain calls.</returns>
        IMultiFieldIndexBuilder<TSource> Take(int count);

        /// <summary>
        /// Bypasses a specific number of documents.
        /// </summary>
        /// <param name="count">The number of document to skip.</param>
        /// <returns>Returns the current instance to chain calls.</returns>
        IMultiFieldIndexBuilder<TSource> Skip(int count);

        /// <summary>
        /// Creates partial index which excludes documents based on the predicate at index time.
        /// </summary>
        /// <param name="predicate">Function to filter documents.</param>
        /// <returns>Returns the current instance to chain calls.</returns>
        IMultiFieldIndexBuilder<TSource> ExcludeWhere(Expression<Func<TSource, bool>> predicate);
    }
}