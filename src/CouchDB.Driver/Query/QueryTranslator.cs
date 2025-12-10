using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Threading;
using CouchDB.Driver.Options;

namespace CouchDB.Driver.Query;

internal partial class QueryTranslator : ExpressionVisitor, IQueryTranslator
{
    private readonly CouchOptions _options;
    private readonly StringBuilder _sb;
    private bool _isSelectorSet;
    private readonly Lock _sbLock = new();
    private readonly JsonNamingPolicy _jsonNamePolicy;

    internal QueryTranslator(CouchOptions options)
    {
        _sb = new StringBuilder();
        _options = options;
        _jsonNamePolicy = _options.JsonSerializerOptions?.PropertyNamingPolicy ?? JsonNamingPolicy.CamelCase;
    }

    public string Translate(Expression e)
    {
        lock (_sbLock)
        {
            _isSelectorSet = false;
            _sb.Clear();
            _sb.Append('{');
            Visit(e);

            // If no Where() calls
            if (!_isSelectorSet)
            {
                // If no other methods calls - ToList()
                if (_sb.Length > 1)
                {
                    _sb.Length--;
                    _sb.Append(',');
                }

                _sb.Append("\"selector\":{}");
            }
            else
            {
                _sb.Length--;
            }

            _sb.Append('}');
            var body = _sb.ToString();
            return body;
        }
    }

    protected override Expression VisitLambda<T>(Expression<T> l)
    {
        Visit(l.Body);
        return l;
    }
}