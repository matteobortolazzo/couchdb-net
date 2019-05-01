using CouchDB.Driver.DTOs;
using CouchDB.Driver.Helpers;
using CouchDB.Driver.Settings;
using CouchDB.Driver.Types;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public override object Execute(Expression e, bool completeResponse)
        {
            var inMemoryCalls = new List<MethodCallExpression>();

            // Search for method calls to run in-memory,
            // Once one is found all method calls after that must run in-memory.
            // The expression to translate in JSON ends with the last not in-memory call.
            bool InspectInmemoryCalls(Expression ex)
            {
                if (ex is MethodCallExpression m)
                {
                    Expression previousCall = m.Arguments[0];
                    var needInMemory = InspectInmemoryCalls(previousCall);
                    if (needInMemory)
                    {
                        inMemoryCalls.Add(m);
                        return needInMemory;
                    }
                    if (!QueryTranslator.NativeIQueryableMethods.Contains(m.Method.Name))
                    {
                        inMemoryCalls.Add(m);
                        e = previousCall;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            var needInMemoryOperations = InspectInmemoryCalls(e);

            var body = Translate(e);
            Type elementType = TypeSystem.GetElementType(e.Type);

            MethodInfo method = typeof(CouchQueryProvider).GetMethod(nameof(CouchQueryProvider.GetCouchListOrFiltered));
            MethodInfo generic = method.MakeGenericMethod(elementType);
            var result = generic.Invoke(this, new[] { body });

            if (!needInMemoryOperations)
            {
                return result;
            }

            foreach(MethodCallExpression inMemoryCall in inMemoryCalls)
            {
                result = ApplyInMemoryQuery(result, inMemoryCall);
            }
            return result;
        }
        private string Translate(Expression expression)
        {
            expression = Evaluator.PartialEval(expression);
            return new QueryTranslator(_settings).Translate(expression);
        }

        public object GetCouchListOrFiltered<T>(string body)
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

        private object ApplyInMemoryQuery(object result, MethodCallExpression callExp)
        {
            MethodInfo methodInfo = callExp.Method;
            Expression[] methodArguments = callExp.Arguments.ToArray();
            MethodInfo enumarableMethod = GetEnumerableEquivalent(callExp);

            object GetMethodParameter(Expression e)
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

            // For every parameter, convert the expression to a executable method.
            var invokeParameter = new List<object> { result };
            IEnumerable<object> callParams = methodArguments.Skip(1).Select(GetMethodParameter);
            invokeParameter.AddRange(callParams);

            Type[] requestedGenericParameters = enumarableMethod.GetGenericMethodDefinition().GetGenericArguments();
            Type[] genericParameters = methodInfo.GetGenericArguments();
            Type[] usableParameters = genericParameters.Take(requestedGenericParameters.Length).ToArray();
            MethodInfo enumarableGenericMethod = enumarableMethod.MakeGenericMethod(usableParameters);
            var filtered = enumarableGenericMethod.Invoke(null, invokeParameter.ToArray());
            return filtered;
        }

        private static MethodInfo GetEnumerableEquivalent(MethodCallExpression callExp)
        {
            MethodInfo methodInfo = callExp.Method;
            Expression[] methodArguments = callExp.Arguments.ToArray();

            bool IsCorrectMethod(MethodInfo m)
            {
                // Must have the same name
                if (m.Name != methodInfo.Name)
                {
                    return false;
                }

                // Must have the same number of parameters
                ParameterInfo[] parameters = m.GetParameters();
                if (parameters.Length != methodArguments.Length)
                {
                    return false;
                }

                if (methodInfo.ReturnType != m.ReturnType)
                {
                    if (!methodInfo.ReturnType.IsGenericParameter && !typeof(IQueryable<>).IsAssignableFrom(methodInfo.ReturnType) &&
                        !m.ReturnType.IsGenericParameter && !typeof(IEnumerable<>).IsAssignableFrom(m.ReturnType))
                    {
                        return false;
                    }
                }
                
                for (var i = 1; i < parameters.Length; i++)
                {
                    Expression currentExpression = methodArguments[i];

                    // If the expression is constant, check the type
                    if (currentExpression is ConstantExpression c)
                    {
                        if (c.Type != parameters[i].ParameterType)
                        {
                            return false;
                        }
                    }
                    // If it's a lambda expression
                    else if (currentExpression is UnaryExpression u && u.Operand is LambdaExpression l)
                    {
                        Type[] currentParamType = parameters[i].ParameterType.GetGenericArguments();
                        ReadOnlyCollection<ParameterExpression> lambdaParameters = l.Parameters;
                        Type lambdaReturnType = l.ReturnType;

                        if (currentParamType.Length - 1 > lambdaParameters.Count)
                        {
                            return false;
                        }

                        // The return type must be the same
                        var enumerableReturnType = currentParamType[currentParamType.Length - 1];
                        if (!enumerableReturnType.IsGenericType && !enumerableReturnType.IsGenericParameter && enumerableReturnType != lambdaReturnType)
                        {
                            return false;
                        }

                        // For every parameter, the type must be generic or the same
                        for (var j = 0; j < currentParamType.Length - 1; j++)
                        {
                            Type enumerableType = currentParamType[j];
                            Type callType = lambdaParameters[j].Type;
                            if (enumerableType.IsGenericParameter)
                            {
                                continue;
                            }
                            if (enumerableType != callType)
                            {
                                return false;
                            }
                        }
                    }
                }
                return true;
            }

            var enumarableMethods = typeof(Enumerable).GetMethods().Where(IsCorrectMethod).ToList();
            MethodInfo enumarableMethod = enumarableMethods.First();
            if (enumarableMethod == null)
            {
                throw new NotSupportedException($"The method '{methodInfo.Name}' is not supported");
            }
            return enumarableMethod;
        }
    }
}
