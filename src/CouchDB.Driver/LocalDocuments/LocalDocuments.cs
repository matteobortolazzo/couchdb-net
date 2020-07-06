using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.Helpers;
using CouchDB.Driver.Query;
using CouchDB.Driver.Types;
using Flurl.Http;

namespace CouchDB.Driver.LocalDocuments
{
    /// <inheritdoc />
    internal class LocalDocuments: ILocalDocuments
    {
        private readonly IFlurlClient _flurlClient;
        private readonly QueryContext _queryContext;

        public LocalDocuments(IFlurlClient flurlClient, QueryContext queryContext)
        {
            _flurlClient = flurlClient;
            _queryContext = queryContext;
        }

        /// <inheritdoc />
        public async Task<IList<LocalCouchDocument>> GetAsync(LocalDocumentsOptions? options = null, CancellationToken cancellationToken = default)
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
        public async Task<IList<LocalCouchDocument>> GetAsync(IReadOnlyCollection<string> keys, LocalDocumentsOptions? options = null, CancellationToken cancellationToken = default)
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
        public Task<TSource> GetAsync<TSource>(string id, CancellationToken cancellationToken = default) where TSource : LocalCouchDocument
            => NewRequest()
                .AppendPathSegments("_local", id)
                .GetJsonAsync<TSource>(cancellationToken)
                .SendRequestAsync();

        /// <inheritdoc />
        public Task AddAsync<TSource>(TSource document, CancellationToken cancellationToken = default) where TSource : LocalCouchDocument
        {
            Check.NotNull(document, nameof(document));
            return NewRequest()
                .AppendPathSegments("_local", document.Id)
                .PostJsonAsync(document, cancellationToken)
                .SendRequestAsync();
        }

        /// <inheritdoc />
        public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
            => NewRequest()
                .AppendPathSegments("_local", id)
                .DeleteAsync(cancellationToken)
                .SendRequestAsync();

        private IFlurlRequest NewRequest()
        {
            return _flurlClient.Request(_queryContext.Endpoint).AppendPathSegment(_queryContext.EscapedDatabaseName);
        }
    }
}
