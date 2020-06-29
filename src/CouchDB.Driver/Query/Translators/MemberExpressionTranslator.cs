using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace CouchDB.Driver.Query
{
    internal partial class QueryTranslator
    {
        protected override Expression VisitMember(MemberExpression m)
        {
            string GetPropertyName(MemberInfo memberInfo)
            {
                var jsonPropertyAttributes = memberInfo.GetCustomAttributes(typeof(JsonPropertyAttribute), true);
                JsonPropertyAttribute? jsonProperty = jsonPropertyAttributes.Length > 0
                    ? jsonPropertyAttributes[0] as JsonPropertyAttribute
                    : null;

                return jsonProperty != null
                    ? jsonProperty.PropertyName
                    : _settings.PropertiesCase.Convert(memberInfo.Name);
            }

            var members = new List<string> { GetPropertyName(m.Member) };

            Expression currentExpression = m.Expression;

            while (currentExpression is MemberExpression cm)
            {
                members.Add(GetPropertyName(cm.Member));
                currentExpression = cm.Expression;
            }

            members.Reverse();
            var propName = string.Join(".", members.ToArray());

            _sb.Append($"\"{propName}\"");
            return m;
        }
    }
}