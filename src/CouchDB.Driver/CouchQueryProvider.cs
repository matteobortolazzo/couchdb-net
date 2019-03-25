using CouchDB.Driver.Helpers;
using CouchDB.Driver.Types;
using Flurl.Http;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace CouchDB.Driver
{
    internal class CouchQueryProvider : QueryProvider
    {
        private readonly FlurlClient _flurlClient;
        private readonly string _connectionString;
        private readonly string _db;

        public CouchQueryProvider(FlurlClient flurlClient, string connectionString, string db)
        {
            _flurlClient = flurlClient;
            _connectionString = connectionString;
            _db = db;
        }

        public override string GetQueryText(Expression expression)
        {
            return Translate(expression);
        }

        public override object Execute(Expression e)
        {
            var request = Translate(e);
            var elementType = TypeSystem.GetElementType(e.Type);
            MethodInfo method = typeof(CouchQueryProvider).GetMethod("SendRequest");
            MethodInfo generic = method.MakeGenericMethod(elementType);
            return generic.Invoke(this, new[] { request });
        }

        private string Translate(Expression expression)
        {
            expression = Evaluator.PartialEval(expression);
            return new QueryTranslator().Translate(expression);
        }

        public IEnumerable<T> SendRequest<T>(string body)
        {
            var result = _flurlClient
                .Request(_connectionString)
                .AppendPathSegments(_db, "_find")
                .WithHeader("Content-Type", "application/json")
                .PostStringAsync(body).ReceiveJson<FindResult<T>>()
                .SendRequest();
            return result.Docs;
        }
    }
}
