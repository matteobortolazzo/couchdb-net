using System;

using System.Linq.Expressions;
using System.Reflection;
using CouchDB.Driver.Types;

namespace CouchDB.Driver.Query.Extensions;

public static class QueryableQueryExtensions
{
    #region Helper methods to obtain MethodInfo in a safe way

    // ReSharper disable once UnusedParameter.Local
    private static MethodInfo GetMethodInfo<T1, T2>(Func<T1, T2> f, T1 unused1)
    {
        return f.Method;
    }

    // ReSharper disable once UnusedParameter.Local
    private static MethodInfo GetMethodInfo<T1, T2, T3>(Func<T1, T2, T3> f, T1 unused1, T2 unused2)
    {
        return f.Method;
    }

    #endregion

    /// <param name="source">The source of items.</param>
    /// <typeparam name="TSource">Type of source.</typeparam>
    extension<TSource>(IQueryable<TSource> source)
    {
        /// <summary>
        /// Paginates elements in the sequence using a bookmark.
        /// </summary>
        /// <param name="bookmark">A string that enables you to specify which page of results you require.</param>
        /// <return>An <see cref="IQueryable{TSource}"/> that contains the paginated of elements of the sequence.</return>
        public IQueryable<TSource> UseBookmark(string bookmark)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(bookmark);

            return source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(UseBookmark, source, bookmark),
                    [source.Expression, Expression.Constant(bookmark)]));
        }

        /// <summary>
        /// Ensures that elements from the sequence will be read from at least that many replicas.
        /// </summary>
        /// <param name="quorum">Read quorum needed for the result.</param>
        /// <return>An <see cref="IQueryable{TSource}"/> that contains the elements of the sequence after had been read from at least that many replicas.</return>
        public IQueryable<TSource> WithReadQuorum(int quorum)
        {
            ArgumentNullException.ThrowIfNull(source);
            if (quorum < 1)
            {
                throw new ArgumentException("Read quorum cannot be less than 1.", nameof(quorum));
            }

            return source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(WithReadQuorum, source, quorum),
                    [source.Expression, Expression.Constant(quorum)]));
        }

        /// <summary>
        /// Disables the index update in the sequence.
        /// </summary>
        /// <return>An <see cref="IQueryable{TSource}"/> that contains the instruction to disable index updates in the sequence.</return>
        public IQueryable<TSource> WithoutIndexUpdate()
        {
            ArgumentNullException.ThrowIfNull(source);

            return source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(WithoutIndexUpdate, source), source.Expression));
        }

        /// <summary>
        /// Ensures that elements returned is from a "stable" set of shards in the sequence.
        /// </summary>
        /// <return>An <see cref="IQueryable{TSource}"/> that contains the instruction to request elements from a "stable" set of shards in the sequence.</return>
        public IQueryable<TSource> FromStable()
        {
            ArgumentNullException.ThrowIfNull(source);

            return source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(FromStable, source), source.Expression));
        }

        /// <summary>
        /// Applies an index when requesting elements from the sequence.
        /// </summary>
        /// <param name="indexes">Array representing the design document and, optionally, the index name.</param>
        /// <return>An <see cref="IQueryable{TSource}"/> that contains the index to use when requesting elements from the sequence.</return>
        public IQueryable<TSource> UseIndex(params string[] indexes)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(indexes);

            if (indexes.Length != 1 && indexes.Length != 2)
            {
                throw new ArgumentException(
                    "Only 1 or 2 parameters are allowed. \"<design_document>\" or [\"<design_document>\",\"<index_name>\"]",
                    nameof(indexes));
            }

            return source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(UseIndex, source, indexes),
                    [source.Expression, Expression.Constant(indexes)]));
        }

        /// <summary>
        /// Asks for execution stats when requesting elements from the sequence.
        /// </summary>
        /// <return>An <see cref="IQueryable{TSource}"/> that contains the request to ask for execution stats when requesting elements from the sequence.</return>
        public IQueryable<TSource> IncludeExecutionStats()
        {
            ArgumentNullException.ThrowIfNull(source);

            return source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(IncludeExecutionStats, source), source.Expression));
        }

        /// <summary>
        /// Asks for conflicts when requesting elements from the sequence.
        /// </summary>
        /// <return>An <see cref="IQueryable{TSource}"/> that contains the request to ask for conflicts when requesting elements from the sequence.</return>
        public IQueryable<TSource> IncludeConflicts()
        {
            ArgumentNullException.ThrowIfNull(source);

            return source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(IncludeConflicts, source), source.Expression));
        }

        /// <summary>
        /// Select specific fields to return in the result.
        /// </summary>
        /// <param name="selectFunctions">List of functions to select fields.</param>
        /// <return>An <see cref="IQueryable{TSource}"/> that contains the request specific fields when requesting elements from the sequence.</return>
        public IQueryable<TSource> Select(params Expression<Func<TSource, object>>[] selectFunctions)
        {
            ArgumentNullException.ThrowIfNull(source);

            foreach (Expression<Func<TSource, object>> selectFunction in selectFunctions)
            {
                ArgumentNullException.ThrowIfNull(selectFunction, nameof(selectFunctions));
            }

            return source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(Select, source, selectFunctions),
                    [source.Expression, Expression.Constant(selectFunctions)]));
        }

        /// <summary>
        /// Select only the field defined in <see cref="TResult"/>
        /// </summary>
        /// <typeparam name="TResult">Type of output list.</typeparam>
        /// <return>An <see cref="IQueryable{TResult}"/> that contains the request specific fields when requesting elements from the sequence.</return>
        public IQueryable<TResult> Convert<TResult>()
            where TResult : CouchDocument
        {
            ArgumentNullException.ThrowIfNull(source);

            return source.Provider.CreateQuery<TResult>(
                Expression.Call(
                    null,
                    GetMethodInfo(Convert<TSource, TResult>, source), source.Expression));
        }
    }
}