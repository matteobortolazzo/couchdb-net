using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.Options;
using CouchDB.Driver.Query;
using CouchDB.Driver.Types;

namespace CouchDB.Driver.Indexes
{
    internal class IndexBuilder<TSource>: IOrderedIndexBuilder<TSource>, IOrderedDescendingIndexBuilder<TSource>
        where TSource : CouchDocument
    {
        private readonly CouchOptions _options;
        private readonly IAsyncQueryProvider _queryProvider;

        private bool _ascending = true;
        private readonly List<string> _fields;
        private readonly List<string> _fieldsOrder;
        private int? _toTake;
        private int? _toSkip;
        private string? _selector;
        private string? _partialSelector;

        public IndexBuilder(CouchOptions options, IAsyncQueryProvider queryProvider)
        {
            _options = options;
            _queryProvider = queryProvider;
            _fields = new List<string>();
            _fieldsOrder = new List<string>();
        }

        public IMultiFieldIndexBuilder<TSource> IndexBy<TSelector>(Expression<Func<TSource, TSelector>> selector)
        {
            var m = selector.ToMemberExpression();
            _fields.Clear();
            _fields.Add(m.GetPropertyName(_options));
            return this;
        }

        public IMultiFieldIndexBuilder<TSource> Where(Expression<Func<TSource, bool>> selector)
        {
            MethodCallExpression whereExpression = selector.WrapInWhereExpression();
            var jsonSelector = _queryProvider.ToString(whereExpression);
            _selector = jsonSelector.Substring(1, jsonSelector.Length - 2);
            return this;
        }

        public IOrderedIndexBuilder<TSource> OrderBy<TSelector>(Expression<Func<TSource, TSelector>> selector)
        {
            var m = selector.ToMemberExpression();
            _ascending = true;
            _fieldsOrder.Clear();
            _fieldsOrder.Add(m.GetPropertyName(_options));
            return this;
        }

        public IOrderedDescendingIndexBuilder<TSource> OrderByDescending<TSelector>(Expression<Func<TSource, TSelector>> selector)
        {
            var m = selector.ToMemberExpression();
            _ascending = false;
            _fieldsOrder.Clear();
            _fieldsOrder.Add(m.GetPropertyName(_options));
            return this;
        }

        public IMultiFieldIndexBuilder<TSource> Take(int take)
        {
            _toTake = take;
            return this;
        }

        public IMultiFieldIndexBuilder<TSource> Skip(int skip)
        {
            _toSkip = skip;
            return this;
        }

        public IMultiFieldIndexBuilder<TSource> ExcludeWhere(Expression<Func<TSource, bool>> selector)
        {
            MethodCallExpression whereExpression = selector.WrapInWhereExpression();
            var jsonSelector = _queryProvider.ToString(whereExpression);
            _partialSelector = jsonSelector
                .Substring(1, jsonSelector.Length - 2)
                .Replace("selector", "partial_filter_selector", StringComparison.CurrentCultureIgnoreCase);
            return this;
        }

        public IMultiFieldIndexBuilder<TSource> AlsoBy<TSelector>(Expression<Func<TSource, TSelector>> selector)
        {
            var m = selector.ToMemberExpression();
            _fields.Add(m.GetPropertyName(_options));
            return this;
        }

        IOrderedIndexBuilder<TSource> IOrderedIndexBuilder<TSource>.ThenBy<TSelector>(Expression<Func<TSource, TSelector>> selector)
        {
            var m = selector.ToMemberExpression();
            _fieldsOrder.Add(m.GetPropertyName(_options));
            return this;
        }

        IOrderedDescendingIndexBuilder<TSource> IOrderedDescendingIndexBuilder<TSource>.ThenByDescending<TSelector>(Expression<Func<TSource, TSelector>> selector)
        {
            var m = selector.ToMemberExpression();
            _fieldsOrder.Add(m.GetPropertyName(_options));
            return this;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append("{");

            // Selector
            if (_selector != null)
            {
                sb.Append(_selector);
                sb.Append(",");
            }

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
                sb.Append($"\"{field}\",");
            }

            sb.Length--;
            sb.Append("],");

            // Sort
            if (_fieldsOrder.Any())
            {
                sb.Append("\"sort\":[");
                var order = _ascending ? "asc" : "desc";

                foreach (var field in _fieldsOrder)
                {
                    sb.Append($"{{\"{field}\":\"{order}\"}},");
                }

                sb.Length--;
                sb.Append("],");
            }

            // Limit 
            if (_toTake != null)
            {
                sb.Append($"\"limit\":{_toTake},");
            }

            // Skip 
            if (_toSkip != null)
            {
                sb.Append($"\"skip\":{_toSkip},");
            }

            sb.Length--;
            sb.Append("}");

            return sb.ToString();
        }
    }
}