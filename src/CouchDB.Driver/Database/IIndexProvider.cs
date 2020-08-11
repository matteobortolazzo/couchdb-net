using System;
using System.Threading;
using System.Threading.Tasks;
using CouchDB.Driver.Types;

namespace CouchDB.Driver.Database
{
    public interface IIndexProvider<TSource> where TSource : CouchDocument
    {
        Task<bool> CreateAsync(Action<IndexInfo<TSource>> index, CancellationToken cancellationToken = default);
        Task<bool> CreateOrUpdateAsync(Action<IndexInfo<TSource>> index, CancellationToken cancellationToken = default);
    }
}