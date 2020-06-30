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
            return query switch
            {
                ConstantExpression constantExpression => SendRequestWithoutFilter<TResult>(constantExpression, query,
                    async, cancellationToken),
                MethodCallExpression methodCallExpression => SendRequestWithFilter<TResult>(methodCallExpression, query,
                    async, cancellationToken),
                _ => throw new ArgumentException($"Expression of type {query.GetType().Name} is not valid.")
            };
        }

        private TResult SendRequestWithoutFilter<TResult>(Expression constantExpression, Expression query, bool async, CancellationToken cancellationToken)
        {
            Type documentType = constantExpression.Type.GetGenericArguments()[0];
            return (TResult)SendRequest(query, documentType, async, cancellationToken);
        }

        private TResult SendRequestWithFilter<TResult>(MethodCallExpression methodCallExpression, Expression query,
            bool async, CancellationToken cancellationToken)
        {
            if (!(methodCallExpression.Arguments[0] is ConstantExpression queryableExpression))
            {
                throw new InvalidOperationException($"The first argument of the method {methodCallExpression.Method.Name} must be a constant.");
            }

            Expression optimizedQuery = _queryOptimizer.Optimize(query);

            if (!(optimizedQuery is MethodCallExpression optimizedMethodCall))
            {
                throw new ArgumentException($"Expression of type {optimizedQuery.GetType().Name} is not valid.");
            }

            Type documentType = queryableExpression.Type.GenericTypeArguments[0];
            var couchList = SendRequest(query, documentType, async, cancellationToken);

            // If no operation must be done on the list return, otherwise post-process it
            return methodCallExpression.Method.IsSupportedByComposition()
                ? PostProcessResult<TResult>(couchList, documentType, methodCallExpression, optimizedMethodCall)
                : (TResult)couchList;
        }

        private object SendRequest(Expression query, Type documentType, bool async, CancellationToken cancellationToken)
        {
            var body = _queryTranslator.Translate(query);

            Type couchListType = _couchListGenericType.MakeGenericType(documentType);

            // Execute the database call
            return GenericSendMethod.MakeGenericMethod(couchListType)
                .Invoke(_requestSender, new object[] { body, async, cancellationToken });
        }

        private static TResult PostProcessResult<TResult>(object couchList, Type documentType, MethodCallExpression methodCallExpression,
            MethodCallExpression optimizedMethodCall)
        {
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
