using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using CouchDB.Client.Query.Sort;
using Flurl.Http;
using Newtonsoft.Json;

namespace CouchDB.Client.Query
{
    public interface ICouchQueryable<TSource> where TSource : CouchEntity
    {
        IAscendingOrderedCouchQueryable<TSource> OrderBy<TProperty>(Expression<Func<TSource, TProperty>> keySelector);
        IDescendingOrderedCouchQueryable<TSource> OrderByDescending<TProperty>(Expression<Func<TSource, TProperty>> keySelector);
        ICouchQueryable<TSource> Select(params Expression<Func<TSource, object>>[] fieldSelectors);
        ICouchQueryable<TSource> Skip(int count);
        ICouchQueryable<TSource> Take(int count);
        ICouchQueryable<TSource> WithExecutionStats();
        ICouchQueryable<TSource> WithReadQuorum(int quorum);
        ICouchQueryable<TSource> UseBookmark(string bookmark);
        ICouchQueryable<TSource> UpdateIndex();
        ICouchQueryable<TSource> FromStable();
        Task<List<TSource>> ToListAsync();
    }

    public interface IAscendingOrderedCouchQueryable<TSource> : ICouchQueryable<TSource> where TSource : CouchEntity
    {
        IAscendingOrderedCouchQueryable<TSource> ThenBy<TProperty>(Expression<Func<TSource, TProperty>> keySelector);
    }

    public interface IDescendingOrderedCouchQueryable<TSource> : ICouchQueryable<TSource> where TSource : CouchEntity
    {
        IDescendingOrderedCouchQueryable<TSource> ThenByDescending<TProperty>(Expression<Func<TSource, TProperty>> keySelector);
    }
}
