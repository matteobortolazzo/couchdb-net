using CouchDB.Client.Helpers;
using Flurl.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

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
            var request = this.Translate(expression);
            return request.Body;
        }

        public override object Execute(Expression expression)
        {
            var request = this.Translate(expression);
            // TODO
            return null;
        }

        private CouchRequest Translate(Expression expression)
        {
            expression = Evaluator.PartialEval(expression);
            return new QueryTranslator(db).Translate(expression);
        }
    }
}
