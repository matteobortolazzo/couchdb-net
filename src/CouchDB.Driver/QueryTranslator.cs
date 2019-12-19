﻿using CouchDB.Driver.Settings;
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

#pragma warning disable IDE0058 // Expression value is never used
namespace CouchDB.Driver
{
    internal partial class QueryTranslator : ExpressionVisitor
    {
        private readonly CouchSettings _settings;
        private StringBuilder _sb;
        private bool _isSelectorSet;

        internal QueryTranslator(CouchSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }
        internal string Translate(Expression expression)
        {
            _sb = new StringBuilder();
            _sb.Append("{");
            Visit(expression);

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
#pragma warning restore IDE0058 // Expression value is never used
