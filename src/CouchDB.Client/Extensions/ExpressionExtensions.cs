using CouchDB.Client.Extensions;
using Humanizer;
using Newtonsoft.Json;
using System.Linq.Expressions;

namespace CouchDB.Client.Helpers
{
    internal static class ExpressionExtensions
    {
        public static string GetTypeName(this Expression e)
        {
            var elementType = TypeSystem.GetElementType(e.Type);
            var jsonObjectAttributes = elementType.GetCustomAttributes(typeof(JsonObjectAttribute), true);
            var jsonObject = jsonObjectAttributes.Length > 0 ? jsonObjectAttributes[0] as JsonObjectAttribute : null;

            if (jsonObject != null)
                return jsonObject.Id;

            var typeName = elementType.Name.ToCamelCase();
            return typeName.Pluralize();
        }
    }
}
