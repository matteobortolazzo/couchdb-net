using CouchDB.Driver.DTOs;
namespace CouchDB.Driver.Exceptions;

/// <summary>
/// The exception that is thrown when there is a conflict.
/// </summary>
public class CouchConflictException : CouchException
{
    internal CouchConflictException(CouchError couchError) : base(couchError, null) { }

}