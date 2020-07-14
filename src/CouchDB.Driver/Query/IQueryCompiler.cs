using System.Linq.Expressions;
using System.Threading;

namespace CouchDB.Driver.Query
{
    internal interface IQueryCompiler
    {
        string ToString(Expression query);
        TResult Execute<TResult>(Expression query);
        TResult ExecuteAsync<TResult>(Expression query, CancellationToken cancellationToken);
    }
}
