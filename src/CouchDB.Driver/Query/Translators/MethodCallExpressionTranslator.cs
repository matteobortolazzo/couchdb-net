using CouchDB.Driver.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Authentication;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.Shared;

#pragma warning disable IDE0058 // Expression value is never used
namespace CouchDB.Driver.Query
{
    internal partial class QueryTranslator
    {
        protected override Expression VisitExtension(Expression x)
        {
            return x;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (QueryableMethods.IsAverageWithoutSelector(m.Method) ||
                QueryableMethods.IsSumWithoutSelector(m.Method))
            {
                return Visit(m.Arguments[0]);
            }

            MethodInfo? genericDefinition = m.Method.IsGenericMethod
                ? m.Method.GetGenericMethodDefinition()
                : null;

            if (genericDefinition != null && genericDefinition.IsSupportedByComposition())
            {
                return Visit(m.Arguments[0]);
            }

            // Queryable

            if (genericDefinition == QueryableMethods.Where)
            {
                return VisitWhereMethod(m);
            }

            if (genericDefinition == QueryableMethods.OrderBy || genericDefinition == QueryableMethods.ThenBy)
            {
                return VisitOrderAscendingMethod(m);
            }

            if (genericDefinition == QueryableMethods.OrderByDescending || genericDefinition == QueryableMethods.ThenByDescending)
            {
                return VisitOrderDescendingMethod(m);
            }

            if (genericDefinition == QueryableMethods.Skip)
            {
                return VisitSkipMethod(m);
            }

            if (genericDefinition == QueryableMethods.Take)
            {
                return VisitTakeMethod(m);
            }

            if (genericDefinition == QueryableMethods.Select)
            {
                return VisitSelectMethod(m);
            }

            // Enumerable

            if (m.Method == SupportedQueryMethods.Contains)
            {
                return VisitContainsMethod(m);
            }

            // IQueryable extensions

            if (genericDefinition == SupportedQueryMethods.AnyWithPredicate)
            {
                return VisitAnyMethod(m);
            }

            if (genericDefinition == SupportedQueryMethods.All)
            {
                return VisitAllMethod(m);
            }

            if (genericDefinition == SupportedQueryMethods.UseBookmark)
            {
                return VisitUseBookmarkMethod(m);
            }

            if (genericDefinition == SupportedQueryMethods.WithReadQuorum)
            {
                return VisitWithQuorumMethod(m);
            }

            if (genericDefinition == SupportedQueryMethods.WithoutIndexUpdate)
            {
                return VisitWithoutIndexUpdateMethod(m);
            }

            if (genericDefinition == SupportedQueryMethods.FromStable)
            {
                return VisitFromStableMethod(m);
            }

            if (genericDefinition == SupportedQueryMethods.UseIndex)
            {
                return VisitUseIndexMethod(m);
            }

            if (genericDefinition == SupportedQueryMethods.IncludeExecutionStats)
            {
                return VisitIncludeExecutionStatsMethod(m);
            }

            if (genericDefinition == SupportedQueryMethods.IncludeConflicts)
            {
                return VisitIncludeConflictsMethod(m);
            }

            if (genericDefinition == SupportedQueryMethods.Select)
            {
                return VisitSelectFieldMethod(m);
            }

            if (genericDefinition == SupportedQueryMethods.Convert)
            {
                return VisitConvertMethod(m);
            }

            // IEnumerable extensions

            if (genericDefinition == SupportedQueryMethods.EnumerableContains)
            {
                return VisitEnumerableContains(m);
            }

            // Object extensions

            if (genericDefinition == SupportedQueryMethods.FieldExists)
            {
                return VisitFieldExistsMethod(m);
            }

            if (genericDefinition == SupportedQueryMethods.IsCouchType)
            {
                return VisitIsCouchTypeMethod(m);
            }

            if (genericDefinition == SupportedQueryMethods.In)
            {
                return VisitInMethod(m);
            }

            // String extensions

            if (m.Method == SupportedQueryMethods.IsMatch)
            {
                return VisitIsMatchMethod(m);
            }

            // List 
            if (m.Method.Name == nameof(List<object>.Contains) && m.Method.DeclaringType.IsGenericType && m.Method.DeclaringType.GetGenericTypeDefinition() == typeof(List<>))
            {
                return VisitContainsMethod(m);
            }

            throw new NotSupportedException($"The method '{m.Method.Name}' is not supported");
        }

        #region Queryable

        private Expression VisitWhereMethod(MethodCallExpression m)
        {
            Visit(m.Arguments[0]);
            _sb.Append("\"selector\":");
            Expression lambdaBody = m.GetLambdaBody();
            Visit(lambdaBody);
            _sb.Append(',');
            _isSelectorSet = true;
            return m;
        }
        private Expression VisitOrderAscendingMethod(Expression m)
        {
            void InspectOrdering(Expression e)
            {
                MethodCallExpression o = e as MethodCallExpression ?? throw new AuthenticationException($"Invalid expression type {e.GetType().Name}");
                Expression lambdaBody = o.GetLambdaBody();

                switch (o.Method.Name)
                {
                    case "OrderBy":
                        Visit(o.Arguments[0]);
                        _sb.Append("\"sort\":[");
                        Visit(lambdaBody);
                        break;
                    case "OrderByDescending":
                        throw new InvalidOperationException("Cannot order in different directions.");
                    case "ThenBy":
                        InspectOrdering(o.Arguments[0]);
                        Visit(lambdaBody);
                        break;
                    default:
                        return;
                }

                _sb.Append(',');
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
                MethodCallExpression o = e as MethodCallExpression ?? throw new AuthenticationException($"Invalid expression type {e.GetType().Name}");
                Expression lambdaBody = o.GetLambdaBody();

                switch (o.Method.Name)
                {
                    case "OrderBy":
                        throw new InvalidOperationException("Cannot order in different directions.");
                    case "OrderByDescending":
                        Visit(o.Arguments[0]);
                        _sb.Append("\"sort\":[{");
                        Visit(lambdaBody);
                        _sb.Append(":\"desc\"}");
                        break;
                    case "ThenByDescending":
                        InspectOrdering(o.Arguments[0]);
                        _sb.Append('{');
                        Visit(lambdaBody);
                        _sb.Append(":\"desc\"}");
                        break;
                    default:
                        return;
                }

                _sb.Append(',');
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
            Expression lambdaBody = m.GetLambdaBody();

            if (lambdaBody is NewExpression n)
            {
                foreach (Expression a in n.Arguments)
                {
                    Visit(a);
                    _sb.Append(',');
                }
                _sb.Length--;
            }
            else if (lambdaBody is MemberExpression mb)
            {
                Visit(mb);
            }
            else
            {
                throw new NotSupportedException($"The expression of type {lambdaBody} is not supported in the Select method.");
            }

            _sb.Append("],");

            return m;
        }

        #endregion

        #region Enumerable

        private Expression VisitAnyMethod(MethodCallExpression m)
        {
            _sb.Append('{');
            Visit(m.Arguments[0]);
            _sb.Append(":{\"$elemMatch\":");
            Expression lambdaBody = m.GetLambdaBody();
            Visit(lambdaBody);
            _sb.Append("}}");
            return m;
        }
        private Expression VisitAllMethod(MethodCallExpression m)
        {
            _sb.Append('{');
            Visit(m.Arguments[0]);
            _sb.Append(":{\"$allMatch\":");
            Expression lambdaBody = m.GetLambdaBody();
            Visit(lambdaBody);
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
            _sb.Append(',');
            return m;
        }
        private Expression VisitWithQuorumMethod(MethodCallExpression m)
        {
            Visit(m.Arguments[0]);
            _sb.Append("\"r\":");
            Visit(m.Arguments[1]);
            _sb.Append(',');
            return m;
        }
        private Expression VisitWithoutIndexUpdateMethod(MethodCallExpression m)
        {
            Visit(m.Arguments[0]);
            _sb.Append("\"update\":false");
            _sb.Append(',');
            return m;
        }
        private Expression VisitFromStableMethod(MethodCallExpression m)
        {
            Visit(m.Arguments[0]);
            _sb.Append("\"stable\":true");
            _sb.Append(',');
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

            _sb.Append(',');
            return m;
        }
        public Expression VisitIncludeExecutionStatsMethod(MethodCallExpression m)
        {
            Visit(m.Arguments[0]);
            _sb.Append("\"execution_stats\":true");
            _sb.Append(',');
            return m;
        }

        public Expression VisitIncludeConflictsMethod(MethodCallExpression m)
        {
            Visit(m.Arguments[0]);
            _sb.Append("\"conflicts\":true");
            _sb.Append(',');
            return m;
        }

        public Expression VisitSelectFieldMethod(MethodCallExpression m)
        {
            Visit(m.Arguments[0]);
            _sb.Append("\"fields\":[");

            if (!(((ConstantExpression)m.Arguments[1]).Value is Expression[] fieldExpressions))
            {
                throw new InvalidOperationException();
            }

            foreach (Expression a in fieldExpressions)
            {
                Visit(a);
                _sb.Append(',');
            }

            _sb.Length--;
            _sb.Append("],");
            return m;
        }

        public Expression VisitConvertMethod(MethodCallExpression m)
        {
            Visit(m.Arguments[0]);
            _sb.Append("\"fields\":[");

            Type returnType = m.Method.GetGenericArguments()[1];
            PropertyInfo[] properties = returnType
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.DeclaringType != typeof(CouchDocument))
                .ToArray();

            foreach (PropertyInfo property in properties)
            {
                var field = property.GetCouchPropertyName(_options.PropertiesCase);
                _sb.Append($"\"{field}\",");
            }

            _sb.Length--;
            _sb.Append("],");
            return m;
        }

        #endregion

        #region EnumerableExtensions

        private Expression VisitEnumerableContains(MethodCallExpression m)
        {
            _sb.Append('{');
            Visit(m.Arguments[0]);
            _sb.Append(":{\"$all\":");
            Visit(m.Arguments[1]);
            _sb.Append("}}");
            return m;
        }
        private Expression VisitInMethod(MethodCallExpression m, bool not = false)
        {
            _sb.Append('{');
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
            _sb.Append('{');
            Visit(m.Arguments[1]);
            _sb.Append(":{\"$exists\":true");
            _sb.Append("}}");
            return m;
        }
        private Expression VisitIsCouchTypeMethod(MethodCallExpression m)
        {
            _sb.Append('{');
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
            _sb.Append('{');

            Visit(m.Arguments[0]);
            var isParameter = m.Arguments[0].NodeType == ExpressionType.Parameter;
            if (!isParameter)
            {
                _sb.Append(":{");
            }

            _sb.Append("\"$regex\":");
            Visit(m.Arguments[1]);
            _sb.Append(!isParameter ? "}}" : "}");

            return m;
        }

        #endregion

        #region Other

        private Expression VisitContainsMethod(MethodCallExpression m)
        {
            _sb.Append('{');
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
