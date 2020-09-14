using System;
using System.IO;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CouchDB.Driver.ChangesFeed.Filters;
using CouchDB.Driver.ChangesFeed.Responses;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.Query;
using CouchDB.Driver.Types;
using Flurl.Http;

namespace CouchDB.Driver.ChangesFeed
{
    internal static class ChangesFeedFilterExtensions
    {
        public static async Task<ChangesFeedResponse<TSource>> QueryWithFilterAsync<TSource>(this IFlurlRequest request, IAsyncQueryProvider queryProvider, ChangesFeedFilter filter,
            CancellationToken cancellationToken)
            where TSource : CouchDocument
        {
            if (filter is DocumentIdsChangesFeedFilter documentIdsFilter)
            {
                return await request
                    .SetQueryParam("filter", "_doc_ids")
                    .PostJsonAsync(new ChangesFeedFilterDocuments(documentIdsFilter.Value), cancellationToken)
                    .ReceiveJson<ChangesFeedResponse<TSource>>()
                    .ConfigureAwait(false);
            }

            if (filter is SelectorChangesFeedFilter<TSource> selectorFilter)
            {
                MethodCallExpression whereExpression = selectorFilter.Value.WrapInWhereExpression();
                var jsonSelector = queryProvider.ToString(whereExpression);

                return await request
                    .WithHeader("Content-Type", "application/json")
                    .SetQueryParam("filter", "_selector")
                    .PostStringAsync(jsonSelector, cancellationToken)
                    .ReceiveJson<ChangesFeedResponse<TSource>>()
                    .ConfigureAwait(false);
            }

            if (filter is DesignChangesFeedFilter)
            {
                return await request
                    .SetQueryParam("filter", "_design")
                    .GetJsonAsync<ChangesFeedResponse<TSource>>(cancellationToken)
                    .ConfigureAwait(false);
            }

            if (filter is ViewChangesFeedFilter viewFilter)
            {
                return await request
                    .SetQueryParam("filter", "_view")
                    .SetQueryParam("view", viewFilter.Value)
                    .GetJsonAsync<ChangesFeedResponse<TSource>>(cancellationToken)
                    .ConfigureAwait(false);
            }
            throw new InvalidOperationException($"Filter of type {filter.GetType().Name} not supported.");
        }

        public static async Task<Stream> QueryContinuousWithFilterAsync<TSource>(this IFlurlRequest request, IAsyncQueryProvider queryProvider, ChangesFeedFilter filter, CancellationToken cancellationToken)
            where TSource: CouchDocument
        {
            if (filter is DocumentIdsChangesFeedFilter documentIdsFilter)
            {
                return await request
                    .SetQueryParam("filter", "_doc_ids")
                    .PostJsonStreamAsync(new ChangesFeedFilterDocuments(documentIdsFilter.Value), cancellationToken, HttpCompletionOption.ResponseHeadersRead)
                    .ConfigureAwait(false);
            }

            if (filter is SelectorChangesFeedFilter<TSource> selectorFilter)
            {
                MethodCallExpression whereExpression = selectorFilter.Value.WrapInWhereExpression();
                var jsonSelector = queryProvider.ToString(whereExpression);

                return await request
                    .WithHeader("Content-Type", "application/json")
                    .SetQueryParam("filter", "_selector")
                    .PostStringStreamAsync(jsonSelector, cancellationToken, HttpCompletionOption.ResponseHeadersRead)
                    .ConfigureAwait(false);
            }

            if (filter is DesignChangesFeedFilter)
            {
                return await request
                    .SetQueryParam("filter", "_design")
                    .GetStreamAsync(cancellationToken, HttpCompletionOption.ResponseHeadersRead)
                    .ConfigureAwait(false);
            }

            if (filter is ViewChangesFeedFilter viewFilter)
            {
                return await request
                    .SetQueryParam("filter", "_view")
                    .SetQueryParam("view", viewFilter.Value)
                    .GetStreamAsync(cancellationToken, HttpCompletionOption.ResponseHeadersRead)
                    .ConfigureAwait(false);
            }
            throw new InvalidOperationException($"Filter of type {filter.GetType().Name} not supported.");
        }
    }
}
