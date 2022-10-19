using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.Helpers;
using CouchDB.Driver.Query;
using CouchDB.Driver.Types;
using Flurl.Http;

namespace CouchDB.Driver.Local
{
    /// <inheritdoc />
    public class LocalDocuments : ILocalDocuments
    {
        private readonly IFlurlClient _flurlClient;
        private readonly QueryContext _queryContext;

        public LocalDocuments(IFlurlClient flurlClient, QueryContext queryContext)
        {
            _flurlClient = flurlClient;
            _queryContext = queryContext;
        }

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
                .GetJsonAsync<LocalDocumentsResult>(cancellationToken)
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
                .PostJsonAsync(new {keys}, cancellationToken)
                .SendRequestAsync()
                .ReceiveJson<LocalDocumentsResult>()
                .ConfigureAwait(false);
            return result.Rows;
        }

        /// <inheritdoc />
        public Task<TSource> GetAsync<TSource>(string id, CancellationToken cancellationToken = default)
            where TSource : CouchDocument
        {
            Check.NotNull(id, nameof(id));
            return NewRequest()
                .AppendPathSegments(GetLocalId(Uri.EscapeDataString(id)))
                .GetJsonAsync<TSource>(cancellationToken)
                .SendRequestAsync();
        }

        /// <inheritdoc />
        public Task CreateOrUpdateAsync<TSource>(TSource document, CancellationToken cancellationToken = default)
            where TSource : CouchDocument
        {
            Check.NotNull(document, nameof(document));
            return NewRequest()
                .AppendPathSegments(GetLocalId(Uri.EscapeDataString(document.Id)))
                .PutJsonAsync(document, cancellationToken)
                .SendRequestAsync();
        }

        /// <inheritdoc />
        public Task DeleteAsync<TSource>(TSource document, CancellationToken cancellationToken = default)
            where TSource : CouchDocument
        {
            Check.NotNull(document, nameof(document));
            return NewRequest()
                .AppendPathSegments(GetLocalId(Uri.EscapeDataString(document.Id)))
                .DeleteAsync(cancellationToken)
                .SendRequestAsync();
        }

        private static string GetLocalId(string id)
            => !id.Contains("_local/", StringComparison.InvariantCultureIgnoreCase)
                ? $"_local/{id}"
                : id;

        private IFlurlRequest NewRequest()
        {
            return _flurlClient.Request(_queryContext.Endpoint).AppendPathSegment(_queryContext.EscapedDatabaseName);
        }
    }
}
