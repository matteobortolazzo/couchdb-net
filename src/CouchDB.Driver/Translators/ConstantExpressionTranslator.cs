using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace CouchDB.Driver
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
                _sb.Append("null");
            }
            else
            {
                switch (Type.GetTypeCode(c.Value.GetType()))
                {
                    case TypeCode.Boolean:
                        _sb.Append(((bool)c.Value) ? "true" : "false");
                        break;
                    case TypeCode.String:
                        _sb.Append($"\"{c.Value}\"");
                        break;
                    case TypeCode.Object:
                        if (c.Value is IList<bool>)
                            VisitIEnumerable(c.Value as IList<bool>);
                        else if (c.Value is IList<int>)
                            VisitIEnumerable(c.Value as IList<int>);
                        else if (c.Value is IList<long>)
                            VisitIEnumerable(c.Value as IList<long>);
                        else if(c.Value is IList<decimal>)
                            VisitIEnumerable(c.Value as IList<decimal>);
                        else if (c.Value is IList<float>)
                            VisitIEnumerable(c.Value as IList<float>);
                        else if (c.Value is IList<double>)
                            VisitIEnumerable(c.Value as IList<double>);
                        else if(c.Value is IList<string>)
                            VisitIEnumerable(c.Value as IList<string>);
                        else
                            throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", c.Value));
                        break;
                    default:
                        _sb.Append(c.Value);
                        break;
                }
            }

            return c;
        }

        private void VisitIEnumerable<T>(IList<T> list)
        {
            if (list.Count < 1)
                return;
            if (list.Count == 1)
            {
                _sb.Append(VisitConst(list[0]));
            }
            else
            {
                _sb.Append("[");
                _sb.Append(string.Join(",", list.Select(e => VisitConst(e))));
                _sb.Append("]");
            }

            string VisitConst(object o)
            {
                switch (Type.GetTypeCode(o.GetType()))
                {
                    case TypeCode.Boolean:
                        return (bool)o ? "true" : "false";
                    case TypeCode.String:
                        return $"\"{o}\"";
                    case TypeCode.Object:
                        throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", o));                        
                    default:
                        return o.ToString();
                }
            }
        }
    }
}
