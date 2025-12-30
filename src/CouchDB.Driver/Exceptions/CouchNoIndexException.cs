using CouchDB.Driver.DTOs;
namespace CouchDB.Driver.Exceptions;

/// <summary>
/// The exception that is thrown when there is no index for the query.
/// </summary>
public class CouchNoIndexException : CouchException
{
    internal CouchNoIndexException(CouchError couchError) : base(couchError, null) { }
}