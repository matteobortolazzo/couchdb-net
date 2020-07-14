using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace CouchDB.Driver.Query
{
    internal interface IAsyncQueryProvider: IQueryProvider
    {
        TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default);
        string ToString(Expression expression);
    }
}