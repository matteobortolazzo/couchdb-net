using CouchDB.Client.Helpers;
using CouchDB.Client.Types;
using Flurl.Http;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace CouchDB.Client
{
    internal class CouchQueryProvider : QueryProvider
    {
        private readonly FlurlClient flurlClient;
        private readonly string connectionString;
        private readonly string db;

        public CouchQueryProvider(FlurlClient flurlClient, string connectionString, string db)
        {
            this.flurlClient = flurlClient;
            this.connectionString = connectionString;
            this.db = db;
        }

        public override string GetQueryText(Expression expression)
        {
            return this.Translate(expression);
        }

        public override object Execute(Expression e)
        {
            var request = this.Translate(e);
            var elementType = TypeSystem.GetElementType(e.Type);
            MethodInfo method = typeof(CouchQueryProvider).GetMethod("SendRequest");
            MethodInfo generic = method.MakeGenericMethod(elementType);
            return generic.Invoke(this, new[] { request });
        }

        private string Translate(Expression expression)
        {
            expression = Evaluator.PartialEval(expression);
            return new QueryTranslator(db).Translate(expression);
        }

        public IEnumerable<T> SendRequest<T>(string body)
        {
            var result = flurlClient
                .Request(connectionString)
                .AppendPathSegments(db, "_find")
                .WithHeader("Content-Type", "application/json")
                .PostStringAsync(body).ReceiveJson<FindResult<T>>()
                .SendRequest();
            return result.Docs;
        }
    }
}
