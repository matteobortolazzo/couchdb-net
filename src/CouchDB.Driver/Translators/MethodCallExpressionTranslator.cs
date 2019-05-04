using CouchDB.Driver.Extensions;
using CouchDB.Driver.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

#pragma warning disable IDE0058 // Expression value is never used
namespace CouchDB.Driver
{
    internal partial class QueryTranslator
    {
        internal static List<string> NativeQueryableMethods { get; } = new List<string>
        {
            "Where",
            "OrderBy", "ThenByWhere",
            "OrderByDescending", "ThenByDescending",
            "Skip",
            "Take",
            "Select"
        };

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }
            return e;
        }
        protected override Expression VisitExtension(Expression x)
        {
            return x;
        }
        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(Queryable))
            {
                if (m.Method.Name == "Where")
                {
                    return VisitWhereMethod(m);
                }
                else if (m.Method.Name == "OrderBy" || m.Method.Name == "ThenBy")
                {
                    return VisitOrderAscendingMethod(m);
                }
                else if (m.Method.Name == "OrderByDescending" || m.Method.Name == "ThenByDescending")
                {
                    return VisitOrderDescendingMethod(m);
                }
                else if (m.Method.Name == "Skip")
                {
                    return VisitSkipMethod(m);
                }
                else if (m.Method.Name == "Take")
                {
                    return VisitTakeMethod(m);
                }
                else if (m.Method.Name == "Select")
                {
                    return VisitSelectMethod(m);
                }
            }
            else if (m.Method.DeclaringType == typeof(Enumerable))
            {
                if (m.Method.Name == "Any")
                {
                    return VisitAnyMethod(m);
                }
                else if (m.Method.Name == "All")
                {
                    return VisitAllMethod(m);
                }
            }
            else if (m.Method.DeclaringType == typeof(QueryableExtensions))
            {
                if (m.Method.Name == "UseBookmark")
                {
                    return VisitUseBookmarkMethod(m);
                }
                else if (m.Method.Name == "WithReadQuorum")
                {
                    return VisitWithQuorumMethod(m);
                }
                else if (m.Method.Name == "WithoutIndexUpdate")
                {
                    return VisitWithoutIndexUpdateMethod(m);
                }
                else if (m.Method.Name == "FromStable")
                {
                    return VisitFromStableMethod(m);
                }
                else if (m.Method.Name == "UseIndex")
                {
                    return VisitUseIndexMethod(m);
                }
                else if (m.Method.Name == "IncludeExecutionStats")
                {
                    return VisitIncludeExecutionStatsMethod(m);
                }
                else if (m.Method.Name == nameof(QueryableExtensions.IncludeConflicts))
                {
                    return VisitIncludeConflictsMethod(m);
                }
            }
            else if (m.Method.DeclaringType == typeof(EnumerableExtensions))
            {
                if (m.Method.Name == "Contains")
                {
                    return VisitEnumarableContains(m);
                }
            }
            else if (m.Method.DeclaringType == typeof(ObjectExtensions))
            {
                if (m.Method.Name == "FieldExists")
                {
                    return VisitFieldExistsMethod(m);
                }
                else if (m.Method.Name == "IsCouchType")
                {
                    return VisitIsCouchTypeMethod(m);
                }
                else if (m.Method.Name == "In")
                {
                    return VisitInMethod(m);
                }
            }
            else if (m.Method.DeclaringType == typeof(StringExtensions))
            {
                if (m.Method.Name == "IsMatch")
                {
                    return VisitIsMatchMethod(m);
                }
            }
            else
            {
                if (m.Method.Name == "Contains")
                {
                    return VisitContainsMethod(m);
                }
            }

            throw new NotSupportedException($"The method '{m.Method.Name}' is not supported");
        }

        #region Queryable

        private Expression VisitWhereMethod(MethodCallExpression m)
        {
            Visit(m.Arguments[0]);
            _sb.Append("\"selector\":");
            var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
            Visit(lambda.Body);
            _sb.Append(",");
            _isSelectorSet = true;
            return m;
        }
        private Expression VisitOrderAscendingMethod(MethodCallExpression m)
        {
            void InspectOrdering(Expression e)
            {
                var o = e as MethodCallExpression;
                var lambda = (LambdaExpression)StripQuotes(o.Arguments[1]);

                if (o.Method.Name == "OrderBy")
                {
                    Visit(o.Arguments[0]);
                    _sb.Append("\"sort\":[");
                    Visit(lambda.Body);
                }
                else if (o.Method.Name == "OrderByDescending")
                {
                    throw new InvalidOperationException("Cannot order in different directions.");
                }
                else if (o.Method.Name == "ThenBy")
                {
                    InspectOrdering(o.Arguments[0]);
                    Visit(lambda.Body);
                }
                else
                {
                    return;
                }

                _sb.Append(",");
            }

            InspectOrdering(m);
            _sb.Length--;
            _sb.Append("],");
            return m;
        }
        private Expression VisitOrderDescendingMethod(MethodCallExpression m)
        {
            void InspectOrdering(Expression e)
            {
                var o = e as MethodCallExpression;
                var lambda = (LambdaExpression)StripQuotes(o.Arguments[1]);

                if (o.Method.Name == "OrderBy")
                {
                    throw new InvalidOperationException("Cannot order in different directions.");
                }
                else if (o.Method.Name == "OrderByDescending")
                {
                    Visit(o.Arguments[0]);
                    _sb.Append("\"sort\":[");
                    _sb.Append("{");
                    Visit(lambda.Body);
                    _sb.Append(":\"desc\"}");
                }
                else if (o.Method.Name == "ThenByDescending")
                {
                    InspectOrdering(o.Arguments[0]);
                    _sb.Append("{");
                    Visit(lambda.Body);
                    _sb.Append(":\"desc\"}");
                }
                else
                {
                    return;
                }

                _sb.Append(",");
            }

            InspectOrdering(m);
            _sb.Length--;
            _sb.Append("],");
            return m;
        }
        private Expression VisitSkipMethod(MethodCallExpression m)
        {
            Visit(m.Arguments[0]);
            _sb.Append($"\"skip\":{m.Arguments[1]},");
            return m;
        }
        private Expression VisitTakeMethod(MethodCallExpression m)
        {
            Visit(m.Arguments[0]);
            _sb.Append($"\"limit\":{m.Arguments[1]},");
            return m;
        }
        private Expression VisitSelectMethod(MethodCallExpression m)
        {
            Visit(m.Arguments[0]);
            _sb.Append("\"fields\":[");
            var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);

            if (!(lambda.Body is NewExpression n))
            {
                throw new NotSupportedException($"The expression of type {lambda.Body.GetType()} is not supported in the Select method.");
            }

            foreach (Expression a in n.Arguments)
            {
                Visit(a);
                _sb.Append(",");
            }
            _sb.Length--;
            _sb.Append("],");

            return m;
        }

        #endregion

        #region Enumerable

        private Expression VisitAnyMethod(MethodCallExpression m)
        {
            _sb.Append("{");
            Visit(m.Arguments[0]);
            _sb.Append(":{\"$elemMatch\":");
            var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
            Visit(lambda.Body);
            _sb.Append("}}");
            return m;
        }
        private Expression VisitAllMethod(MethodCallExpression m)
        {
            _sb.Append("{");
            Visit(m.Arguments[0]);
            _sb.Append(":{\"$allMatch\":");
            var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
            Visit(lambda.Body);
            _sb.Append("}}");
            return m;
        }

        #endregion

        #region QueryableExtensions

        private Expression VisitUseBookmarkMethod(MethodCallExpression m)
        {
            Visit(m.Arguments[0]);
            _sb.Append("\"bookmark\":");
            Visit(m.Arguments[1]);
            _sb.Append(",");
            return m;
        }
        private Expression VisitWithQuorumMethod(MethodCallExpression m)
        {
            Visit(m.Arguments[0]);
            _sb.Append("\"r\":");
            Visit(m.Arguments[1]);
            _sb.Append(",");
            return m;
        }
        private Expression VisitWithoutIndexUpdateMethod(MethodCallExpression m)
        {
            Visit(m.Arguments[0]);
            _sb.Append("\"update\":false");
            _sb.Append(",");
            return m;
        }
        private Expression VisitFromStableMethod(MethodCallExpression m)
        {
            Visit(m.Arguments[0]);
            _sb.Append("\"stable\":true");
            _sb.Append(",");
            return m;
        }
        private Expression VisitUseIndexMethod(MethodCallExpression m)
        {
            Visit(m.Arguments[0]);
            _sb.Append("\"use_index\":");
            if (!(m.Arguments[1] is ConstantExpression indexArgsExpression))
            {
                throw new ArgumentException("UseIndex requires an IList<string> argument");
            }

            if (!(indexArgsExpression.Value is IList<string> indexArgs))
            {
                throw new ArgumentException("UseIndex requires an IList<string> argument");
            }
            else if (indexArgs.Count == 1)
            {
                // use_index expects the value with [ or ] when it's a single item array
                Visit(Expression.Constant(indexArgs[0]));
            }
            else if (indexArgs.Count == 2)
            {
                Visit(indexArgsExpression);
            }
            else
            {
                throw new ArgumentException("UseIndex requires 1 or 2 strings");
            }

            _sb.Append(",");
            return m;
        }
        public Expression VisitIncludeExecutionStatsMethod(MethodCallExpression m)
        {
            Visit(m.Arguments[0]);
            _sb.Append("\"execution_stats\":true");
            _sb.Append(",");
            return m;
        }
        public Expression VisitIncludeConflictsMethod(MethodCallExpression m)
        {
            Visit(m.Arguments[0]);
            _sb.Append("\"conflicts\":true");
            _sb.Append(",");
            return m;
        }

        #endregion

        #region EnumerableExtensions

        private Expression VisitEnumarableContains(MethodCallExpression m)
        {
            _sb.Append("{");
            Visit(m.Arguments[0]);
            _sb.Append(":{\"$all\":");
            Visit(m.Arguments[1]);
            _sb.Append("}}");
            return m;
        }
        private Expression VisitInMethod(MethodCallExpression m, bool not = false)
        {
            _sb.Append("{");
            Visit(m.Arguments[0]);
            if (not)
            {
                _sb.Append(":{\"$nin\":");
            }
            else
            {
                _sb.Append(":{\"$in\":");
            }

            Visit(m.Arguments[1]);
            _sb.Append("}}");
            return m;
        }

        #endregion

        #region ObjectExtensions

        private Expression VisitFieldExistsMethod(MethodCallExpression m)
        {
            _sb.Append("{");
            Visit(m.Arguments[1]);
            _sb.Append(":{\"$exists\":true");
            _sb.Append("}}");
            return m;
        }
        private Expression VisitIsCouchTypeMethod(MethodCallExpression m)
        {
            _sb.Append("{");
            Visit(m.Arguments[0]);
            _sb.Append(":{\"$type\":");
            var cExpression = m.Arguments[1] as ConstantExpression;
            var couchType = cExpression.Value as CouchType;
            _sb.Append($"\"{couchType.Value}\"");
            _sb.Append("}}");
            return m;
        }

        #endregion

        #region StringExtensions

        private Expression VisitIsMatchMethod(MethodCallExpression m)
        {
            _sb.Append("{");
            Visit(m.Arguments[0]);
            _sb.Append(":{\"$regex\":");
            Visit(m.Arguments[1]);
            _sb.Append("}}");
            return m;
        }

        #endregion

        #region Other

        private Expression VisitContainsMethod(MethodCallExpression m)
        {
            _sb.Append("{");
            Visit(m.Object);
            _sb.Append(":{\"$all\":[");
            Visit(m.Arguments[0]);
            _sb.Append("]}}");
            return m;
        }

        #endregion
    }
}
#pragma warning restore IDE0058 // Expression value is never used
