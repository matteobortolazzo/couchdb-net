using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.Helpers;
using CouchDB.Driver.Query;
using CouchDB.Driver.Types;
using Flurl.Http;

namespace CouchDB.Driver.Local;

/// <inheritdoc />
public class LocalDocuments(IFlurlClient flurlClient, QueryContext queryContext) : ILocalDocuments
{
    /// <inheritdoc />
    public async Task<IList<CouchDocumentInfo>> GetAsync(LocalDocumentsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        IFlurlRequest request = NewRequest();

        if (options != null)
        {
            request = request.ApplyQueryParametersOptions(options);
        }

        LocalDocumentsResult result = await request
            .AppendPathSegments("_local_docs")
            .GetJsonAsync<LocalDocumentsResult>(cancellationToken: cancellationToken)
            .SendRequestAsync()
            .ConfigureAwait(false);
        return result.Rows;
    }

    /// <inheritdoc />
    public async Task<IList<CouchDocumentInfo>> GetAsync(IReadOnlyCollection<string> keys,
        LocalDocumentsOptions? options = null, CancellationToken cancellationToken = default)
    {
        IFlurlRequest request = NewRequest();

        if (options != null)
        {
            request = request.ApplyQueryParametersOptions(options);
        }

        LocalDocumentsResult result = await request
            .AppendPathSegments("_local_docs")
            .PostJsonAsync(new { keys }, cancellationToken: cancellationToken)
            .SendRequestAsync()
            .ReceiveJson<LocalDocumentsResult>()
            .ConfigureAwait(false);
        return result.Rows;
    }

    /// <inheritdoc />
    public Task<TSource> GetAsync<TSource>(string id, CancellationToken cancellationToken = default)
        where TSource : CouchDocument
    {
        ArgumentNullException.ThrowIfNull(id);
        return NewRequest()
            .AppendPathSegments(Uri.EscapeDataString(GetLocalId(id)))
            .GetJsonAsync<TSource>(cancellationToken: cancellationToken)
            .SendRequestAsync();
    }

    /// <inheritdoc />
    public Task CreateOrUpdateAsync<TSource>(TSource document, CancellationToken cancellationToken = default)
        where TSource : CouchDocument
    {
        ArgumentNullException.ThrowIfNull(document);
        return NewRequest()
            .AppendPathSegments(Uri.EscapeDataString(GetLocalId(document.Id)))
            .PutJsonAsync(document, cancellationToken: cancellationToken)
            .SendRequestAsync();
    }

    /// <inheritdoc />
    public Task DeleteAsync<TSource>(TSource document, CancellationToken cancellationToken = default)
        where TSource : CouchDocument
    {
        ArgumentNullException.ThrowIfNull(document);
        return NewRequest()
            .AppendPathSegments(Uri.EscapeDataString(GetLocalId(document.Id)))
            .DeleteAsync(cancellationToken: cancellationToken)
            .SendRequestAsync();
    }

    private static string GetLocalId(string id)
        => !id.Contains("_local/", StringComparison.InvariantCultureIgnoreCase)
            ? $"_local/{id}"
            : id;

    private IFlurlRequest NewRequest()
    {
        return flurlClient.Request(queryContext.Endpoint).AppendPathSegment(queryContext.EscapedDatabaseName);
    }
}