using CouchDB.Driver.Extensions;
using CouchDB.Driver.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Authentication;

#pragma warning disable IDE0058 // Expression value is never used
namespace CouchDB.Driver
{
    internal partial class QueryTranslator
    {
        internal static List<string> NativeQueryableMethods { get; } = new List<string>
        {
            nameof(Queryable.Where),
            nameof(Queryable.OrderBy),
            nameof(Queryable.ThenBy),
            nameof(Queryable.OrderByDescending),
            nameof(Queryable.ThenByDescending),
            nameof(Queryable.Skip),
            nameof(Queryable.Take),
            nameof(Queryable.Select)
        };

        internal static List<string> CompositeQueryableMethods { get; } = new List<string>
        {
            nameof(Queryable.Max),
            nameof(Queryable.Min),
            nameof(Queryable.Any),
            nameof(Queryable.All),
            nameof(Queryable.First),
            nameof(Queryable.FirstOrDefault),
            nameof(Queryable.Single),
            nameof(Queryable.SingleOrDefault),
            nameof(Queryable.Last),
            nameof(Queryable.LastOrDefault)
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
                switch (m.Method.Name)
                {
                    case "Where":
                        return VisitWhereMethod(m);
                    case "OrderBy":
                    case "ThenBy":
                        return VisitOrderAscendingMethod(m);
                    case "OrderByDescending":
                    case "ThenByDescending":
                        return VisitOrderDescendingMethod(m);
                    case "Skip":
                        return VisitSkipMethod(m);
                    case "Take":
                        return VisitTakeMethod(m);
                    case "Select":
                        return VisitSelectMethod(m);
                }
            }
            else if (m.Method.DeclaringType == typeof(Enumerable))
            {
                switch (m.Method.Name)
                {
                    case "Any":
                        return VisitAnyMethod(m);
                    case "All":
                        return VisitAllMethod(m);
                }
            }
            else if (m.Method.DeclaringType == typeof(QueryableExtensions))
            {
                switch (m.Method.Name)
                {
                    case "UseBookmark":
                        return VisitUseBookmarkMethod(m);
                    case "WithReadQuorum":
                        return VisitWithQuorumMethod(m);
                    case "WithoutIndexUpdate":
                        return VisitWithoutIndexUpdateMethod(m);
                    case "FromStable":
                        return VisitFromStableMethod(m);
                    case "UseIndex":
                        return VisitUseIndexMethod(m);
                    case "IncludeExecutionStats":
                        return VisitIncludeExecutionStatsMethod(m);
                    case "IncludeConflicts":
                        return VisitIncludeConflictsMethod(m);
                }
            }
            else if (m.Method.DeclaringType == typeof(EnumerableExtensions))
            {
                switch (m.Method.Name)
                {
                    case "Contains":
                        return VisitEnumerableContains(m);
                }
            }
            else if (m.Method.DeclaringType == typeof(ObjectExtensions))
            {
                switch (m.Method.Name)
                {
                    case "FieldExists":
                        return VisitFieldExistsMethod(m);
                    case "IsCouchType":
                        return VisitIsCouchTypeMethod(m);
                    case "In":
                        return VisitInMethod(m);
                }
            }
            else if (m.Method.DeclaringType == typeof(StringExtensions))
            {
                switch (m.Method.Name)
                {
                    case "IsMatch":
                        return VisitIsMatchMethod(m);
                }
            }
            else
            {
                switch (m.Method.Name)
                {
                    case "Contains":
                        return VisitContainsMethod(m);
                }
            }

            if (CompositeQueryableMethods.Contains(m.Method.Name))
            {
                return Visit(m.Arguments[0]);
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
                MethodCallExpression o = e as MethodCallExpression ?? throw new AuthenticationException($"Invalid expression type {e.GetType().Name}");
                var lambda = (LambdaExpression)StripQuotes(o.Arguments[1]);

                switch (o.Method.Name)
                {
                    case "OrderBy":
                        Visit(o.Arguments[0]);
                        _sb.Append("\"sort\":[");
                        Visit(lambda.Body);
                        break;
                    case "OrderByDescending":
                        throw new InvalidOperationException("Cannot order in different directions.");
                    case "ThenBy":
                        InspectOrdering(o.Arguments[0]);
                        Visit(lambda.Body);
                        break;
                    default:
                        return;
                }

                _sb.Append(",");
            }

            InspectOrdering(m);
            _sb.Length--;
            _sb.Append("],");
            return m;
        }
        private Expression VisitOrderDescendingMethod(Expression m)
        {
            void InspectOrdering(Expression e)
            {
                MethodCallExpression o = e as MethodCallExpression?? throw new AuthenticationException($"Invalid expression type {e.GetType().Name}");
                var lambda = (LambdaExpression)StripQuotes(o.Arguments[1]);

                switch (o.Method.Name)
                {
                    case "OrderBy":
                        throw new InvalidOperationException("Cannot order in different directions.");
                    case "OrderByDescending":
                        Visit(o.Arguments[0]);
                        _sb.Append("\"sort\":[{");
                        Visit(lambda.Body);
                        _sb.Append(":\"desc\"}");
                        break;
                    case "ThenByDescending":
                        InspectOrdering(o.Arguments[0]);
                        _sb.Append("{");
                        Visit(lambda.Body);
                        _sb.Append(":\"desc\"}");
                        break;
                    default:
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

            if (lambda.Body is NewExpression n)
            {
                foreach (Expression a in n.Arguments)
                {
                    Visit(a);
                    _sb.Append(",");
                }
                _sb.Length--;
            }
            else
            {
                throw new NotSupportedException($"The expression of type {lambda.Body.GetType()} is not supported in the Select method.");
            }

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

            switch (indexArgs.Count)
            {
                case 1:
                    // use_index expects the value with [ or ] when it's a single item array
                    Visit(Expression.Constant(indexArgs[0]));
                    break;
                case 2:
                    Visit(indexArgsExpression);
                    break;
                default:
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

        private Expression VisitEnumerableContains(MethodCallExpression m)
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
            _sb.Append(not ? ":{\"$nin\":" : ":{\"$in\":");

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
            ConstantExpression cExpression = m.Arguments[1] as ConstantExpression ?? throw new ArgumentException("Argument is not of type ConstantExpression.");
            CouchType couchType = cExpression.Value as CouchType ?? throw new ArgumentException("Argument is not of type CouchType.");
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
