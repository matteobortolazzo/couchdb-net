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
            MethodInfo _filterMethodInfo = null;
            Expression[] _filteringExpressions = Array.Empty<Expression>();
            if (e is MethodCallExpression m)
            {
                if (
                    m.Method.Name == "First" ||
                    m.Method.Name == "FirstOrDefault" ||
                    m.Method.Name == "Last" ||
                    m.Method.Name == "LastOrDefault" ||
                    m.Method.Name == "Single" ||
                    m.Method.Name == "SingleOrDefault")
                {
                    _filterMethodInfo = m.Method;
                    _filteringExpressions = m.Arguments.Skip(1).ToArray();
                    e = m.Arguments[0];
                }
            }

            var body = Translate(e);
            Type elementType = TypeSystem.GetElementType(e.Type);

            MethodInfo method = typeof(CouchQueryProvider).GetMethod(nameof(CouchQueryProvider.GetCouchListOrFiltered));
            MethodInfo generic = method.MakeGenericMethod(elementType);
            var result = generic.Invoke(this, new[] { body, (object)_filterMethodInfo, _filteringExpressions });
            return result;
        }
        
        private string Translate(Expression expression)
        {
            expression = Evaluator.PartialEval(expression);
            return new QueryTranslator(_settings).Translate(expression);
        }

        public object GetCouchListOrFiltered<T>(string body, MethodInfo filteringMethodInfo, Expression[] filteringExpressions)
        {
            FindResult<T> result = _flurlClient
                .Request(_connectionString)
                .AppendPathSegments(_db, "_find")
                .WithHeader("Content-Type", "application/json")
                .PostStringAsync(body).ReceiveJson<FindResult<T>>()
                .SendRequest();

            var couchList = new CouchList<T>(result.Docs.ToList(), result.Bookmark, result.ExecutionStats);

            if (filteringMethodInfo == null)
            {
                return couchList;
            }

            var filteringMethods = typeof(Enumerable).GetMethods()
                .Where(m => 
                    m.Name == filteringMethodInfo.Name && 
                    m.GetParameters().Length - 1 == filteringExpressions.Length)
                .OrderBy(m => m.GetParameters().Length).ToList();


            var invokeParameter = new object[filteringExpressions.Length + 1];
            invokeParameter[0] = couchList;

            bool IsRightOverload(MethodInfo m)
            {
                ParameterInfo[] parameters = m.GetParameters();
                for (var i = 0; i < filteringExpressions.Length; i++)
                {
                    var lamdaExpression = filteringExpressions[i] as UnaryExpression;
                    if (lamdaExpression == null)
                    {
                        return false;
                    }

                    if (lamdaExpression.Operand.Type != parameters[i + 1].ParameterType)
                    {
                        return false;
                    }
                    invokeParameter[i + 1] = lamdaExpression.Operand;
                }
                return true;
            }

            MethodInfo rightOverload = filteringMethods.Single(IsRightOverload);

            MethodInfo enumerableGenericFilteringMethod = rightOverload.MakeGenericMethod(typeof(T));


            var filtered = enumerableGenericFilteringMethod.Invoke(null, invokeParameter);
            return filtered;
        }
    }
}
