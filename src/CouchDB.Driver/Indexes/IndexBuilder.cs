using System.Linq.Expressions;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.Options;
using CouchDB.Driver.Query;
using CouchDB.Driver.Types;

namespace CouchDB.Driver.Indexes;

internal class IndexBuilder<TSource>(CouchOptions options, IAsyncQueryProvider queryProvider) : IIndexBuilder<TSource>,
    IOrderedIndexBuilder<TSource>, IOrderedDescendingIndexBuilder<TSource>
    where TSource: class
{
    private bool _ascending = true;
    private readonly List<string> _fields = [];
    private string? _partialSelector;

    public IOrderedIndexBuilder<TSource> IndexBy<TSelector>(Expression<Func<TSource, TSelector>> selector)
    {
        AddField(selector);
        return this;
    }

    public IOrderedDescendingIndexBuilder<TSource> IndexByDescending<TSelector>(Expression<Func<TSource, TSelector>> selector)
    {
        _ascending = false;
        AddField(selector);
        return this;
    }

    public IOrderedIndexBuilder<TSource> ThenBy<TSelector>(Expression<Func<TSource, TSelector>> selector)
    {
        return IndexBy(selector);
    }

    public IOrderedDescendingIndexBuilder<TSource> ThenByDescending<TSelector>(Expression<Func<TSource, TSelector>> selector)
    {
        return IndexByDescending(selector);
    }
        
    public void Where(Expression<Func<TSource, bool>> predicate)
    {
        MethodCallExpression whereExpression = predicate.WrapInWhereExpression();
        var jsonSelector = queryProvider.ToString(whereExpression);
        _partialSelector = jsonSelector
            .Substring(1, jsonSelector.Length - 2)
            .Replace("selector", "partial_filter_selector", StringComparison.CurrentCultureIgnoreCase);
    }

    private void AddField<TSelector>(Expression<Func<TSource, TSelector>> selector)
    {
        var memberExpression = selector.ToMemberExpression();
        _fields.Add(memberExpression.GetPropertyName(options));
    }

    public IndexDefinition Build()
    {
        var fields = _fields.ToDictionary(
            field => field,
            _ => _ascending ? IndexFieldDirection.Ascending : IndexFieldDirection.Descending);
        return new IndexDefinition(fields, _partialSelector);
    }
}