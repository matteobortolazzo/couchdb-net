using System.Threading;

namespace CouchDB.Driver
{
    internal interface IQuerySender
    {
        TResult Send<TResult>(string body, bool async, CancellationToken cancellationToken);
    }
}