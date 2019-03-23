using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CouchDB.Client
{
    public class CouchDatabase<T>
    {
        private readonly QueryProvider queryProvider;
        private readonly string db;

        internal CouchDatabase(QueryProvider queryProvider, string db)
        {
            this.queryProvider = queryProvider;
            this.db = db;
        }

        public IQueryable<T> AsQueryable()
        {
            return new CouchQuery<T>(queryProvider);
        }
    }
}
