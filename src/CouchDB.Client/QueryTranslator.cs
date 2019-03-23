using System.Linq.Expressions;
using System.Net.Http;
using System.Text;

namespace CouchDB.Client
{
    internal partial class QueryTranslator : ExpressionVisitor
    {
        private string db;
        private StringBuilder sb;

        internal QueryTranslator(string db)
        {
            this.db = db;
        }
        internal string Translate(Expression expression)
        {
            this.sb = new StringBuilder();
            sb.Append("{");
            this.Visit(expression);
            sb.Append("}");
            var body = sb.ToString();
            if (body[body.Length - 2] == ',')
                body = body.Remove(body.Length - 2, 1);
            return body;
        }

        protected override Expression VisitLambda<T>(Expression<T> l)
        {
            this.Visit(l.Body);
            return l;
        }
    }
}
