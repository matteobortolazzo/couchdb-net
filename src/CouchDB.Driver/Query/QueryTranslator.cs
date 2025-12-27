using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace CouchDB.Driver.Query;

internal partial class QueryTranslator : ExpressionVisitor, IQueryTranslator
{
    private readonly StringBuilder _sb;
    private bool _isSelectorSet;
    private readonly Lock _sbLock = new();
    private readonly JsonNamingPolicy? _jsonNamePolicy;

    internal QueryTranslator(JsonSerializerOptions jsonSerializerOptions)
    {
        _sb = new StringBuilder();
        _jsonNamePolicy = jsonSerializerOptions.PropertyNamingPolicy;
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