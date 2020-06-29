using System.Linq.Expressions;

namespace CouchDB.Driver
{
    internal interface IQueryOptimizer
    {
        Expression Optimize(Expression e);
    }
}