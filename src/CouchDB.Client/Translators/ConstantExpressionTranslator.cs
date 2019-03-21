using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
                        throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", c.Value));
                    default:
                        sb.Append(c.Value);
                        break;
                }
            }

            return c;
        }
    }
}
