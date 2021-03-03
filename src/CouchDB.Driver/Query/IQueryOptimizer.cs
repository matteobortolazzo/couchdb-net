using System.Linq.Expressions;

namespace CouchDB.Driver.Query
{
    internal interface IQueryOptimizer
    {
        Expression Optimize(Expression e, string? discriminator);
    }
}