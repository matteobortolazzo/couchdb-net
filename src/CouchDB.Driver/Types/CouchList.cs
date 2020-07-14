using System.Collections;
using System.Collections.Generic;

namespace CouchDB.Driver.Types
{
    /// <summary>
    /// Represents a Couch query response.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    public class CouchList<TSource> : IReadOnlyList<TSource>
    {
        private readonly IReadOnlyList<TSource> _source;

        /// <summary>
        /// An opaque string used for paging.
        /// </summary>
        public string Bookmark { get; }
        /// <summary>
        /// Execution statistics.
        /// </summary>
        public ExecutionStats ExecutionStats { get; }

        public int Count => _source.Count;
        public bool IsReadOnly => true;
        public TSource this[int index] => _source[index];

        public CouchList(IReadOnlyList<TSource> source, string bookmark, ExecutionStats executionStats)
        {
            _source = source;
            Bookmark = bookmark;
            ExecutionStats = executionStats;
        }

        public IEnumerator<TSource> GetEnumerator()
        {
            return _source.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _source.GetEnumerator();
        }
    }
}
