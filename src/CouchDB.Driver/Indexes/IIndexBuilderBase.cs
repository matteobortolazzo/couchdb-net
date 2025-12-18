using System.Linq.Expressions;

namespace CouchDB.Driver.Indexes;

/// <summary>
/// Builder to configure CouchDB indexes.
/// </summary>
/// <typeparam name="TSource">The type of the document.</typeparam>
public interface IIndexBuilderBase<TSource>
    where TSource: class
{
    /// <summary>
    /// Creates a partial index which excludes documents based on the predicate at index time.
    /// </summary>
    /// <param name="predicate">Function to filter documents.</param>
    /// <returns>Returns the current instance to chain calls.</returns>
    void Where(Expression<Func<TSource, bool>> predicate);
}