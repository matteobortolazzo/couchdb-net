using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.Shared;
using CouchDB.Driver.Types;

namespace CouchDB.Driver.Query
{
    internal class QueryCompiler : IQueryCompiler
    {
        private readonly IQueryOptimizer _queryOptimizer;
        private readonly IQueryTranslator _queryTranslator;
        private readonly IQuerySender _requestSender;
        private readonly string? _discriminator;

        private static readonly MethodInfo RequestSendMethod
            = typeof(IQuerySender).GetRuntimeMethods()
                .Single(m => (m.Name == nameof(IQuerySender.Send)) && m.IsGenericMethod);

        private static readonly MethodInfo PostProcessResultMethod
            = typeof(QueryCompiler).GetMethod(nameof(PostProcessResult),
                BindingFlags.NonPublic | BindingFlags.Static);

        private static readonly MethodInfo PostProcessResultAsyncMethod
            = typeof(QueryCompiler).GetMethod(nameof(PostProcessResultAsync),
                BindingFlags.NonPublic | BindingFlags.Static);

        public QueryCompiler(IQueryOptimizer queryOptimizer, IQueryTranslator queryTranslator,
            IQuerySender requestSender, string? discriminator)
        {
            _queryOptimizer = queryOptimizer;
            _queryTranslator = queryTranslator;
            _requestSender = requestSender;
            _discriminator = discriminator;
        }

        public string ToString(Expression query)
        {
            Expression optimizedQuery = _queryOptimizer.Optimize(query, _discriminator);
            return _queryTranslator.Translate(optimizedQuery);
        }

        public TResult Execute<TResult>(Expression query)
            => SendRequest<TResult>(query, false, default);

        public TResult ExecuteAsync<TResult>(Expression query, CancellationToken cancellationToken)
            => SendRequest<TResult>(query, true, cancellationToken);

        private TResult SendRequest<TResult>(Expression query, bool async, CancellationToken cancellationToken)
        {
            try
            {
                return query switch
                {
                    ConstantExpression _ => SendRequestWithoutFilter<TResult>(query,
                        async, cancellationToken),
                    MethodCallExpression methodCallExpression => SendRequestWithFilter<TResult>(methodCallExpression,
                        query,
                        async, cancellationToken),
                    _ => throw new ArgumentException($"Expression of type {query.GetType().Name} is not valid.")
                };
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                return default;
            }
        }

        private TResult SendRequestWithoutFilter<TResult>(Expression query, bool async,
            CancellationToken cancellationToken)
        {
            Expression optimizedQuery = _queryOptimizer.Optimize(query, _discriminator);
            var body = _queryTranslator.Translate(optimizedQuery);
            return _requestSender.Send<TResult>(body, async, cancellationToken);
        }

        private TResult SendRequestWithFilter<TResult>(MethodCallExpression methodCallExpression, Expression query,
            bool async, CancellationToken cancellationToken)
        {
            Expression optimizedQuery = _queryOptimizer.Optimize(query, _discriminator);

            if (optimizedQuery is not MethodCallExpression optimizedMethodCall)
            {
                throw new ArgumentException($"Expression of type {optimizedQuery.GetType().Name} is not valid.");
            }

            var body = _queryTranslator.Translate(optimizedQuery);

            // If no operation must be done on the list return
            if (!methodCallExpression.Method.IsSupportedByComposition())
            {
                return _requestSender.Send<TResult>(body, async, cancellationToken);
            }

            Type documentType = GetDocumentType(methodCallExpression);
            Type couchListType = GetCouchListType(documentType, async);
            Type returnType = GetReturnType<TResult>(async);

            // Query database
            object couchQueryable = RequestSendMethod
                .MakeGenericMethod(couchListType)
                .Invoke(_requestSender, new object[] { body, async, cancellationToken });

            // Apply in-memory operations
            MethodInfo postProcessResultMethodInfo = (async
                    ? PostProcessResultAsyncMethod
                    : PostProcessResultMethod)
                .MakeGenericMethod(documentType, returnType);

            return (TResult)postProcessResultMethodInfo.Invoke(null,
                new[] { couchQueryable, methodCallExpression, optimizedMethodCall });
        }

        private static Type GetDocumentType(MethodCallExpression methodCall)
        {
            while (true)
            {
                if (methodCall.Arguments[0] is ConstantExpression listExpression)
                {
                    return listExpression.Type.GetGenericArguments()[0];
                }

                if (methodCall.Arguments[0] is MethodCallExpression methodCallArg)
                {
                    methodCall = methodCallArg;
                    continue;
                }

                throw new InvalidOperationException();
            }
        }

        private static Type GetCouchListType(Type documentType, bool async)
        {
            Type couchListType = typeof(CouchList<>).MakeGenericType(documentType);
            return async
                ? typeof(Task<>).MakeGenericType(couchListType)
                : couchListType;
        }

        private static Type GetReturnType<TResult>(bool async) =>
            async
                ? typeof(TResult).GetGenericArguments()[0]
                : typeof(TResult);

        private static TResult PostProcessResult<TSource, TResult>(
            CouchList<TSource> couchList,
            MethodCallExpression originalMethodCall,
            MethodCallExpression optimizedMethodCall)
        {
            Type documentType = typeof(TSource);
            object result = couchList;

            // If the original call contains a property selector, execute Enumerable.Select
            if (originalMethodCall.ContainsSelector())
            {
                Type selectorType = originalMethodCall.GetSelectorType();
                MethodInfo selectMethodInfo = EnumerableMethods
                    .GetEnumerableEquivalent(QueryableMethods.Select)
                    .MakeGenericMethod(documentType, selectorType);
                Delegate selector = originalMethodCall.GetSelectorDelegate();
                result = selectMethodInfo.Invoke(null, new object[] { couchList, selector });
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
                return (TResult)enumerableMethodInfo.Invoke(null, new[] { result });
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                return default;
            }
        }

        private static async Task<TResult> PostProcessResultAsync<TSource, TResult>(
            Task<CouchList<TSource>> couchListTask,
            MethodCallExpression originalMethodCall,
            MethodCallExpression optimizedMethodCall)
        {
            CouchList<TSource> couchList = await couchListTask.ConfigureAwait(false);
            if (couchList == null)
            {
                throw new ArgumentNullException(nameof(couchListTask));
            }

            return PostProcessResult<TSource, TResult>(couchList, originalMethodCall, optimizedMethodCall);
        }
    }
}