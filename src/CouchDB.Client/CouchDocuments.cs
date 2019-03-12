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
using CouchDB.Client.Responses;
using Flurl.Http;
using Newtonsoft.Json;

namespace CouchDB.Client
{
    public class CouchDocuments<TSource> : ICouchQueryable<TSource> where TSource : CouchEntity
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

        #endregion

        #region Selector

        public ICouchQueryable<TSource> Where(Expression<Func<TSource, bool>> predicate)
        {
            return new CouchQueryable<TSource>(_db, predicate);
        }

        #endregion

        #region Limit

        public ICouchQueryable<TSource> Take(int count)
        {
            return new CouchQueryable<TSource>(_db).Take(count);
        }

        #endregion

        #region Skip

        public ICouchQueryable<TSource> Skip(int count)
        {
            return new CouchQueryable<TSource>(_db).Skip(count);
        }

        #endregion

        #region Sort

        #region Sort

        public IAscendingOrderedCouchQueryable<TSource> OrderBy<TProperty>(Expression<Func<TSource, TProperty>> keySelector)
        {
            return new CouchQueryable<TSource>(_db).OrderBy(keySelector);
        }

        public IDescendingOrderedCouchQueryable<TSource> OrderByDescending<TProperty>(Expression<Func<TSource, TProperty>> keySelector)
        {
            return new CouchQueryable<TSource>(_db).OrderByDescending(keySelector);
        }

        #endregion

        #region Fields

        public ICouchQueryable<TSource> Select(params Expression<Func<TSource, object>>[] fieldSelectors)
        {
            return new CouchQueryable<TSource>(_db).Select(fieldSelectors);
        }

        #endregion

        #region R

        public ICouchQueryable<TSource> WithReadQuorum(int count)
        {
            return new CouchQueryable<TSource>(_db).WithReadQuorum(count);
        }

        #endregion

        #region Bookmark

        public ICouchQueryable<TSource> UseBookmark(string bookmark)
        {
            return new CouchQueryable<TSource>(_db).UseBookmark(bookmark);
        }

        #endregion

        #region Update

        public ICouchQueryable<TSource> UpdateIndex()
        {
            return new CouchQueryable<TSource>(_db).UpdateIndex();
        }

        #endregion

        #region Stable

        public ICouchQueryable<TSource> FromStable()
        {
            return new CouchQueryable<TSource>(_db).FromStable();
        }

        #endregion

        #region Stats

        public ICouchQueryable<TSource> WithExecutionStats()
        {
            return new CouchQueryable<TSource>(_db).WithExecutionStats();
        }

        #endregion

        #region Request

        public Task<List<TSource>> ToListAsync()
        {
            return new CouchQueryable<TSource>(_db).ToListAsync();
        }

        #endregion

        #endregion

        #region Add

        public async Task<TSource> AddAsync(TSource document)
        {
            var response = await _db.NewDbRequest()
                .PostJsonAsync(document)
                .ReceiveJson<DocumentSaveResponse>()
                .SendAsync();

            ProcessSaveResponse(document, response);

            return document;
        }

        public async Task<IEnumerable<TSource>> AddRangeAsync(IEnumerable<TSource> documents) => await UpdateRangeAsync(documents);

        #endregion

        #region Update

        public async Task<TSource> UpdateAsync(TSource document)
        {
            var response = await _db.NewDbRequest()
                .AppendPathSegment(document.Id)
                .PutJsonAsync(document)
                .ReceiveJson<DocumentSaveResponse>()
                .SendAsync();

            ProcessSaveResponse(document, response);

            return document;
        }

        private static void ProcessSaveResponse(TSource document, DocumentSaveResponse response)
        {
            if (!response.Ok)
            {
                throw new CouchException(response.Error, response.Reason);
            }

            document.Id = response.Id;
            document.Rev = response.Rev;
        }

        public async Task<IEnumerable<TSource>> UpdateRangeAsync(IEnumerable<TSource> documents)
        {
            var response = await _db.NewDbRequest()
                .AppendPathSegment("_bulk_docs")
                .PostJsonAsync(new { docs = documents })
                .ReceiveJson<DocumentSaveResponse[]>()
                .SendAsync();

            var zipped = documents.Zip(response, (doc, saveResponse) => (Document: doc, SaveResponse: saveResponse));

            foreach (var (document, saveResponse) in zipped)
            {
                ProcessSaveResponse(document, saveResponse);
            }

            return documents;
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
