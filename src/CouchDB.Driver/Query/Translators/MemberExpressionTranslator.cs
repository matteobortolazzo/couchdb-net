using System.Linq.Expressions;
using CouchDB.Driver.Extensions;

namespace CouchDB.Driver.Query
{
    internal partial class QueryTranslator
    {
        protected override Expression VisitMember(MemberExpression m)
        {
            var propName = m.GetPropertyName(_options);

            _sb.Append($"\"{propName}\"");
            return m;
        }
    }
}