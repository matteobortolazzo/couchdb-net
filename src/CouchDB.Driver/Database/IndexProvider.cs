using System;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CouchDB.Driver.DTOs;
using CouchDB.Driver.Helpers;
using CouchDB.Driver.Types;
using Flurl.Http;

namespace CouchDB.Driver.Database
{
    public class IndexProvider<TSource> : IIndexProvider<TSource> where TSource : CouchDocument
    {
        private readonly ICouchDatabase<TSource> _database;

        public IndexProvider(ICouchDatabase<TSource> database)
        {
            _database = database;
        }

        public async Task<bool> CreateAsync(Action<IndexInfo<TSource>> index,
            CancellationToken cancellationToken = default)
        {
            object message = _createMessage(index);

            IFlurlRequest request = _database.NewRequest();
            IndexSaveResponse response = await request
                .AppendPathSegment("_index")
                .PostJsonAsync(message, cancellationToken)
                .ReceiveJson<IndexSaveResponse>()
                .SendRequestAsync()
                .ConfigureAwait(false);

            return "created" == response.Result;
        }

        public async Task<bool> CreateOrUpdateAsync(Action<IndexInfo<TSource>> index,
            CancellationToken cancellationToken = default)
        {
            IFlurlRequest request = _database.NewRequest();
            IndexDesignResponse response = await request
                .AppendPathSegment("_index")
                .GetJsonAsync<IndexDesignResponse>()
                .SendRequestAsync()
                .ConfigureAwait(false);

            var message = _createMessage(index);
            var currentIndex = response.Indexes.FirstOrDefault(x => x.Name == message.Name);
            if (currentIndex != null)
            {
                await Delete(currentIndex.DDoc, currentIndex.Name, cancellationToken).ConfigureAwait(false);
            }

            return await CreateAsync(index, cancellationToken).ConfigureAwait(false);
        }

        private static IndexSaveRequest _createMessage(Action<IndexInfo<TSource>> index)
        {
            if (index == null)
            {
                throw new ArgumentNullException(nameof(index));
            }

            var info = new IndexInfo<TSource>();
            index.Invoke(info);

            Check.NotNull(info, nameof(info));
            Check.NotNull(info.IndexName!, "index must be not null or empty");
            Check.NotNull(info.Fields!, "fields must be not null or empty");


            NewExpression? body = info?.Fields?.Body as NewExpression;
            var fields = body?.Arguments.Select(x => (x as MemberExpression)?.Member.Name);
            var message = new IndexSaveRequest
            {
                Name = info?.IndexName, Index = new IndexSaveRequestFields {Fields = fields}, Type = "json"
            };
            return message;
        }

        private async Task<bool> Delete(string? currentIndexDDoc, string? currentIndexName,
            CancellationToken cancellationToken)
        {
            IFlurlRequest request = _database.NewRequest();
            var response = await request
                .AppendPathSegments("_index", currentIndexDDoc, "json", currentIndexName)
                .DeleteAsync(cancellationToken)
                .SendRequestAsync()
                .ReceiveJson<OperationResult>()
                .ConfigureAwait(false);

            return response.Ok;
        }
    }
}