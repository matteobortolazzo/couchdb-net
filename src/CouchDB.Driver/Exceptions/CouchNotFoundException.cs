using CouchDB.Driver.DTOs;

namespace CouchDB.Driver.Exceptions;

/// <summary>
/// The exception that is thrown when something is not found.
/// </summary>
public class CouchNotFoundException : CouchException
{
    internal CouchNotFoundException(CouchError couchError, Exception innerException) : base(couchError, innerException) { }
}