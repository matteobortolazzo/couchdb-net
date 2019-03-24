using System.Linq.Expressions;
using System.Net.Http;
using System.Text;

namespace CouchDB.Client
{
    internal partial class QueryTranslator : ExpressionVisitor
    {
        private StringBuilder sb;
        private bool isSelectorSet;

        internal QueryTranslator() { }
        internal string Translate(Expression expression)
        {
            this.sb = new StringBuilder();
            sb.Append("{");
            this.Visit(expression);

            // If no Where() calls
            if (!isSelectorSet)
            {
                // If no other methods calls - ToList()
                if (sb.Length > 1)
                {
                    sb.Length--;
                    sb.Append(",");
                }
                sb.Append("\"selector\":{}");
            }

            sb.Append("}");
            var body = sb.ToString();
            return body;
        }

        protected override Expression VisitLambda<T>(Expression<T> l)
        {
            this.Visit(l.Body);
            return l;
        }
    }
}
