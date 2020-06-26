using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CouchDB.Driver.Helpers;
using CouchDB.Driver.Types;

namespace CouchDB.Driver.Extensions
{
    public static class QueryableAsyncExtensions
    {
        /// <summary>
        /// Creates a <see cref="CouchList{TSource}"/> from a sequence by enumerating it asynchronously.
        /// </summary>
        /// <param name="source">The source of items.</param>
        /// <param name="cancellationToken">The source of items.</param>
        /// <retuns>A task that represents the asynchronous operation. The task result contains a CouchList that contains elements from the sequence.</retuns>
        public static Task<CouchList<TSource>> ToCouchListAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));
            return source.AsCouchQueryable().ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Creates a <see cref="List{TSource}"/> from a sequence by enumerating it asynchronously.
        /// </summary>
        /// <param name="source">The source of items.</param>
        /// <param name="cancellationToken">The source of items.</param>
        /// <retuns>A task that represents the asynchronous operation. The task result contains a <see cref="List{TSource}"/> that contains elements from the sequence.</retuns>
        public static async Task<List<TSource>> ToListAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));
            CouchList<TSource> couchList = await source.AsCouchQueryable().ToListAsync(cancellationToken).ConfigureAwait(false);
            return couchList.ToList();
        }

        private static CouchQueryable<TSource> AsCouchQueryable<TSource>(this IQueryable<TSource> source)
        {
            if (source is CouchQueryable<TSource> couchQuery)
            {
                return couchQuery;
            }

            throw new NotSupportedException($"Operation not supported on type: {source.GetType().Name}.");
        }
    }
}
