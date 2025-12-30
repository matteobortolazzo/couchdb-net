using System.Linq.Expressions;
using System.Text;
using System.Threading;
using CouchDB.Driver.Options;

namespace CouchDB.Driver.Query;

internal partial class QueryTranslator : ExpressionVisitor, IQueryTranslator
{
    public bool ThrowOnQueryWarning { get; private set; }
    
    private readonly InternalCouchClientOptions _options;
    private readonly StringBuilder _sb;
    private bool _isSelectorSet;
    private readonly Lock _sbLock = new();

    internal QueryTranslator(InternalCouchClientOptions options)
    {
        _options = options;
        _sb = new StringBuilder();
    }

    public string Translate(Expression e)
    {
        lock (_sbLock)
        {
            ThrowOnQueryWarning = _options.ThrowOnQueryWarning;
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