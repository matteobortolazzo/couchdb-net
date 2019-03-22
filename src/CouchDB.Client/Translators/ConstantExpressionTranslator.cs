using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace CouchDB.Client
{
    internal partial class QueryTranslator
    {
        protected override Expression VisitConstant(ConstantExpression c)
        {
            IQueryable q = c.Value as IQueryable;

            if (q != null)
            {
                // assume constant nodes w/ IQueryables are table references
                // q.ElementType.Name
            }
            else if (c.Value == null)
            {
                sb.Append("null");
            }
            else
            {
                switch (Type.GetTypeCode(c.Value.GetType()))
                {
                    case TypeCode.Boolean:
                        sb.Append(((bool)c.Value) ? "true" : "false");
                        break;
                    case TypeCode.String:
                        sb.Append($"\"{c.Value}\"");
                        break;
                    case TypeCode.Object:
                        if (c.Value is IList<string>)
                            this.VisitStringIEnumerable(c.Value as IList<string>);
                        else
                            throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", c.Value));
                        break;
                    default:
                        sb.Append(c.Value);
                        break;
                }
            }

            return c;
        }

        private void VisitStringIEnumerable(IList<string> enumerable)
        {
            if (enumerable.Count < 1)
                return;
            if (enumerable.Count == 1)
            {
                sb.Append($"\"{enumerable[0]}\"");
            }
            else
            {
                sb.Append("[");
                sb.Append(string.Join(",", enumerable.Select(e => $"\"{e}\"")));
                sb.Append("]");
            }
        }
    }
}
