using System.Threading;
using System.Threading.Tasks;
using CouchDB.Driver.Helpers;
using CouchDB.Driver.Types;

namespace CouchDB.Driver.Extensions
{
    public static class CouchDatabaseExtensions
    {
        /// <summary>
        /// Change the password of the given user.
        /// </summary>
        /// <typeparam name="TCouchUser">The type of user.</typeparam>
        /// <param name="database">The users database.</param>
        /// <param name="user">The user to update.</param>
        /// <param name="password">The password.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated user.</returns>
        public static Task<TCouchUser> ChangeUserPassword<TCouchUser>(this ICouchDatabase<TCouchUser> database, TCouchUser user,
            string password, CancellationToken cancellationToken = default)
            where TCouchUser : CouchUser
        {
            Check.NotNull(database, nameof(database));
            Check.NotNull(user, nameof(user));
            Check.NotNull(password, nameof(password));

            user.Password = password;
            return database.AddOrUpdateAsync(user, false, cancellationToken);
        }
    }
}
