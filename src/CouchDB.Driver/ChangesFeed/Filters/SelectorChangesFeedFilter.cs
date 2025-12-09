using System;
using System.Linq.Expressions;
using CouchDB.Driver.Types;

namespace CouchDB.Driver.ChangesFeed.Filters;

internal class SelectorChangesFeedFilter<TSource>(Expression<Func<TSource, bool>> value) : ChangesFeedFilter
    where TSource : CouchDocument
{
    public Expression<Func<TSource, bool>> Value { get; } = value;
}