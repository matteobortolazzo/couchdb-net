using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CouchDB.Driver.DTOs;
using CouchDB.Driver.Helpers;
using CouchDB.Driver.Query;
using CouchDB.Driver.Types;
using Flurl.Http;

namespace CouchDB.Driver.LocalDocuments
{
    internal class LocalDocuments: ILocalDocuments
    {
        private readonly IFlurlClient _flurlClient;
        private readonly QueryContext _queryContext;

        public LocalDocuments(IFlurlClient flurlClient, QueryContext queryContext)
        {
            _flurlClient = flurlClient;
            _queryContext = queryContext;
        }

        public async Task<IList<CouchDocument>> GetAsync(CancellationToken cancellationToken = default)
        {
            LocalDocumentsResult result = await NewRequest()
                .AppendPathSegments("_local_docs")
                .GetJsonAsync<LocalDocumentsResult>(cancellationToken)
                .SendRequestAsync()
                .ConfigureAwait(false);
            return result.Rows;
        }

        public async Task<IList<CouchDocument>> GetAsync(IReadOnlyCollection<string> keys, CancellationToken cancellationToken = default)
        {
            LocalDocumentsResult result = await NewRequest()
                .AppendPathSegments("_local_docs")
                .PostJsonAsync(new {keys}, cancellationToken)
                .ReceiveJson<LocalDocumentsResult>()
                .ConfigureAwait(false);
            return result.Rows;
        }

        public Task<TSource> GetAsync<TSource>(string id, CancellationToken cancellationToken = default) where TSource : CouchDocument
            => NewRequest()
                .AppendPathSegments("_local", id)
                .GetJsonAsync<TSource>(cancellationToken)
                .SendRequestAsync();

        public Task AddAsync<TSource>(TSource document, CancellationToken cancellationToken = default) where TSource : CouchDocument
            => NewRequest()
                .AppendPathSegments("_local", document.Id)
                .PostJsonAsync(document, cancellationToken)
                .SendRequestAsync();

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
