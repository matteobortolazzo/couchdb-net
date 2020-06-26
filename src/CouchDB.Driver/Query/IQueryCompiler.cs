using System.Linq.Expressions;
using System.Threading;

namespace CouchDB.Driver
{
    internal interface IQueryCompiler
    {
        TResult Execute<TResult>(Expression query);
        TResult ExecuteAsync<TResult>(Expression query, CancellationToken cancellationToken);
    }
}
