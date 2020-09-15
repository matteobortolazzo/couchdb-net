using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
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

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append("{");

            // Partial Selector
            if (_partialSelector != null)
            {
                sb.Append(_partialSelector);
                sb.Append(",");
            }

            // Fields
            sb.Append("\"fields\":[");

            foreach (var field in _fields)
            {
                var fieldString = _ascending
                    ? $"\"{field}\","
                    : $"{{\"{field}\":\"desc\"}},";

                sb.Append(fieldString);
            }

            sb.Length--;
            sb.Append("]}");

            return sb.ToString();
        }
    }
}