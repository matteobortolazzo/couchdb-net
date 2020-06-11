using CouchDB.Driver.ExpressionVisitors;
using CouchDB.Driver.Settings;
using System;
using System.Linq.Expressions;
using System.Text;

namespace CouchDB.Driver
{
    internal partial class QueryTranslator : ExpressionVisitor
    {
        private readonly CouchSettings _settings;
        private readonly StringBuilder _sb;
        private bool _isSelectorSet;

        internal QueryTranslator(CouchSettings settings)
        {
            _sb = new StringBuilder();
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        internal string Translate(Expression e)
        {
            e = Local.PartialEval(e);

            var whereVisitor = new BoolMemberToConstantEvaluator();
            e = whereVisitor.Visit(e);

            var preTranslator = new QueryPreTranslator();
            e = preTranslator.Visit(e);

            _sb.Clear();
            _sb.Append("{");
            Visit(e);
            
            // If no Where() calls
            if (!_isSelectorSet)
            {
                // If no other methods calls - ToList()
                if (_sb.Length > 1)
                {
                    _sb.Length--;
                    _sb.Append(",");
                }
                _sb.Append("\"selector\":{}");
            }
            else
            {
                _sb.Length--;
            }

            _sb.Append("}");
            var body = _sb.ToString();
            return body;
        }

        protected override Expression VisitLambda<T>(Expression<T> l)
        {
            Visit(l.Body);
            return l;
        }
    }
}
