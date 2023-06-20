using System.Linq.Expressions;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.Types;

namespace CouchDB.Driver.Query
{
    internal partial class QueryTranslator
    {
        protected override Expression VisitMember(MemberExpression m)
        {
            // Override database split if needed
            if (m.Member.DeclaringType == typeof(CouchDocument) &&
                m.Member.Name == nameof(CouchDocument.SplitDiscriminator) &&
                !string.IsNullOrWhiteSpace(_options.DatabaseSplitDiscriminator))
            {
                _sb.Append($"\"{_options.DatabaseSplitDiscriminator}\"");
                return m;
            }
            
            var propName = m.GetPropertyName(_options);
            _sb.Append($"\"{propName}\"");
            return m;
        }
    }
}