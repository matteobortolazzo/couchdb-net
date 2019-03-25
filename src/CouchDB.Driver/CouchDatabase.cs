using CouchDB.Driver.Extensions;
using CouchDB.Driver.Helpers;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CouchDB.Driver
{
    public class CouchDatabase<TSource>
    {
        private readonly QueryProvider queryProvider;
        private readonly FlurlClient flurlClient;
        private readonly string connectionString;
        private readonly string db;

        internal CouchDatabase(FlurlClient flurlClient, string connectionString, string db)
        {
            this.flurlClient = flurlClient;
            this.connectionString = connectionString;
            this.db = db;
            this.queryProvider = new CouchQueryProvider(flurlClient, connectionString, db);
        }

        public IQueryable<TSource> AsQueryable()
        {
            return new CouchQuery<TSource>(queryProvider);
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
        public IQueryable<TSource> UpdateIndex(bool needUpdate)
        {
            return AsQueryable().UpdateIndex(needUpdate);
        }
        public IQueryable<TSource> UseBookmark(bool isFromStable)
        {
            return AsQueryable().FromStable(isFromStable);
        }
        public IQueryable<TSource> UseIndex(params string[] indexes)
        {
            return AsQueryable().UseIndex(indexes);
        }

        #endregion

        #region Single

        public async Task<TSource> FindAsync(string docId)
        {
            return await NewRequest()
                .AppendPathSegment(docId)
                .GetJsonAsync<TSource>()
                .SendRequestAsync();
        }

        #endregion

        public override string ToString()
        {
            return AsQueryable().ToString();
        }

        #region Helper

        private IFlurlRequest NewRequest()
        {
            return flurlClient.Request(connectionString).AppendPathSegment(db);
        }

        #endregion
    }
}
