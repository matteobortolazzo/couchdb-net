using System.Linq.Expressions;

namespace CouchDB.Driver.ChangesFeed.Filters;

internal class SelectorChangesFeedFilter<TSource>(Expression<Func<TSource, bool>> value) : ChangesFeedFilter
    where TSource: class
{
    public Expression<Func<TSource, bool>> Value { get; } = value;
}