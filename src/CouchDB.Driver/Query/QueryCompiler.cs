using System.Linq.Expressions;
using System.Threading;

namespace CouchDB.Driver
{
    internal class QueryCompiler : IQueryCompiler
    {
        private readonly IQuerySender _requestSender;
        private readonly IQueryTranslator _queryTranslator;

        public QueryCompiler(IQuerySender requestSender, IQueryTranslator queryTranslator)
        {
            _requestSender = requestSender;
            _queryTranslator = queryTranslator;
        }

        public TResult Execute<TResult>(Expression query)
            => SendRequest<TResult>(query, false, default);

        public TResult ExecuteAsync<TResult>(Expression query, CancellationToken cancellationToken)
            => SendRequest<TResult>(query, true, cancellationToken);

        private TResult SendRequest<TResult>(Expression query, bool async, CancellationToken cancellationToken)
        {
            var body = _queryTranslator.Translate(query);
            return _requestSender.Send<TResult>(body, async, cancellationToken);
        }
    }
}
