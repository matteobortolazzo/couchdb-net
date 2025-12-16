using System;
using System.Collections;
using System.Diagnostics;

using System.Linq.Expressions;
using System.Text.Json;

namespace CouchDB.Driver.Query;

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
            // assume constant nodes w/ IQueryable are table references
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
                case TypeCode.DateTime:
                    _sb.Append(JsonSerializer.Serialize(constant));
                    break;
                case TypeCode.Object:
                    switch (constant)
                    {
                        case IEnumerable enumerable:
                            VisitIEnumerable(enumerable);
                            break;
                        case Guid _:
                            _sb.Append(JsonSerializer.Serialize(constant));
                            break;
                        default:
                            Debug.WriteLine($"The constant for '{constant}' not officially supported.");
                            _sb.Append(JsonSerializer.Serialize(constant));
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