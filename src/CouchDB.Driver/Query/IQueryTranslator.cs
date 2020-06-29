using System.Linq.Expressions;

namespace CouchDB.Driver.Query
{
    internal interface IQueryTranslator
    {
        string Translate(Expression e);
    }
}