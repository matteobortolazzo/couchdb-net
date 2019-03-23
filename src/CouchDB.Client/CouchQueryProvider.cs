using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace CouchDB.Client
{
    public class CouchQueryProvider : QueryProvider
    {
        CouchConnection connection;

        public CouchQueryProvider(CouchConnection connection)
        {
            this.connection = connection;
        }

        public override string GetQueryText(Expression expression)
        {
            var body = this.Translate(expression).Body;            
            return body;
        }

        public override object Execute(Expression expression)
        {
            var cmd = this.connection.CreateCommand();

            cmd.Request = this.Translate(expression);

            Type elementType = TypeSystem.GetElementType(expression.Type);
            MethodInfo method = typeof(CouchCommand).GetMethod("ExecuteReader");
            MethodInfo generic = method.MakeGenericMethod(elementType);
            return generic.Invoke(cmd, null);
        }

        private MangoQuery Translate(Expression expression)
        {
            expression = Evaluator.PartialEval(expression);
            return new QueryTranslator().Translate(expression);
        }
    }
}
