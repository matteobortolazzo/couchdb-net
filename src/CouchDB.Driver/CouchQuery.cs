using CouchDB.Driver.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace CouchDB.Driver
{
    internal class CouchQuery<T> : IQueryable<T>, IQueryable, IEnumerable<T>, IEnumerable, IOrderedQueryable<T>, IOrderedQueryable
    {
        private readonly QueryProvider _provider;
        private readonly Expression _expression;

        public CouchQuery(QueryProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException("provider");
            _expression = Expression.Constant(this);
        }
               
        public CouchQuery(QueryProvider provider, Expression expression)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            if (!typeof(IQueryable<T>).IsAssignableFrom(expression.Type))
            {
                throw new ArgumentOutOfRangeException("expression");
            }

            _provider = provider;
            _expression = expression;
        }
               
        Expression IQueryable.Expression
        {
            get { return _expression; }
        }
               
        Type IQueryable.ElementType
        { 
            get { return typeof(T); }
        }

        IQueryProvider IQueryable.Provider
        {
            get { return _provider; }
        }
               
        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)_provider.Execute(_expression, false)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_provider.Execute(_expression, false)).GetEnumerator();
        }

        public override string ToString()
        {
            return _provider.GetQueryText(_expression);
        }

        public ICouchList<T> ToCouchList()
        {
            return (ICouchList<T>)_provider.Execute(_expression, true);
        }
    }
}
