using System;
using System.Linq.Expressions;
using CouchDB.Driver.Types;

namespace CouchDB.Driver.Indexes
{
    /// <summary>
    /// Builder to configure CouchDB indexes.
    /// </summary>
    /// <typeparam name="TSource">The type of the document.</typeparam>
    public interface IIndexBuilder<TSource>
        where TSource : CouchDocument
    {
        /// <summary>
        /// Select a field for the index sort in ascending order.
        /// </summary>
        /// <typeparam name="TSelector">The type of the selected property.</typeparam>
        /// <param name="selector">Function to select a property.</param>
        /// <returns>Returns the current instance to chain calls.</returns>
        IOrderedIndexBuilder<TSource> IndexBy<TSelector>(Expression<Func<TSource, TSelector>> selector);

        /// <summary>
        /// Select a field for the index sort in descending order.
        /// </summary>
        /// <typeparam name="TSelector">The type of the selected property.</typeparam>
        /// <param name="selector">Function to select a property.</param>
        /// <returns>Returns the current instance to chain calls.</returns>
        IOrderedDescendingIndexBuilder<TSource> IndexByDescending<TSelector>(Expression<Func<TSource, TSelector>> selector);
    }
}