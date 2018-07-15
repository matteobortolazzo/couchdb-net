using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using CouchDB.Client.Helpers;
using CouchDB.Client.Query;
using CouchDB.Client.Query.Extensions;
using CouchDB.Client.Query.Selector;
using CouchDB.Client.Query.Sort;
using Flurl.Http;
using Newtonsoft.Json;

namespace CouchDB.Client
{
    public class CouchDocuments<TSource> : IAscendingOrderedCouchQueryable<TSource>, IDescendingOrderedCouchQueryable<TSource> where TSource : CouchEntity
    {
        private readonly CouchDatabase<TSource> _db;

        public CouchDocuments(CouchDatabase<TSource> db)
        {
            _db = db;
        }

        #region Find

        public async Task<TSource> FindAsync(string id)
        {
            return await _db.NewDbRequest()
                .AppendPathSegment(id)
                .GetJsonAsync<TSource>()
                .SendAsync();
        }

        #region Results

        private class FindResult
        {
            [JsonProperty("results")]
            public List<FindResultDoc> Results { get; set; }
        }

        private class FindResultDoc
        {
            [JsonProperty("docs")]
            public List<FindResultItem> Docs { get; set; }
        }

        private class FindResultItem
        {
            [JsonProperty("ok")]
            public TSource Item { get; set; }
        }

        #endregion

        public async Task<List<TSource>> FindAsync(params string[] ids)
        {
            var result = await _db.NewDbRequest()
                .AppendPathSegment("_bulk_get")
                .PostJsonAsync(new
                {
                    docs = ids.Select(id => new { id })
                }).ReceiveJson<FindResult>()
                .SendAsync();
            return result.Results.SelectMany(r => r.Docs).Select(d => d.Item).ToList();
        }

        #endregion

        #region Where

        public bool StatsEnabled { get; private set; }
        public string LastBookmark { get; private set; }
        public ExecutionStats LastExecutionStats { get; private set; }
        
        #region Selector

        private IDictionary<string, object> _selector;
        public ICouchQueryable<TSource> Where(Expression<Func<TSource, bool>> predicate)
        {
            _selector = SelectorObjectBuilder.Serialize(predicate);
            return this;
        }

        #endregion
       
        #region Limit

        private int? _takeCount;
        public ICouchQueryable<TSource> Take(int count)
        {
            if (count <= 0) throw new ArgumentException("Cannot take a not positive number of documents.");
            _takeCount = count;
            return this;
        }

        #endregion

        #region Skip

        private int? _skipCount;
        public ICouchQueryable<TSource> Skip(int count)
        {
            if (count < 0) throw new ArgumentException("Cannot skip a negative number of documents,");
            _skipCount = count;
            return this;
        }

        #endregion
       
        #region Sort

        private List<SortProperty> _sortProperties;

        public IAscendingOrderedCouchQueryable<TSource> OrderBy<TProperty>(Expression<Func<TSource, TProperty>> keySelector)
        {
            var propName = keySelector.GetJsonPropertyName();
            _sortProperties = new List<SortProperty> { new SortProperty(propName, true) };
            return this;
        }

        public IAscendingOrderedCouchQueryable<TSource> ThenBy<TProperty>(Expression<Func<TSource, TProperty>> keySelector)
        {
            var propName = keySelector.GetJsonPropertyName();
            _sortProperties.Add(new SortProperty(propName, true));
            return this;
        }

        public IDescendingOrderedCouchQueryable<TSource> OrderByDescending<TProperty>(Expression<Func<TSource, TProperty>> keySelector)
        {
            var propName = keySelector.GetJsonPropertyName();
            _sortProperties = new List<SortProperty> { new SortProperty(propName, false) };
            return this;
        }

        public IDescendingOrderedCouchQueryable<TSource> ThenByDescending<TProperty>(Expression<Func<TSource, TProperty>> keySelector)
        {
            var propName = keySelector.GetJsonPropertyName();
            _sortProperties.Add(new SortProperty(propName, false));
            return this;
        }

        #endregion

        #region Fields

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

        #endregion

        #region R

        private int? _quorum;
        public ICouchQueryable<TSource> WithReadQuorum(int count)
        {
            if (count <= 0) throw new ArgumentException("Cannot set a not positive quorum count.");
            _quorum = count;
            return this;
        }

        #endregion

        #region Bookmark

        private string _bookmarkToUse;
        public ICouchQueryable<TSource> UseBookmark(string bookmark)
        {
            _bookmarkToUse = bookmark ?? throw new ArgumentNullException(nameof(bookmark));
            return this;
        }

        #endregion

        #region Update

        private bool? _updateIndex;
        public ICouchQueryable<TSource> UpdateIndex()
        {
            _updateIndex = true;
            return this;
        }

        #endregion

        #region Stable

        private bool? _fromStable;
        public ICouchQueryable<TSource> FromStable()
        {
            _fromStable = true;
            return this;
        }

        #endregion

        #region Stats

        public void EnableStats(bool enabled = true)
        {
            StatsEnabled = enabled;
        }

        #endregion

        #region Request

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
                findQuery.Add("sort", _sortProperties.Select(s => new Dictionary<string, string> {{s.Name, s.Direction}}));
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
            if (StatsEnabled)
                findQuery.Add("execution_stats", true);

            var jsonRequest = JsonConvert.SerializeObject(findQuery);

            var result = await _db.NewDbRequest()
                .AppendPathSegment("_find")
                .PostJsonAsync(findQuery)
                .ReceiveJson<ListResult>();

            ResetQueryParameters();
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

        private void ResetQueryParameters()
        {
            LastBookmark = null;
            LastExecutionStats = null;

            _selector = null;
            _takeCount = null;
            _skipCount = null;
            _sortProperties = null;
            _fields = null;
            _quorum = null;
            _bookmarkToUse = null;
            _updateIndex = null;
            _fromStable = null;
        }

        #endregion

        #endregion

        #region Add

        public async Task AddAsync(TSource document)
        {
            await _db.NewDbRequest()
                .AppendPathSegment(document.Id)
                .PutJsonAsync(document)
                .SendAsync();
        }

        public async Task AddRangeAsync(List<TSource> documents)
        {
            await _db.NewDbRequest()
                .AppendPathSegment("_bulk_docs")
                .PostJsonAsync(new { docs = documents })
                .SendAsync();
        }

        #endregion

        #region Update

        public async Task UpdateAsync(TSource document)
        {
            await _db.NewDbRequest()
                .AppendPathSegment(document.Id)
                .PutJsonAsync(document)
                .SendAsync();
        }

        public async Task UpdateRangeAsync(IEnumerable<TSource> documents)
        {
            await _db.NewDbRequest()
                .AppendPathSegment("_buld_docs")
                .PostJsonAsync(new { docs = documents })
                .SendAsync();
        }

        #endregion

        #region Remove

        public async Task RemoveAsync(TSource document)
        {
            await _db.NewDbRequest()
                .AppendPathSegment(document.Id)
                .SetQueryParam("rev", document.Rev)
                .DeleteAsync()
                .SendAsync();
        }

        #endregion
    }
}
