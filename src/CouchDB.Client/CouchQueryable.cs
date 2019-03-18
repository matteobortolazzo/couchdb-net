using CouchDB.Client.Query;
using CouchDB.Client.Query.Extensions;
using CouchDB.Client.Query.Selector;
using CouchDB.Client.Query.Sort;
using Flurl.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CouchDB.Client
{
    public class CouchQueryable<TSource> : IAscendingOrderedCouchQueryable<TSource>, IDescendingOrderedCouchQueryable<TSource> where TSource : CouchEntity
    {
        private readonly CouchDatabase<TSource> _db;
        public CouchQueryable(CouchDatabase<TSource> db)
        {
            _db = db;
        }

        public CouchQueryable(CouchDatabase<TSource> db, Expression<Func<TSource, bool>> predicate) : this(db)
        {
            Where(predicate);
        }

        private IDictionary<string, object> _selector;
        private ICouchQueryable<TSource> Where(Expression<Func<TSource, bool>> predicate)
        {
            // Can only use 1 where, so keep it private
            _selector = SelectorObjectBuilder.Serialize(predicate);
            return this;
        }

        private List<SortProperty> _sortProperties;
        public IAscendingOrderedCouchQueryable<TSource> OrderBy<TProperty>(Expression<Func<TSource, TProperty>> keySelector)
        {
            var propName = keySelector.GetJsonPropertyName();
            _sortProperties = new List<SortProperty> { new SortProperty(propName, true) };
            return this;
        }

        public IDescendingOrderedCouchQueryable<TSource> OrderByDescending<TProperty>(Expression<Func<TSource, TProperty>> keySelector)
        {
            var propName = keySelector.GetJsonPropertyName();
            _sortProperties = new List<SortProperty> { new SortProperty(propName, false) };
            return this;
        }

        public IAscendingOrderedCouchQueryable<TSource> ThenBy<TProperty>(Expression<Func<TSource, TProperty>> keySelector)
        {
            var propName = keySelector.GetJsonPropertyName();
            _sortProperties.Add(new SortProperty(propName, true));
            return this;
        }

        public IDescendingOrderedCouchQueryable<TSource> ThenByDescending<TProperty>(Expression<Func<TSource, TProperty>> keySelector)
        {
            var propName = keySelector.GetJsonPropertyName();
            _sortProperties.Add(new SortProperty(propName, false));
            return this;
        }

        private List<string> _fields;
        public ICouchQueryable<TSource> Select(params Expression<Func<TSource, object>>[] fieldSelectors)
        {
            _fields = new List<string>();
            foreach (var fieldSelector in fieldSelectors)
            {
                var property = fieldSelector.GetJsonPropertyName();
                _fields.Add(property);
            }
            return this;
        }

        private int? _skipCount;
        public ICouchQueryable<TSource> Skip(int count)
        {
            if (count < 0) throw new ArgumentException("Cannot skip a negative number of documents,");
            _skipCount = count;
            return this;
        }

        private int? _takeCount;
        public ICouchQueryable<TSource> Take(int count)
        {
            if (count <= 0) throw new ArgumentException("Cannot take a not positive number of documents.");
            _takeCount = count;
            return this;
        }

        private bool? _updateIndex;
        public ICouchQueryable<TSource> UpdateIndex()
        {
            _updateIndex = true;
            return this;
        }

        private bool? _fromStable;
        public ICouchQueryable<TSource> FromStable()
        {
            _fromStable = true;
            return this;
        }

        private bool _statsEnabled;
        public ICouchQueryable<TSource> WithExecutionStats()
        {
            _statsEnabled = true;
            return this;
        }

        private string _bookmarkToUse;
        public ICouchQueryable<TSource> UseBookmark(string bookmark)
        {
            _bookmarkToUse = bookmark ?? throw new ArgumentNullException(nameof(bookmark));
            return this;
        }

        private int? _quorum;

        public object LastBookmark { get; private set; }
        public object LastExecutionStats { get; private set; }

        public ICouchQueryable<TSource> WithReadQuorum(int count)
        {
            if (count <= 0) throw new ArgumentException("Cannot set a not positive quorum count.");
            _quorum = count;
            return this;
        }

        public async Task<List<TSource>> ToListAsync()
        {
            var findQuery = new Dictionary<string, object>();

            if (_selector == null)
            {
                var greaterThanNull = new ExpandoObject();
                greaterThanNull.AddProperty("$gt", null);
                var emptySelector = new ExpandoObject();
                emptySelector.AddProperty("_id", greaterThanNull);
                _selector = emptySelector;
            }

            findQuery.Add("selector", _selector);

            if (_takeCount.HasValue)
                findQuery.Add("limit", _takeCount.Value);
            if (_skipCount.HasValue)
                findQuery.Add("skip", _skipCount.Value);
            if (_sortProperties != null)
                findQuery.Add("sort", _sortProperties.Select(s => new Dictionary<string, string> { { s.Name, s.Direction } }));
            if (_fields != null)
                findQuery.Add("fields", _fields);
            if (_quorum.HasValue)
                findQuery.Add("r", _quorum.Value);
            if (_bookmarkToUse != null)
                findQuery.Add("bookmark", _bookmarkToUse);
            if (_updateIndex.HasValue)
                findQuery.Add("update", true);
            if (_fromStable.HasValue)
                findQuery.Add("stable", true);
            if (_statsEnabled)
                findQuery.Add("execution_stats", true);

            var jsonRequest = JsonConvert.SerializeObject(findQuery);

            var result = await _db.NewDbRequest()
                .AppendPathSegment("_find")
                .PostJsonAsync(findQuery)
                .ReceiveJson<ListResult>();

            LastBookmark = result.Bookmark;
            LastExecutionStats = result.ExecutionStats;

            return result.Docs;
        }

        private class ListResult
        {
            [JsonProperty("docs")]
            public List<TSource> Docs { get; set; }
            [JsonProperty("bookmark")]
            public string Bookmark { get; set; }
            [JsonProperty("execution_stats")]
            public ExecutionStats ExecutionStats { get; set; }
        }
    }
}
