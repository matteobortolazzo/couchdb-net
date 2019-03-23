using CouchDB.Client.Helpers;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CouchDB.Client
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

        #region Reading

        public List<TSource> ToList()
        {
            return QueryableNoFilter().Where(_ => true).ToList();
        }
        public IQueryable<TSource> Where(Expression<Func<TSource, bool>> predicate)
        {
            return AsQueryable().Where(predicate);
        }
        public IQueryable<TSource> OrderBy<TKey>(Expression<Func<TSource, TKey>> keySelector)
        {
            return QueryableNoFilter().OrderBy(keySelector);
        }
        public IQueryable<TSource> OrderByDescending<TKey>(Expression<Func<TSource, TKey>> keySelector)
        {
            return QueryableNoFilter().OrderByDescending(keySelector);
        }
        public IQueryable<TResult> Select<TResult>(Expression<Func<TSource, TResult>> selector)
        {
            return QueryableNoFilter().Select(selector);
        }
        public IQueryable<TSource> Skip(int count)
        {
            return QueryableNoFilter().Skip(count);
        }
        public IQueryable<TSource> Take(int count)
        {
            return QueryableNoFilter().Take(count);
        }
        public async Task<TSource> FindAsync(string docId)
        {
            return await NewRequest()
                .AppendPathSegment(docId)
                .GetJsonAsync<TSource>()
                .SendRequestAsync();
        }

        #endregion

        #region Helper

        private IFlurlRequest NewRequest()
        {
            return flurlClient.Request(connectionString).AppendPathSegment(db);
        }
        private IQueryable<TSource> QueryableNoFilter()
        {
            return AsQueryable().Where(_ => true);
        }

        #endregion
    }
}
