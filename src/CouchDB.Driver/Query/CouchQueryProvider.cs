using CouchDB.Driver.Extensions;

using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace CouchDB.Driver.Query;

internal class CouchQueryProvider : IAsyncQueryProvider
{
    private static readonly MethodInfo GenericCreateQueryMethod
        = typeof(CouchQueryProvider).GetRuntimeMethods()
            .Single(m => m is { Name: "CreateQuery", IsGenericMethod: true });

    private readonly MethodInfo _genericExecuteMethod;

    private readonly IQueryCompiler _queryCompiler;

    public CouchQueryProvider(IQueryCompiler queryCompiler)
    {
        _queryCompiler = queryCompiler;
        _genericExecuteMethod = queryCompiler.GetType()
            .GetRuntimeMethods()
            .Single(m => m is { Name: "Execute", IsGenericMethod: true });
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        => new CouchQueryable<TElement>(this, expression);

    public IQueryable CreateQuery(Expression expression)
        => (IQueryable)GenericCreateQueryMethod
            .MakeGenericMethod(expression.Type.GetSequenceType())
            .Invoke(this, [expression])!;

    public TResult Execute<TResult>(Expression expression)
        => _queryCompiler.Execute<TResult>(expression);

    public object Execute(Expression expression)
        => _genericExecuteMethod.MakeGenericMethod(expression.Type)
            .Invoke(_queryCompiler, [expression])!;

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        => _queryCompiler.ExecuteAsync<TResult>(expression, cancellationToken);

    public string ToString(Expression expression)
        => _queryCompiler.ToString(expression);
}