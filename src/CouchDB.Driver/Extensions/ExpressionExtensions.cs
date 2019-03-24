using CouchDB.Driver.Extensions;
using System.Linq.Expressions;

namespace CouchDB.Driver.Helpers
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
