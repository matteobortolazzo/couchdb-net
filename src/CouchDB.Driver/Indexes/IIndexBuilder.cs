using System;
using System.Linq.Expressions;
using CouchDB.Driver.Types;

namespace CouchDB.Driver.Indexes
{
    public interface IIndexBuilder<TSource>
        where TSource : CouchDocument
    {
        IMultiFieldIndexBuilder<TSource> IndexBy<TSelector>(Expression<Func<TSource, TSelector>> selector);
    }

    public interface IMultiFieldIndexBuilder<TSource> : IIndexBuilder<TSource>
        where TSource : CouchDocument
    {
        IMultiFieldIndexBuilder<TSource> AlsoBy<TSelector>(Expression<Func<TSource, TSelector>> selector);
        IMultiFieldIndexBuilder<TSource> Where(Expression<Func<TSource, bool>> selector);
        IOrderedIndexBuilder<TSource> OrderBy<TSelector>(Expression<Func<TSource, TSelector>> selector);
        IOrderedDescendingIndexBuilder<TSource> OrderByDescending<TSelector>(Expression<Func<TSource, TSelector>> selector);
        IMultiFieldIndexBuilder<TSource> Take(int take);
        IMultiFieldIndexBuilder<TSource> Skip(int skip);
    }

    public interface IOrderedIndexBuilder<TSource> : IMultiFieldIndexBuilder<TSource>
        where TSource : CouchDocument
    {
        IOrderedIndexBuilder<TSource> ThenBy<TSelector>(Expression<Func<TSource, TSelector>> selector);
    }

    public interface IOrderedDescendingIndexBuilder<TSource> : IMultiFieldIndexBuilder<TSource>
        where TSource : CouchDocument
    {
        IOrderedDescendingIndexBuilder<TSource> ThenByDescending<TSelector>(Expression<Func<TSource, TSelector>> selector);
    }
}