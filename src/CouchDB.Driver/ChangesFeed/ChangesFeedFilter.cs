using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CouchDB.Driver.ChangesFeed.Filters;
using CouchDB.Driver.Types;

namespace CouchDB.Driver.ChangesFeed
{
    /// <summary>
    /// Represent a filter for the changes feed.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1052:Static holder types should be Static or NotInheritable", Justification = "<Pending>")]
    public class ChangesFeedFilter
    {
        /// <summary>
        /// Create a filter for document IDs.
        /// </summary>
        /// <param name="documentIds">The document IDs to use as filters.</param>
        /// <returns></returns>
        public static ChangesFeedFilter DocumentIds(IList<string> documentIds)
            => new DocumentIdsChangesFeedFilter(documentIds);

        /// <summary>
        /// Create a filter using the same syntax used to query.
        /// </summary>
        /// <remarks>
        /// This is significantly more efficient than using a JavaScript filter function and is the recommended option if filtering on document attributes only.
        /// </remarks>
        /// <typeparam name="TSource">The type of database documents.</typeparam>
        /// <param name="selector">The function used to filter.</param>
        /// <returns></returns>
        public static ChangesFeedFilter Selector<TSource>(Expression<Func<TSource, bool>> selector) where TSource : CouchDocument
            => new SelectorChangesFeedFilter<TSource>(selector);

        /// <summary>
        /// Create a filter that accepts only changes for any design document within the requested database.
        /// </summary>
        /// <returns></returns>
        public static ChangesFeedFilter Design()
            => new DesignChangesFeedFilter();

        /// <summary>
        /// Create a filter that uses an existing map function to the filter. 
        /// </summary>
        /// <remarks>
        /// If the map function emits anything for the processed document it counts as accepted and the changes event emits to the feed.
        /// </remarks>
        /// <param name="view">The view.</param>
        /// <returns></returns>
        public static ChangesFeedFilter View(string view)
            => new ViewChangesFeedFilter(view);
    }
}
