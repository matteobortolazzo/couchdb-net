using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace CouchDB.Driver.ExpressionVisitors
{
    public class QueryPretranslator : ExpressionVisitor
    {
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == nameof(Queryable.Min))
            {
                Type[] genericArgs = node.Method.GetGenericArguments();
                MethodCallExpression orderByDescExpression = Expression.Call(typeof(Queryable), nameof(Queryable.OrderBy), genericArgs, node.Arguments[0], node.Arguments[1]);
                MethodCallExpression takeExpression = Expression.Call(typeof(Queryable), nameof(Queryable.Take), genericArgs.Take(1).ToArray(), orderByDescExpression, Expression.Constant(1));
                return takeExpression;
            }
            if (node.Method.Name == nameof(Queryable.Max))
            {
                Type[] genericArgs = node.Method.GetGenericArguments();
                MethodCallExpression orderByDescExpression = Expression.Call(typeof(Queryable), nameof(Queryable.OrderByDescending), genericArgs, node.Arguments[0], node.Arguments[1]);
                MethodCallExpression takeExpression = Expression.Call(typeof(Queryable), nameof(Queryable.Take), genericArgs.Take(1).ToArray(), orderByDescExpression, Expression.Constant(1));
                return takeExpression;
            }
            return base.VisitMethodCall(node);
        }
    }
}
