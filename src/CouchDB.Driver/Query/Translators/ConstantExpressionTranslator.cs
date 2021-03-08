using Newtonsoft.Json;
using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

#pragma warning disable IDE0058 // Expression value is never used
namespace CouchDB.Driver.Query
{
    internal partial class QueryTranslator
    {
        protected override Expression VisitConstant(ConstantExpression c)
        {
            HandleConstant(c.Value);

            return c;
        }

        private void HandleConstant(object? constant)
        {
            if (constant is IQueryable)
            {
                // assume constant nodes w/ IQueryables are table references
                // q.ElementType.Name
            }
            else if (constant == null)
            {
                _sb.Append("null");
            }
            else
            {
                switch (Type.GetTypeCode(constant.GetType()))
                {
                    case TypeCode.Boolean:
                        _sb.Append(((bool)constant) ? "true" : "false");
                        break;
                    case TypeCode.String:
                        _sb.Append(JsonConvert.SerializeObject(constant));
                        break;
                    case TypeCode.DateTime:
                        _sb.Append(JsonConvert.SerializeObject(constant));
                        break;
                    case TypeCode.Object:
                        switch (constant)
                        {
                            case IEnumerable enumerable:
                                VisitIEnumerable(enumerable);
                                break;
                            case Guid _:
                                _sb.Append(JsonConvert.SerializeObject(constant));
                                break;
                            default:
                                Debug.WriteLine($"The constant for '{constant}' not officially supported.");
                                _sb.Append(JsonConvert.SerializeObject(constant));
                                break;
                        }
                        break;
                    case TypeCode.Int32:
                        _sb.Append((int)constant);
                        break;
                    default:
                        _sb.Append(constant);
                        break;
                }
            }

        }

        private void VisitIEnumerable(IEnumerable list)
        {
            _sb.Append('[');
            var needsComma = false;
            foreach (var item in list)
            {
                if (needsComma)
                {
                    _sb.Append(',');
                }
                HandleConstant(item);
                needsComma = true;
            }
            _sb.Append(']');
        }
    }
}
#pragma warning restore IDE0058 // Expression value is never used