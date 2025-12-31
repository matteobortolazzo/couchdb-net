using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CouchDB.Driver.DTOs;
using CouchDB.Driver.Exceptions;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.Helpers;
using CouchDB.Driver.Types;

namespace CouchDB.Driver.Query;

internal class QuerySender(HttpClient httpClient, QueryContext queryContext) : IQuerySender
{
    private static readonly MethodInfo GenericToListMethod
        = typeof(QuerySender).GetRuntimeMethods()
            .Single(m => m is { Name: nameof(ToList), IsGenericMethod: true });

    private static readonly MethodInfo GenericToListAsyncMethod
        = typeof(QuerySender).GetRuntimeMethods()
            .Single(m => m is { Name: nameof(ToListAsync), IsGenericMethod: true });

    public TResult Send<TResult>(string body, bool async, bool throwExceptionOnWarning,
        CancellationToken cancellationToken)
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
            .Invoke(this, [body, throwExceptionOnWarning, cancellationToken])!;
    }

    private CouchList<TItem> ToList<TItem>(string body, bool throwExceptionOnWarning,
        CancellationToken cancellationToken)
    {
        FindResult<TItem> result = SendAsync<TItem>(body, throwExceptionOnWarning, cancellationToken).GetAwaiter()
            .GetResult();
        return new CouchList<TItem>(result.Docs.ToList(), result.Bookmark, result.ExecutionStats);
    }

    private async Task<CouchList<TItem>> ToListAsync<TItem>(string body, bool throwExceptionOnWarning,
        CancellationToken cancellationToken)
    {
        FindResult<TItem> result =
            await SendAsync<TItem>(body, throwExceptionOnWarning, cancellationToken).ConfigureAwait(false);
        return new CouchList<TItem>(result.Docs.ToList(), result.Bookmark, result.ExecutionStats);
    }

    private async Task<FindResult<TItem>> SendAsync<TItem>(string body, bool throwExceptionOnWarning,
        CancellationToken cancellationToken)
    {
        FindResult<TItem>? findResult = await httpClient
            .Request(queryContext.Endpoint)
            .AppendPathSegments(queryContext.DatabaseName, "_find")
            .WithHeader("Content-Type", "application/json")
            .PostStringAsync(body, cancellationToken: cancellationToken)
            .ReceiveJson<FindResult<TItem>>()
            .SendRequestAsync();

        if (throwExceptionOnWarning && !string.IsNullOrEmpty(findResult.Warning))
        {
            throw new CouchQueryWarningException(findResult.Warning);
        }

        return findResult;
    }
}