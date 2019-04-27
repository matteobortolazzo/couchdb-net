using CouchDB.Driver.Extensions;
using CouchDB.Driver.Settings;
using CouchDB.Driver.Types;
using Humanizer;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace CouchDB.Driver
{
    internal partial class QueryTranslator
    {
        protected override Expression VisitMember(MemberExpression m)
        {
            PropertyCaseType caseType = _settings.PropertiesCase;

            var members = new List<string> { m.Member.GetCouchPropertyName(caseType) };

            var currentExpression = m.Expression;

            while (currentExpression is MemberExpression cm)
            {
                members.Add(cm.Member.GetCouchPropertyName(caseType));
                currentExpression = cm.Expression;
            }

            members.Reverse();
            var propName = string.Join(".", members.ToArray());

            _sb.Append($"\"{propName}\"");
            return m;
        }
    }
}
