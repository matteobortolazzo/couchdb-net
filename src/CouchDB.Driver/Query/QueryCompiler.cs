using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.Shared;
using CouchDB.Driver.Types;

namespace CouchDB.Driver.Query
{
    internal class QueryCompiler : IQueryCompiler
    {
        private readonly Type _couchListGenericType;
        private readonly IQueryOptimizer _queryOptimizer;
        private readonly IQueryTranslator _queryTranslator;
        private readonly IQuerySender _requestSender;

        private static readonly MethodInfo GenericSendMethod
            = typeof(IQuerySender).GetRuntimeMethods()
                .Single(m => (m.Name == "Send") && m.IsGenericMethod);

        public QueryCompiler(IQueryOptimizer queryOptimizer, IQueryTranslator queryTranslator, IQuerySender requestSender)
        {
            _couchListGenericType = typeof(CouchList<>);
            _queryOptimizer = queryOptimizer;
            _queryTranslator = queryTranslator;
            _requestSender = requestSender;
        }

        public string ToString(Expression query)
        {
            Expression optimizedQuery = _queryOptimizer.Optimize(query);
            return _queryTranslator.Translate(optimizedQuery);
        }

        public TResult Execute<TResult>(Expression query)
            => SendRequest<TResult>(query, false, default);

        public TResult ExecuteAsync<TResult>(Expression query, CancellationToken cancellationToken)
            => SendRequest<TResult>(query, true, cancellationToken);

        private TResult SendRequest<TResult>(Expression query, bool async, CancellationToken cancellationToken)
        {
            Expression optimizedQuery = _queryOptimizer.Optimize(query);

            var body = _queryTranslator.Translate(optimizedQuery);

            if (!(query is MethodCallExpression methodCallExpression))
            {
                throw new ArgumentException($"Expression of type {query.GetType().Name} is not valid.");
            }

            if (!(optimizedQuery is MethodCallExpression optimizedMethodCall))
            {
                throw new InvalidOperationException($"Expression of type {optimizedQuery.GetType().Name} is not valid.");
            }

            if (!(methodCallExpression.Arguments[0] is ConstantExpression queryableExpression))
            {
                throw new InvalidOperationException($"The first argument of the method {methodCallExpression.Method.Name} must be a constant.");
            }

            // Create CouchList type for the requested document type
            Type documentType = queryableExpression.Type.GenericTypeArguments[0];
            Type couchListType = _couchListGenericType.MakeGenericType(documentType);

            // Execute the database call
            var couchList = GenericSendMethod.MakeGenericMethod(couchListType)
                .Invoke(_requestSender, new object[] {body, async, cancellationToken});

            // If no operation must be done on the list
            if (!methodCallExpression.Method.GetGenericMethodDefinition().IsSupportedByComposition())
            {
                return (TResult)couchList;
            }
            
            // If the original call contains a property selector, execute Enumerable.Select
            if (methodCallExpression.ContainsSelector())
            {
                Type selectorType = methodCallExpression.GetSelectorType();
                MethodInfo selectMethodInfo = EnumerableMethods
                    .GetEnumerableEquivalent(QueryableMethods.Select)
                    .MakeGenericMethod(documentType, selectorType);
                Delegate selector = methodCallExpression.GetSelectorDelegate();
                couchList = selectMethodInfo.Invoke(null, new[] { couchList, selector });
            }

            // Get Enumerable equivalent of the last IQueryable method
            MethodInfo enumerableMethodInfo = EnumerableMethods.GetEnumerableEquivalent(optimizedMethodCall.Method);
            if (enumerableMethodInfo.IsGenericMethod)
            {
                enumerableMethodInfo = enumerableMethodInfo.MakeGenericMethod(documentType);
            }
            
            // Execute
            try
            {
                return (TResult)enumerableMethodInfo.Invoke(null, new[] { couchList });
            }
            catch (TargetInvocationException targetInvocationException)
            {
                throw targetInvocationException.InnerException;
            }
        }
    }
}
