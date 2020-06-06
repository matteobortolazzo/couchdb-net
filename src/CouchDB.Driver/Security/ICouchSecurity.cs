using System.Threading.Tasks;

namespace CouchDB.Driver.Security
{
    public interface ICouchSecurity
    {
        /// <summary>
        /// Gets security information about the database.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the database security information.</returns>
        Task<CouchSecurityInfo> GetInfoAsync();

        /// <summary>
        /// Sets security information about the database.
        /// </summary>
        /// <param name="info">The security object to set.</param>
        /// <returns>A task that represents the asynchronous operation. </returns>
        Task SetInfoAsync(CouchSecurityInfo info);
    }
}