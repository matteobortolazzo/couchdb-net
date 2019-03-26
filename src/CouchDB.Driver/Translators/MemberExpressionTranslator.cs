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
            string GetPropertyName(MemberInfo memberInfo)
            {
                var jsonPropertyAttributes = memberInfo.GetCustomAttributes(typeof(JsonPropertyAttribute), true);
                var jsonProperty = jsonPropertyAttributes.Length > 0 ? jsonPropertyAttributes[0] as JsonPropertyAttribute : null;

                if (jsonProperty != null)
                {
                    return jsonProperty.PropertyName;
                }
                if (_settings.PropertiesCaseType != CaseType.None)
                {
                    return _settings.PropertiesCaseType.Convert(memberInfo.Name);
                }
                return memberInfo.Name;
            }

            var members = new List<string> { GetPropertyName(m.Member) };

            var currentExpression = m.Expression;

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
