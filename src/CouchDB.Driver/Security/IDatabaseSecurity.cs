using System.Threading.Tasks;

namespace CouchDB.Driver.Security;

public interface IDatabaseSecurity
{
    /// <summary>
    /// Gets security information about the database.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the database security information.</returns>
    Task<DatabaseSecurityInfo> GetInfoAsync();

    /// <summary>
    /// Sets security information about the database.
    /// </summary>
    /// <param name="info">The security object to set.</param>
    /// <returns>A task that represents the asynchronous operation. </returns>
    Task SetInfoAsync(DatabaseSecurityInfo info);
}