using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace CouchDB.Driver
{
    internal class CouchQuery<T> : IQueryable<T>, IQueryable, IEnumerable<T>, IEnumerable, IOrderedQueryable<T>, IOrderedQueryable
    {
        private readonly QueryProvider provider;
        private readonly Expression expression;

        public CouchQuery(QueryProvider provider)
        {
            this.provider = provider ?? throw new ArgumentNullException("provider");
            this.expression = Expression.Constant(this);
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

            this.provider = provider;
            this.expression = expression;
        }
               
        Expression IQueryable.Expression
        {
            get { return this.expression; }
        }
               
        Type IQueryable.ElementType
        { 
            get { return typeof(T); }
        }

        IQueryProvider IQueryable.Provider
        {
            get { return this.provider; }
        }
               
        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)this.provider.Execute(this.expression)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.provider.Execute(this.expression)).GetEnumerator();
        }

        public override string ToString()
        {
            return this.provider.GetQueryText(this.expression);
        }
    }
}
