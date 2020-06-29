using System.Linq.Expressions;

namespace CouchDB.Driver
{
    internal interface IQueryTranslator
    {
        string Translate(Expression e);
    }
}