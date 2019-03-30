namespace CouchDB.Driver.Exceptions
{
    /// <summary>
    /// The exception that is thrown when there is a conflict.
    /// </summary>
    public class CouchConflictException : CouchException
    {
        /// <summary>
        /// Creates a new instance of CouchConflictException.
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="reason">Error reason</param>
        public CouchConflictException(string message, string reason) : base(message, reason) { }
    }
}
