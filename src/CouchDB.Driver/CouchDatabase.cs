using CouchDB.Driver.Exceptions;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.Helpers;
using CouchDB.Driver.Types;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CouchDB.Driver
{
    public class CouchDatabase<TSource> where TSource : CouchEntity
    {
        private readonly QueryProvider _queryProvider;
        private readonly FlurlClient _flurlClient;
        private readonly CouchSettings _settings;
        private readonly string _connectionString;
        private readonly string _db;

        internal CouchDatabase(FlurlClient flurlClient, CouchSettings settings, string connectionString, string db)
        {
            _flurlClient = flurlClient ?? throw new ArgumentNullException(nameof(flurlClient));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _queryProvider = new CouchQueryProvider(flurlClient, _settings, connectionString, db);
        }

        public IQueryable<TSource> AsQueryable()
        {
            return new CouchQuery<TSource>(_queryProvider);
        }
        
        #region Query

        public List<TSource> ToList()
        {
            return AsQueryable().ToList();
        }
        public Task<List<TSource>> ToListAsync()
        {
            return AsQueryable().ToListAsync();
        }
        public IQueryable<TSource> Where(Expression<Func<TSource, bool>> predicate)
        {
            return AsQueryable().Where(predicate);
        }
        public IOrderedQueryable<TSource> OrderBy<TKey>(Expression<Func<TSource, TKey>> keySelector)
        {
            return AsQueryable().OrderBy(keySelector);
        }
        public IOrderedQueryable<TSource> OrderByDescending<TKey>(Expression<Func<TSource, TKey>> keySelector)
        {
            return AsQueryable().OrderByDescending(keySelector);
        }
        public IQueryable<TResult> Select<TResult>(Expression<Func<TSource, TResult>> selector)
        {
            return AsQueryable().Select(selector);
        }
        public IQueryable<TSource> Skip(int count)
        {
            return AsQueryable().Skip(count);
        }
        public IQueryable<TSource> Take(int count)
        {
            return AsQueryable().Take(count);
        }
        public IQueryable<TSource> UseBookmark(string bookmark)
        {
            return AsQueryable().UseBookmark(bookmark);
        }
        public IQueryable<TSource> WithReadQuorum(int quorum)
        {
            return AsQueryable().WithReadQuorum(quorum);
        }
        public IQueryable<TSource> WithoutIndexUpdate()
        {
            return AsQueryable().WithoutIndexUpdate();
        }
        public IQueryable<TSource> FromStable()
        {
            return AsQueryable().FromStable();
        }
        public IQueryable<TSource> UseIndex(params string[] indexes)
        {
            return AsQueryable().UseIndex(indexes);
        }

        #endregion

        #region Find

        public async Task<TSource> FindAsync(string docId)
        {
            return await NewRequest()
                .AppendPathSegment(docId)
                .GetJsonAsync<TSource>()
                .SendRequestAsync();
        }

        #endregion

        #region Writing

        public async Task<TSource> AddAsync(TSource item)
        {
            var response = await NewRequest()
                .PostJsonAsync(item)
                .ReceiveJson<DocumentSaveResponse>()
                .SendRequestAsync();

            return (TSource)item.ProcessSaveResponse(response);
        }
        public async Task<IEnumerable<TSource>> AddRangeAsync(IEnumerable<TSource> documents)
        {
            var response = await NewRequest()
                .AppendPathSegment("_bulk_docs")
                .PostJsonAsync(new { docs = documents })
                .ReceiveJson<DocumentSaveResponse[]>()
                .SendRequestAsync();

            var zipped = documents.Zip(response, (doc, saveResponse) => (Document: doc, SaveResponse: saveResponse));
            foreach (var (document, saveResponse) in zipped)
                document.ProcessSaveResponse(saveResponse);
            return documents;
        }

        public async Task<TSource> UpdateAsync(TSource item)
        {
            var response = await NewRequest()
                .AppendPathSegment(item.Id)
                .PutJsonAsync(item)
                .ReceiveJson<DocumentSaveResponse>()
                .SendRequestAsync();

            return (TSource)item.ProcessSaveResponse(response);
        }
        public Task<IEnumerable<TSource>> UpdateRangeAsync(IEnumerable<TSource> documents)
        {
            return AddRangeAsync(documents);
        }

        public async Task RemoveAsync(TSource document)
        {
            await NewRequest()
                .AppendPathSegment(document.Id)
                .SetQueryParam("rev", document.Rev)
                .DeleteAsync()
                .SendRequestAsync();
        }

        #endregion
                
        #region Utils

        public async Task CompactAsync()
        {
            await NewRequest()
                .AppendPathSegment("_compact")
                .PostJsonAsync(null)
                .SendRequestAsync();
        }
        public async Task<CouchDatabaseInfo> GetInfoAsync()
        {
            return await NewRequest()
                .GetJsonAsync<CouchDatabaseInfo>()
                .SendRequestAsync();
        }

        #endregion

        #region Override

        public override string ToString()
        {
            return AsQueryable().ToString();
        }

        #endregion

        #region Helper

        private IFlurlRequest NewRequest()
        {
            return _flurlClient.Request(_connectionString).AppendPathSegment(_db);
        }

        #endregion
    }
}
