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

        public override object Execute(Expression e, bool completeResponse)
        {
            MethodInfo inMemoryMethodInfo = null;
            Expression[] inMemoryMethodParameters = Array.Empty<Expression>();
            if (e is MethodCallExpression m)
            {
                // If the last method is one of the supported in-memory one
                if (
                    m.Method.Name == "First" ||
                    m.Method.Name == "FirstOrDefault" ||
                    m.Method.Name == "Last" ||
                    m.Method.Name == "LastOrDefault" ||
                    m.Method.Name == "Single" ||
                    m.Method.Name == "SingleOrDefault")
                {
                    // Save method and params.
                    // Skip the current method in the translation
                    inMemoryMethodInfo = m.Method;
                    inMemoryMethodParameters = m.Arguments.Skip(1).ToArray();
                    e = m.Arguments[0];
                }
            }

            var body = Translate(e);
            Type elementType = TypeSystem.GetElementType(e.Type);

            MethodInfo method = typeof(CouchQueryProvider).GetMethod(nameof(CouchQueryProvider.GetCouchListOrFiltered));
            MethodInfo generic = method.MakeGenericMethod(elementType);
            var result = generic.Invoke(this, new[] { body, (object)inMemoryMethodInfo, inMemoryMethodParameters });
            return result;
        }
        
        private string Translate(Expression expression)
        {
            expression = Evaluator.PartialEval(expression);
            return new QueryTranslator(_settings).Translate(expression);
        }

        public object GetCouchListOrFiltered<T>(string body, MethodInfo inMemoryMethodInfo, Expression[] inMemoryMethodParameters)
        {
            FindResult<T> result = _flurlClient
                .Request(_connectionString)
                .AppendPathSegments(_db, "_find")
                .WithHeader("Content-Type", "application/json")
                .PostStringAsync(body).ReceiveJson<FindResult<T>>()
                .SendRequest();

            var couchList = new CouchList<T>(result.Docs.ToList(), result.Bookmark, result.ExecutionStats);

            // If no in-memory method found
            if (inMemoryMethodInfo == null)
            {
                return couchList;
            }

            // Find the same method in the Enumerable class
            MethodInfo enumarableMethod = typeof(Enumerable).GetMethods().Single(m =>
                m.Name == inMemoryMethodInfo.Name && m.GetParameters().Length - 1 == inMemoryMethodParameters.Length);

            // For every parameter, convert the expression to a executable method.
            var invokeParameter = new List<object> { couchList };
            IEnumerable<Delegate> callParams = inMemoryMethodParameters.Select(e =>
            {
                var unaryExpression = e as UnaryExpression;
                var lambdaExpression = unaryExpression.Operand as LambdaExpression;
                Delegate compiledLamda = lambdaExpression.Compile();
                return compiledLamda;
            });
            invokeParameter.AddRange(callParams);

            MethodInfo enumarableGenericMethod = enumarableMethod.MakeGenericMethod(typeof(T));
            var filtered = enumarableGenericMethod.Invoke(null, invokeParameter.ToArray());
            return filtered;
        }
    }
}
