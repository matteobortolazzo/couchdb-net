using System.Collections.Generic;

namespace CouchDB.Driver.Database
{
    internal class IndexSaveRequestFields
    {
#pragma warning disable 8618
        public IEnumerable<string?>? Fields { get; internal set; }
#pragma warning restore 8618
    }
}