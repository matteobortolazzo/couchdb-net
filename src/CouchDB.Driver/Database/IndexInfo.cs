using System;
using System.Linq.Expressions;

namespace CouchDB.Driver.Database
{
    public class IndexInfo<TSource> where TSource : class
    {
        public string? IndexName { get; set; }
        public Expression<Func<TSource, object>>? Fields { get; set; }
    }
}