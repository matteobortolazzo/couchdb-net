namespace CouchDB.Driver.Exceptions
{
    /// <summary>
    /// The exception that is thrown when there is no index for the query.
    /// </summary>
    public class CouchNoIndexException : CouchException
    {
        /// <summary>
        /// Creates a new instance of CouchNoIndexException.
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="reason">Error reason</param>
        public CouchNoIndexException(string message, string reason) : base(message, reason) { }

        public CouchNoIndexException()
        {
        }

        public CouchNoIndexException(string message) : base(message)
        {
        }

        public CouchNoIndexException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }
}
