using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using CouchDB.Driver.Helpers;
using CouchDB.Driver.Types;

namespace CouchDB.Driver
{
    internal class QueryCompiler : IQueryCompiler
    {
        private readonly Type _couchListGenericType;
        private readonly IQuerySender _requestSender;
        private readonly IQueryTranslator _queryTranslator;

        private static readonly MethodInfo GenericSendMethod
            = typeof(IQuerySender).GetRuntimeMethods()
                .Single(m => (m.Name == "Send") && m.IsGenericMethod);

        public QueryCompiler(IQuerySender requestSender, IQueryTranslator queryTranslator)
        {
            _couchListGenericType = typeof(CouchList<>);
            _requestSender = requestSender;
            _queryTranslator = queryTranslator;
        }

        public TResult Execute<TResult>(Expression query)
            => SendRequest<TResult>(query, false, default);

        public TResult ExecuteAsync<TResult>(Expression query, CancellationToken cancellationToken)
            => SendRequest<TResult>(query, true, cancellationToken);

        private TResult SendRequest<TResult>(Expression query, bool async, CancellationToken cancellationToken)
        {
            var body = _queryTranslator.Translate(query);

            if (!(query is MethodCallExpression methodCallExpression))
            {
                throw new ArgumentException($"Invalid expression type {query.GetType().Name}.");
            }

            if (!(methodCallExpression.Arguments[0] is ConstantExpression queryableExpression))
            {
                throw new ArgumentException($"Invalid expression type {query.GetType().Name}.");
            }

            // Create CouchList type for the requested document type
            Type documentType = queryableExpression.Type.GenericTypeArguments[0];
            Type couchListType = _couchListGenericType.MakeGenericType(documentType);

            // Execute the database call
            var couchList = GenericSendMethod.MakeGenericMethod(couchListType)
                .Invoke(_requestSender, new object[] {body, async, cancellationToken});

            // If no operation must be done on the list
            if (!QueryTranslator.CompositeQueryableMethods.Contains(methodCallExpression.Method.Name))
            {
                return (TResult)couchList;
            }

            // Get Enumerable equivalent of the requested IQueryable method
            MethodInfo enumerableMethodInfo = GetEnumerableMethod(methodCallExpression.Method);

            // Create parameters
            var parameters = new List<object> {couchList};
            IEnumerable<object> lambdaParameters = methodCallExpression.Arguments
                .Skip(1)
                .Select(GetArgumentValueFromExpression);
            parameters.AddRange(lambdaParameters);

            // Execute
            try
            {
                return (TResult)enumerableMethodInfo.Invoke(null, parameters.ToArray());
            }
            catch (TargetInvocationException targetInvocationException)
            {
                throw targetInvocationException.InnerException;
            }
        }

        private static object GetArgumentValueFromExpression(Expression e)
        {
            return e switch
            {
                ConstantExpression c => c.Value,
                UnaryExpression u when u.Operand is LambdaExpression l => l.Compile(),
                _ => throw new NotImplementedException($"Expression of type {e.NodeType} not supported.")
            };
        }

        private static MethodInfo GetEnumerableMethod(MethodInfo queryableMethodInfo)
        {
            Check.NotNull(queryableMethodInfo, nameof(queryableMethodInfo));

            MethodInfo FindEnumerableMethod()
            {
                if (queryableMethodInfo.Name == nameof(Queryable.Max) || queryableMethodInfo.Name == nameof(Queryable.Min))
                {
                    return FindEnumerableMinMax(queryableMethodInfo);
                }
                return typeof(Enumerable).GetMethods().Single(info =>
                    queryableMethodInfo.Name == info.Name && ReflectionComparator.IsCompatible(queryableMethodInfo, info));
            }
            
            MethodInfo genericEnumerableMethodInfo = FindEnumerableMethod();

            Type[] requestedGenericParameters = genericEnumerableMethodInfo.GetGenericMethodDefinition().GetGenericArguments();
            Type[] genericParameters = queryableMethodInfo.GetGenericArguments();
            Type[] usableParameters = genericParameters.Take(requestedGenericParameters.Length).ToArray();
            MethodInfo enumerableMethodInfo = genericEnumerableMethodInfo.MakeGenericMethod(usableParameters);

            return enumerableMethodInfo;
        }

        private static MethodInfo FindEnumerableMinMax(MethodBase queryableMethodInfo)
        {
            Type[] genericParams = queryableMethodInfo.GetGenericArguments();
            return typeof(Enumerable).GetMethods().Single(enumerableMethodInfo =>
            {
                Type[] enumerableArguments = enumerableMethodInfo.GetGenericArguments();
                return
                    enumerableMethodInfo.Name == queryableMethodInfo.Name &&
                    enumerableArguments.Length == genericParams.Length - 1 &&
                    enumerableMethodInfo.ReturnType == genericParams[1];
            });
        }
    }
}
