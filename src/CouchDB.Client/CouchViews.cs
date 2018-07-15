using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CouchDB.Client.Helpers;
using Flurl.Http;

namespace CouchDB.Client
{
    public class CouchViews<TSource> where TSource : CouchEntity
    {
        private readonly CouchDatabase<TSource> _db;

        public CouchViews(CouchDatabase<TSource> db)
        {
            _db = db;
        }

        public async Task CompactAsync(string designName)
        {
            await _db.NewDbRequest()
                .AppendPathSegment("_compact")
                .AppendPathSegment(designName)
                .PostJsonAsync(null)
                .SendAsync();
        }

        public async Task CleanUpAsync()
        {
            await _db.NewDbRequest()
                .AppendPathSegment("_view_cleanup")
                .PostJsonAsync(null)
                .SendAsync();
        }
    }
}
