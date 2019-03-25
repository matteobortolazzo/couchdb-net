using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CouchDB.Driver
{
    public abstract class QueryProvider : IQueryProvider
    {
        protected QueryProvider() { }

        IQueryable<S> IQueryProvider.CreateQuery<S>(Expression expression)
        {
            return new CouchQuery<S>(this, expression);
        }

        IQueryable IQueryProvider.CreateQuery(Expression expression)
        {
            Type elementType = TypeSystem.GetElementType(expression.Type);

            try
            {
                return (IQueryable)Activator.CreateInstance(typeof(CouchQuery<>).MakeGenericType(elementType), new object[] { this, expression });
            }

            catch (TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }

        S IQueryProvider.Execute<S>(Expression expression)
        {
            return (S)Execute(expression);
        }

        object IQueryProvider.Execute(Expression expression)
        {
            return Execute(expression);
        }
               
        public abstract string GetQueryText(Expression expression);
        public abstract object Execute(Expression expression);
    }
}
