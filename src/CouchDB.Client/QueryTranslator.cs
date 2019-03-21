using System.Linq.Expressions;
using System.Net.Http;
using System.Text;

namespace CouchDB.Client
{
    internal partial class QueryTranslator : ExpressionVisitor
    {
        private string path;
        private HttpMethod method;
        private StringBuilder sb;

        internal QueryTranslator() { }
        internal TranslatedRequest Translate(Expression expression)
        {
            this.sb = new StringBuilder();
            this.Visit(expression);
            var result = sb.ToString();
            return new TranslatedRequest();
        }
    }
}
