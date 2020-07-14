using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CouchDB.Driver.Query;
using CouchDB.Driver.Types;

namespace CouchDB.Driver
{
    internal class CouchQueryable<TResult> : IOrderedQueryable<TResult>
    {
        private readonly Expression _expression;
        private readonly IAsyncQueryProvider _queryProvider;

        public CouchQueryable(IAsyncQueryProvider queryProvider)
        { 
            _queryProvider = queryProvider;
            _expression = Expression.Constant(this);
        }
               
        public CouchQueryable(IAsyncQueryProvider queryProvider, Expression expression)
        {
            _queryProvider = queryProvider;
            _expression = expression;
        }

        public override string ToString()
        {
            return _queryProvider.ToString(_expression);
        }

        Expression IQueryable.Expression
        {
            get { return _expression; }
        }
               
        Type IQueryable.ElementType
        { 
            get { return typeof(TResult); }
        }

        IQueryProvider IQueryable.Provider
        {
            get { return _queryProvider; }
        }
        
        public IEnumerator<TResult> GetEnumerator()
            => _queryProvider.Execute<IEnumerable<TResult>>(_expression).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => _queryProvider.Execute<IEnumerable>(_expression).GetEnumerator();

        public Task<CouchList<TResult>> ToCouchListAsync(CancellationToken cancellationToken = default)
            => _queryProvider.ExecuteAsync<Task<CouchList<TResult>>>(_expression, cancellationToken);
    }
}
