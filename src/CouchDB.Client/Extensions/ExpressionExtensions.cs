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
            return elementType.GetName();
        }
    }
}
