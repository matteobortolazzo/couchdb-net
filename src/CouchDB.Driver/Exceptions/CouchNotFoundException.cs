namespace CouchDB.Driver.Exceptions
{
    /// <summary>
    /// The exception that is thrown when something is not found.
    /// </summary>
    public class CouchNotFoundException : CouchException
    {
        /// <summary>
        /// Creates a new instance of CouchNotFoundException.
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="reason">Error reason</param>
        public CouchNotFoundException(string message, string reason) : base(message, reason) { }

        public CouchNotFoundException()
        {
        }

        public CouchNotFoundException(string message) : base(message)
        {
        }

        public CouchNotFoundException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }
}
