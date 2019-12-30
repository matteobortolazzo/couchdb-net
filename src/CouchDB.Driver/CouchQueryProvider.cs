﻿using CouchDB.Driver.DTOs;
using CouchDB.Driver.CompositeExpressionsEvaluator;
using CouchDB.Driver.Helpers;
using CouchDB.Driver.Settings;
using CouchDB.Driver.Types;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace CouchDB.Driver
{
    internal class CouchQueryProvider : QueryProvider
    {
        private readonly IFlurlClient _flurlClient;
        private readonly CouchSettings _settings;
        private readonly string _connectionString;
        private readonly string _db;

        public CouchQueryProvider(IFlurlClient flurlClient, CouchSettings settings, string connectionString, string db)
        {
            _flurlClient = flurlClient ?? throw new ArgumentNullException(nameof(flurlClient));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public override string GetQueryText(Expression expression)
        {
            return Translate(ref expression);
        }

        public override object Execute(Expression expression, bool completeResponse)
        {
            // Remove from the expressions tree all IQueryable methods not supported by CouchDB and put them into the list
            var unsupportedMethodCallExpressions = new List<MethodCallExpression>();
            expression = RemoveUnsupportedMethodExpressions(expression, out var hasUnsupportedMethods, unsupportedMethodCallExpressions);

            var body = Translate(ref expression);
            Type elementType = TypeSystem.GetElementType(expression.Type);

            // Create generic GetCouchList method and invoke it, sending the request to CouchDB
            MethodInfo method = typeof(CouchQueryProvider).GetMethod(nameof(CouchQueryProvider.GetCouchList));
            MethodInfo generic = method.MakeGenericMethod(elementType);
            object result = null;
            try
            {
                result = generic.Invoke(this, new[] { body });
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            }

            // If no unsupported methods, return the result
            if (!hasUnsupportedMethods)
            {
                return result;
            }

            // For every unsupported method expression, execute it on the result
            foreach (MethodCallExpression inMemoryCall in unsupportedMethodCallExpressions)
            {
                result = InvokeUnsupportedMethodCallExpression(result, inMemoryCall);
            }
            return result;
        }

        private string Translate(ref Expression e)
        {
            e = Local.PartialEval(e);
            var whereVisitor = new BoolMemberToConstantEvaluator();
            e = whereVisitor.Visit(e);

            var pretranslator = new QueryPretranslator();
            e = pretranslator.Visit(e);

            return new QueryTranslator(_settings).Translate(e);
        }

        public object GetCouchList<T>(string body)
        {
            FindResult<T> result = _flurlClient
                .Request(_connectionString)
                .AppendPathSegments(_db, "_find")
                .WithHeader("Content-Type", "application/json")
                .PostStringAsync(body).ReceiveJson<FindResult<T>>()
                .SendRequest();

            var couchList = new CouchList<T>(result.Docs.ToList(), result.Bookmark, result.ExecutionStats);
            return couchList;
        }

        private Expression RemoveUnsupportedMethodExpressions(Expression expression, out bool hasUnsupportedMethods, IList<MethodCallExpression> unsupportedMethodCallExpressions)
        {
            if (unsupportedMethodCallExpressions == null)
            {
                throw new ArgumentNullException(nameof(unsupportedMethodCallExpressions));
            }

            // Search for method calls to run in-memory,
            // Once one is found all method calls after that must run in-memory.
            // The expression to translate in JSON ends with the last not in-memory call.
            bool IsUnsupportedMethodCallExpression(Expression ex)
            {
                if (ex is MethodCallExpression m)
                {
                    Expression nextCall = m.Arguments[0];
                    // Check if the next expression is unsupported
                    var isUnsupported = IsUnsupportedMethodCallExpression(nextCall);
                    if (isUnsupported)
                    {
                        unsupportedMethodCallExpressions.Add(m);
                        return isUnsupported;
                    }
                    // If the next call is supported and the current is in the composite list
                    if (QueryTranslator.CompositeQueryableMethods.Contains(m.Method.Name))
                    {
                        unsupportedMethodCallExpressions.Add(m);
                        return true;
                    }
                    // If the next call is supported and the current is not in the supported list
                    if (!QueryTranslator.NativeQueryableMethods.Contains(m.Method.Name))
                    {
                        unsupportedMethodCallExpressions.Add(m);
                        expression = nextCall;
                        return true;
                    }
                }
                return false;
            }

            hasUnsupportedMethods = IsUnsupportedMethodCallExpression(expression);
            return expression;
        }

        private object InvokeUnsupportedMethodCallExpression(object result, MethodCallExpression methodCallExpression)
        {
            MethodInfo queryableMethodInfo = methodCallExpression.Method;
            Expression[] queryableMethodArguments = methodCallExpression.Arguments.ToArray();

            // Since Max and Min are not map 1 to 1 from Queryable to Enumerable
            // they need to be handled differently
            MethodInfo FindEnumerableMethod()
            {
                if (queryableMethodInfo.Name == nameof(Queryable.Max) || queryableMethodInfo.Name == nameof(Queryable.Min))
                {
                    return FindEnumerableMinMax(queryableMethodInfo);
                }
                return typeof(Enumerable).GetMethods().Single(enumerableMethodInfo =>
                {
                    return
                        queryableMethodInfo.Name == enumerableMethodInfo.Name &&
                        ReflectionComparator.IsCompatible(queryableMethodInfo, enumerableMethodInfo);
                });
            }

            // Find the equivalent method in Enumerable
            MethodInfo enumarableMethodInfo = FindEnumerableMethod();

            // Add the list as first parameter of the call
            var invokeParameter = new List<object> { result };
            // Convert everty other parameter expression to real values
            IEnumerable<object> enumerableParameters = queryableMethodArguments.Skip(1).Select(GetArgumentValueFromExpression);
            // Add the other parameter to the complete list
            invokeParameter.AddRange(enumerableParameters);

            Type[] requestedGenericParameters = enumarableMethodInfo.GetGenericMethodDefinition().GetGenericArguments();
            Type[] genericParameters = queryableMethodInfo.GetGenericArguments();
            Type[] usableParameters = genericParameters.Take(requestedGenericParameters.Length).ToArray();
            MethodInfo enumarableGenericMethod = enumarableMethodInfo.MakeGenericMethod(usableParameters);
            try
            {
                var filtered = enumarableGenericMethod.Invoke(null, invokeParameter.ToArray());
                return filtered;
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException != null)
                {
                    throw e.InnerException;
                }
                throw;
            }
        }

        private object GetArgumentValueFromExpression(Expression e)
        {
            if (e is ConstantExpression c)
            {
                return c.Value;
            }
            if (e is UnaryExpression u && u.Operand is LambdaExpression l)
            {
                return l.Compile();
            }
            throw new NotImplementedException($"Expression of type {e.NodeType} not supported.");
        }

        private static MethodInfo FindEnumerableMinMax(MethodInfo queryableMethodInfo)
        {
            Type[] genericParams = queryableMethodInfo.GetGenericArguments();
            MethodInfo finalMethodInfo = typeof(Enumerable).GetMethods().Single(enumerableMethodInfo =>
            {
                Type[] enumerableArguments = enumerableMethodInfo.GetGenericArguments();
                return
                    enumerableMethodInfo.Name == queryableMethodInfo.Name &&
                    enumerableArguments.Length == genericParams.Length - 1 &&
                    enumerableMethodInfo.ReturnType == genericParams[1];
            });
            return finalMethodInfo;
        }
    }
}
