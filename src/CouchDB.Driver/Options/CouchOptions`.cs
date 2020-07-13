using System;

namespace CouchDB.Driver.Options
{
    public class CouchOptions<TContext> : CouchOptions
        where TContext : CouchContext
    {
        public override Type ContextType => typeof(TContext);
    }
}
