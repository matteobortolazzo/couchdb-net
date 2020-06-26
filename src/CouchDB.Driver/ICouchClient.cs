using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CouchDB.Driver.Types;

namespace CouchDB.Driver
{
    public interface ICouchClient: IAsyncDisposable
    {
        /// <summary>
        /// Returns an instance of the CouchDB database with the given name.
        /// </summary>
        /// <typeparam name="TSource">The type of database documents.</typeparam>
        /// <param name="database">The database name.</param>
        /// <returns>An instance of the CouchDB database with given name.</returns>
        ICouchDatabase<TSource> GetDatabase<TSource>(string database) where TSource : CouchDocument;

        /// <summary>
        /// Returns an instance of the CouchDB database with the given name.
        /// If no database exists with the given name, it creates it.
        /// The name must begin with a lowercase letter and can contains only lowercase characters, digits or _, $, (, ), +, - and /.
        /// This is equivalent of using <see cref="GetSafeDatabaseAsync"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of database documents.</typeparam>
        /// <typeparam name="TSource">The type of database documents.</typeparam>
        /// <param name="database">The database name.</param>
        /// <param name="shards">Used when creating. The number of range partitions. Default is 8, unless overridden in the cluster config.</param>
        /// <param name="replicas">Used when creating. The number of copies of the database in the cluster. The default is 3, unless overridden in the cluster config.</param>
        /// <returns></returns>
        Task<ICouchDatabase<TSource>> GetSafeDatabaseAsync<TSource>(string database, int? shards = null, int? replicas = null) where TSource : CouchDocument;

        /// <summary>
        /// Creates a new database with the given name in the server.
        /// The name must begin with a lowercase letter and can contains only lowercase characters, digits or _, $, (, ), +, - and /.
        /// </summary>
        /// <typeparam name="TSource">The type of database documents.</typeparam>
        /// <param name="database">The database name.</param>
        /// <param name="shards">The number of range partitions. Default is 8, unless overridden in the cluster config.</param>
        /// <param name="replicas">The number of copies of the database in the cluster. The default is 3, unless overridden in the cluster config.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the newly created CouchDB database.</returns>
        Task<ICouchDatabase<TSource>> CreateDatabaseAsync<TSource>(string database, int? shards = null,
            int? replicas = null) where TSource : CouchDocument;

        /// <summary>
        /// Deletes the database with the given name from the server.
        /// </summary>
        /// <typeparam name="TSource">The type of database documents.</typeparam>
        /// <param name="database">The database name.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DeleteDatabaseAsync<TSource>(string database) where TSource : CouchDocument;

        /// <summary>
        /// Returns an instance of the CouchDB database of the given type.
        /// If EnsureDatabaseExists is configured, it creates the database if it doesn't exists.
        /// </summary>
        /// <typeparam name="TSource">The type of database documents.</typeparam>
        /// <returns>The instance of the CouchDB database of the given type.</returns>
        ICouchDatabase<TSource> GetDatabase<TSource>() where TSource : CouchDocument;

        /// <summary>
        /// Creates a new database of the given type in the server.
        /// The name must begin with a lowercase letter and can contains only lowercase characters, digits or _, $, (, ), +, - and /.s
        /// </summary>
        /// <typeparam name="TSource">The type of database documents.</typeparam>
        /// <returns>A task that represents the asynchronous operation. The task result contains the newly created CouchDB database.</returns>
        Task<ICouchDatabase<TSource>> CreateDatabaseAsync<TSource>() where TSource : CouchDocument;

        /// <summary>
        /// Deletes the database with the given type from the server.
        /// </summary>
        /// <typeparam name="TSource">The type of database documents.</typeparam>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DeleteDatabaseAsync<TSource>() where TSource : CouchDocument;

        /// <summary>
        /// Returns an instance of the users database.
        /// If EnsureDatabaseExists is configured, it creates the database if it doesn't exists.
        /// </summary>
        /// <returns>The instance of the users database.</returns>
        ICouchDatabase<CouchUser> GetUsersDatabase();

        /// <summary>
        /// Returns an instance of the users database.
        /// If EnsureDatabaseExists is configured, it creates the database if it doesn't exists.
        /// </summary>
        /// <typeparam name="TUser">The specific type of user.</typeparam>
        /// <returns>The instance of the users database.</returns>
        ICouchDatabase<TUser> GetUsersDatabase<TUser>() where TUser : CouchUser;

        /// <summary>
        /// Check if database exists.
        /// </summary>
        /// <param name="database">The database name.</param>
        /// <returns>Return True if the database exists, False otherwise.</returns>
        Task<bool> ExistsAsync(string database);

        /// <summary>
        /// Determines whether the server is up, running, and ready to respond to requests.
        /// </summary>
        /// <returns>true is the server is not in maintenance_mode; otherwise, false.</returns>
        Task<bool> IsUpAsync();

        /// <summary>
        /// Returns all databases names in the server.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the sequence of databases names.</returns>
        Task<IEnumerable<string>> GetDatabasesNamesAsync();

        /// <summary>
        /// Returns all active tasks in the server.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the sequence of all active tasks.</returns>
        Task<IEnumerable<CouchActiveTask>> GetActiveTasksAsync();

        /// <summary>
        /// URI of the CouchDB endpoint.
        /// </summary>
        Uri Endpoint { get; }
    }
}