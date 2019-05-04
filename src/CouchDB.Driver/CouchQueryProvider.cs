using CouchDB.Driver.DTOs;
using CouchDB.Driver.Helpers;
using CouchDB.Driver.Settings;
using CouchDB.Driver.Types;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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
            return Translate(expression);
        }

        public override object Execute(Expression expression, bool completeResponse)
        {
            // Remove from the expressions tree all IQueryable methods not supported by CouchDB and put them into the list
            var unsupportedMethodCallExpressions = new List<MethodCallExpression>();
            expression = RemoveUnsupportedMethodExpressions(expression, out var hasUnsupportedMethods, unsupportedMethodCallExpressions);
            
            var body = Translate(expression);
            Type elementType = TypeSystem.GetElementType(expression.Type);

            // Create generic GetCouchList method and invoke it, sending the request to CouchDB
            MethodInfo method = typeof(CouchQueryProvider).GetMethod(nameof(CouchQueryProvider.GetCouchList));
            MethodInfo generic = method.MakeGenericMethod(elementType);
            var result = generic.Invoke(this, new[] { body });

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
        private string Translate(Expression expression)
        {
            expression = Evaluator.PartialEval(expression);
            return new QueryTranslator(_settings).Translate(expression);
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
            // Find the equivalent method in Enumerable
            MethodInfo enumarableMethodInfo = typeof(Enumerable).GetMethods().Single(enumerableMethodInfo =>
            {
                return 
                    queryableMethodInfo.Name ==  enumerableMethodInfo.Name &&
                    ReflectionComparator.IsCompatible(queryableMethodInfo, enumerableMethodInfo);
            });

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
            var filtered = enumarableGenericMethod.Invoke(null, invokeParameter.ToArray());
            return filtered;
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
    }
}
