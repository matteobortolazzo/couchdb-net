using System;
using System.Collections.Generic;
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

namespace CouchDB.Driver.ChangesFeed;

internal static class ChangesFeedFilterExtensions
{
    extension(IFlurlRequest request)
    {
        public async Task<ChangesFeedResponse<TSource>> QueryWithFilterAsync<TSource>(IAsyncQueryProvider queryProvider, ChangesFeedFilter filter,
            CancellationToken cancellationToken)
            where TSource : CouchDocument
        {
            if (filter is DocumentIdsChangesFeedFilter documentIdsFilter)
            {
                return await request
                    .SetQueryParam("filter", "_doc_ids")
                    .PostJsonAsync(new ChangesFeedFilterDocuments(documentIdsFilter.Value),
                        cancellationToken: cancellationToken)
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
                    .PostStringAsync(jsonSelector, cancellationToken: cancellationToken)
                    .ReceiveJson<ChangesFeedResponse<TSource>>()
                    .ConfigureAwait(false);
            }

            if (filter is DesignChangesFeedFilter)
            {
                return await request
                    .SetQueryParam("filter", "_design")
                    .GetJsonAsync<ChangesFeedResponse<TSource>>(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }

            if (filter is ViewChangesFeedFilter viewFilter)
            {
                return await request
                    .SetQueryParam("filter", "_view")
                    .SetQueryParam("view", viewFilter.Value)
                    .GetJsonAsync<ChangesFeedResponse<TSource>>(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }

            if (filter is DesignDocumentChangesFeedFilter designDocFilter)
            {
                IFlurlRequest req = ApplyDesignDocumentFilterParams(request, designDocFilter);

                return await req
                    .GetJsonAsync<ChangesFeedResponse<TSource>>(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }

            throw new InvalidOperationException($"Filter of type {filter.GetType().Name} not supported.");
        }

        public async Task<Stream> QueryContinuousWithFilterAsync<TSource>(IAsyncQueryProvider queryProvider, ChangesFeedFilter filter, CancellationToken cancellationToken)
            where TSource : CouchDocument
        {
            if (filter is DocumentIdsChangesFeedFilter documentIdsFilter)
            {
                return await request
                    .SetQueryParam("filter", "_doc_ids")
                    .PostJsonStreamAsync(new ChangesFeedFilterDocuments(documentIdsFilter.Value), cancellationToken,
                        HttpCompletionOption.ResponseHeadersRead)
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
                    .GetStreamAsync(HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                    .ConfigureAwait(false);
            }

            if (filter is ViewChangesFeedFilter viewFilter)
            {
                return await request
                    .SetQueryParam("filter", "_view")
                    .SetQueryParam("view", viewFilter.Value)
                    .GetStreamAsync(HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                    .ConfigureAwait(false);
            }

            if (filter is DesignDocumentChangesFeedFilter designDocFilter)
            {
                IFlurlRequest req = ApplyDesignDocumentFilterParams(request, designDocFilter);

                return await req
                    .GetStreamAsync(HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                    .ConfigureAwait(false);
            }

            throw new InvalidOperationException($"Filter of type {filter.GetType().Name} not supported.");
        }
    }

    private static IFlurlRequest ApplyDesignDocumentFilterParams(IFlurlRequest request,
        DesignDocumentChangesFeedFilter filter)
    {
        IFlurlRequest? req = request.SetQueryParam("filter", filter.FilterName);

        if (filter.QueryParameters == null)
        {
            return req;
        }

        foreach (KeyValuePair<string, string> param in filter.QueryParameters)
        {
            req = req.SetQueryParam(param.Key, param.Value);
        }

        return req;
    }
}