using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CouchDB.Driver.DTOs;
using CouchDB.Driver.Helpers;
using CouchDB.Driver.Types;
using Flurl.Http;

namespace CouchDB.Driver.Query
{
    internal class QuerySender : IQuerySender
    {
        private readonly IFlurlClient _client;
        private readonly QueryContext _queryContext;

        private static readonly MethodInfo GenericToListMethod
            = typeof(QuerySender).GetRuntimeMethods()
                .Single(m => (m.Name == nameof(ToList)) && m.IsGenericMethod);
        private static readonly MethodInfo GenericToListAsyncMethod
            = typeof(QuerySender).GetRuntimeMethods()
                .Single(m => (m.Name == nameof(ToListAsync)) && m.IsGenericMethod);

        public QuerySender(IFlurlClient client, QueryContext queryContext)
        {
            _client = client;
            _queryContext = queryContext;
        }

        public TResult Send<TResult>(string body, bool async, CancellationToken cancellationToken)
        {
            Type resultType = typeof(TResult);
            if (async)
            {
                resultType = resultType.GetGenericArguments()[0];
            }

            Type itemType = resultType.IsGenericType ? resultType.GetGenericArguments()[0] : typeof(object);

            MethodInfo toListMethodInfo = async ? GenericToListAsyncMethod : GenericToListMethod;

            return (TResult)toListMethodInfo
                .MakeGenericMethod(itemType)
                .Invoke(this, new object[] { body, cancellationToken });
        }

        private CouchList<TItem> ToList<TItem>(string body, CancellationToken cancellationToken)
        {
            FindResult<TItem> result = SendAsync<TItem>(body, cancellationToken).GetAwaiter().GetResult();
            return new CouchList<TItem>(result.Docs.ToList(), result.Bookmark, result.ExecutionStats);
        }

        private async Task<CouchList<TItem>> ToListAsync<TItem>(string body, CancellationToken cancellationToken)
        {
            FindResult<TItem> result = await SendAsync<TItem>(body, cancellationToken).ConfigureAwait(false);
            return new CouchList<TItem>(result.Docs.ToList(), result.Bookmark, result.ExecutionStats);
        }

        private Task<FindResult<TItem>> SendAsync<TItem>(string body, CancellationToken cancellationToken) =>
            _client
                .Request(_queryContext.Endpoint)
                .AppendPathSegments(_queryContext.DatabaseName, "_find")
                .WithHeader("Content-Type", "application/json")
                .PostStringAsync(body, cancellationToken)
                .ReceiveJson<FindResult<TItem>>()
                .SendRequestAsync();
    }
}