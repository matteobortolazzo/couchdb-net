using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.Options;
using CouchDB.Driver.Query;
using CouchDB.Driver.Types;

namespace CouchDB.Driver.Indexes
{
    internal class IndexBuilder<TSource>: IIndexBuilder<TSource>, IOrderedIndexBuilder<TSource>, IOrderedDescendingIndexBuilder<TSource>
        where TSource : CouchDocument
    {
        private readonly CouchOptions _options;
        private readonly IAsyncQueryProvider _queryProvider;

        private bool _ascending = true;
        private readonly List<string> _fields;
        private string? _partialSelector;

        public IndexBuilder(CouchOptions options, IAsyncQueryProvider queryProvider)
        {
            _options = options;
            _queryProvider = queryProvider;
            _fields = new List<string>();
        }

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
            var jsonSelector = _queryProvider.ToString(whereExpression);
            _partialSelector = jsonSelector
                .Substring(1, jsonSelector.Length - 2)
                .Replace("selector", "partial_filter_selector", StringComparison.CurrentCultureIgnoreCase);
        }

        private void AddField<TSelector>(Expression<Func<TSource, TSelector>> selector)
        {
            var memberExpression = selector.ToMemberExpression();
            _fields.Add(memberExpression.GetPropertyName(_options));
        }

        public IndexDefinition Build()
        {
            var fields = _fields.ToDictionary(
                field => field,
                _ => _ascending ? IndexFieldDirection.Ascending : IndexFieldDirection.Descending);
            return new IndexDefinition(fields, _partialSelector);
        }
    }
}