using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CouchDB.Driver.Types
{
    /// <summary>
    /// Represents a Couch query response.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    public interface ICouchList<TSource> : IEnumerable<TSource>
    {
        /// <summary>
        /// An opaque string used for paging.
        /// </summary>
        string Bookmark { get; }

        /// <summary>
        /// Execution statistics.
        /// </summary>
        ExecutionStats ExecutionStats { get; }
    }
    internal class CouchList<TSource> : ICouchList<TSource>
    {
        private IEnumerable<TSource> _source;

        public string Bookmark { get; }
        public ExecutionStats ExecutionStats { get; }

        public CouchList(IEnumerable<TSource> source, string bookmark, ExecutionStats executionStats)
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
            return GetEnumerator();
        }
    }
}
