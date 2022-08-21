using System;
using System.Collections.Generic;
using System.Threading;
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
        /// <param name="discriminator">Filters documents by the given discriminator.</param>
        /// <returns>An instance of the CouchDB database with given name.</returns>
        ICouchDatabase<TSource> GetDatabase<TSource>(string database, string? discriminator = null) where TSource : CouchDocument;

        /// <summary>
        /// Returns an instance of the CouchDB database with the given name.
        /// If a database exists with the given name, it throws an exception.
        /// The name must begin with a lowercase letter and can contains only lowercase characters, digits or _, $, (, ), +, - and /.
        /// </summary>
        /// <typeparam name="TSource">The type of database documents.</typeparam>
        /// <param name="database">The database name.</param>
        /// <param name="shards">The number of range partitions. Default is 8, unless overridden in the cluster config.</param>
        /// <param name="replicas">The number of copies of the database in the cluster. The default is 3, unless overridden in the cluster config.</param>
        /// <param name="partitioned">Whether to create a partitioned database. Default is <c>False</c>.</param>
        /// <param name="discriminator">Filters documents by the given discriminator.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the newly created CouchDB database.</returns>
        Task<ICouchDatabase<TSource>> CreateDatabaseAsync<TSource>(string database,
            int? shards = null, int? replicas = null, bool? partitioned = null, string? discriminator = null, CancellationToken cancellationToken = default)
            where TSource : CouchDocument;

        /// <summary>
        /// Returns an instance of the CouchDB database with the given name.
        /// If no database exists with the given name, it creates it.
        /// The name must begin with a lowercase letter and can contains only lowercase characters, digits or _, $, (, ), +, - and /.
        /// </summary>
        /// <typeparam name="TSource">The type of database documents.</typeparam>
        /// <param name="database">The database name.</param>
        /// <param name="shards">Used when creating. The number of range partitions. Default is 8, unless overridden in the cluster config.</param>
        /// <param name="replicas">Used when creating. The number of copies of the database in the cluster. The default is 3, unless overridden in the cluster config.</param>
        /// <param name="partitioned">Whether to create a partitioned database. Default is <c>False</c>.</param>
        /// <param name="discriminator">Filters documents by the given discriminator.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the newly created CouchDB database.</returns>
        Task<ICouchDatabase<TSource>> GetOrCreateDatabaseAsync<TSource>(string database,
            int? shards = null, int? replicas = null, bool? partitioned = null, string? discriminator = null, CancellationToken cancellationToken = default)
            where TSource : CouchDocument;

        /// <summary>
        /// Deletes the database with the given name from the server.
        /// </summary>
        /// <param name="database">The database name.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        Task DeleteDatabaseAsync(string database, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns an instance of the CouchDB database with the name type <see cref="TSource"/>.
        /// If EnsureDatabaseExists is configured, it creates the database if it doesn't exists.
        /// </summary>
        /// <typeparam name="TSource">The type of database documents.</typeparam>
        /// <returns>The instance of the CouchDB database of the given type.</returns>
        ICouchDatabase<TSource> GetDatabase<TSource>() where TSource : CouchDocument;

        /// <summary>
        /// Returns an instance of the CouchDB database with the name type <see cref="TSource"/>.
        /// If a database exists with the given name, it throws an exception.
        /// </summary>
        /// <typeparam name="TSource">The type of database documents.</typeparam>
        /// <param name="shards">The number of range partitions. Default is 8, unless overridden in the cluster config.</param>
        /// <param name="replicas">The number of copies of the database in the cluster. The default is 3, unless overridden in the cluster config.</param>
        /// <param name="partitioned">Whether to create a partitioned database. Default is <c>False</c>.</param>
        /// <param name="discriminator">Filters documents by the given discriminator.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the newly created CouchDB database.</returns>
        Task<ICouchDatabase<TSource>> CreateDatabaseAsync<TSource>(int? shards = null, int? replicas = null, bool? partitioned = null, string? discriminator = null, CancellationToken cancellationToken = default) where TSource : CouchDocument;

        /// <summary>
        /// Returns an instance of the CouchDB database with the name type <see cref="TSource"/>.
        /// If no database exists with the given name, it creates it.
        /// </summary>
        /// <typeparam name="TSource">The type of database documents.</typeparam>
        /// <param name="shards">Used when creating. The number of range partitions. Default is 8, unless overridden in the cluster config.</param>
        /// <param name="replicas">Used when creating. The number of copies of the database in the cluster. The default is 3, unless overridden in the cluster config.</param>
        /// <param name="partitioned">Whether to create a partitioned database. Default is <c>False</c>.</param>
        /// <param name="discriminator">Filters documents by the given discriminator.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the newly created CouchDB database.</returns>
        Task<ICouchDatabase<TSource>> GetOrCreateDatabaseAsync<TSource>(int? shards = null, int? replicas = null, bool? partitioned = null, string? discriminator = null, CancellationToken cancellationToken = default)
            where TSource : CouchDocument;

        /// <summary>
        /// Deletes the database with the name type <see cref="TSource"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of database documents.</typeparam>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DeleteDatabaseAsync<TSource>(CancellationToken cancellationToken = default) where TSource : CouchDocument;

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
        /// Returns an instance of the users database.
        /// If no users database exists, it creates it.
        /// If EnsureDatabaseExists is configured, it creates the database if it doesn't exists.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the CouchDB database.</returns>
        Task<ICouchDatabase<CouchUser>> GetOrCreateUsersDatabaseAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns an instance of the users database.
        /// If no users database exists, it creates it.
        /// </summary>
        /// <typeparam name="TUser">The specific type of user.</typeparam>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the CouchDB database.</returns>
        Task<ICouchDatabase<TUser>> GetOrCreateUsersDatabaseAsync<TUser>(CancellationToken cancellationToken = default) where TUser : CouchUser;

        /// <summary>
        /// Check if database exists.
        /// </summary>
        /// <param name="database">The database name.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>Return True if the database exists, False otherwise.</returns>
        Task<bool> ExistsAsync(string database, CancellationToken cancellationToken = default);

        /// <summary>
        /// Determines whether the server is up, running, and ready to respond to requests.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>true is the server is not in maintenance_mode; otherwise, false.</returns>
        Task<bool> IsUpAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns all databases names in the server.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the sequence of databases names.</returns>
        Task<IEnumerable<string>> GetDatabasesNamesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns all active tasks in the server.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the sequence of all active tasks.</returns>
        Task<IEnumerable<CouchActiveTask>> GetActiveTasksAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Configures a database replication operation.
        /// </summary>
        /// <param name="source">Fully qualified source database URL or an object which contains the full URL of the source database with additional parameters like headers.</param>
        /// <param name="target">Fully qualified target database URL or an object which contains the full URL of the target database with additional parameters like headers.</param>
        /// <param name="replication">An instance of <see cref="CouchReplication"/>.</param>
        /// <param name="persistent">Persist the operation to the replication database.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>Returns True if the operation succeeded, False otherwise.</returns>
        Task<bool> ReplicateAsync(string source, string target, CouchReplication? replication = null,
            bool persistent = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes a database replication operation.
        /// </summary>
        /// <param name="source">Fully qualified source database URL or an object which contains the full URL of the source database with additional parameters like headers.</param>
        /// <param name="target">Fully qualified target database URL or an object which contains the full URL of the target database with additional parameters like headers.</param>
        /// <param name="replication">An instance of <see cref="CouchReplication"/>.</param>
        /// <param name="persistent"></param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>Returns True if the operation succeeded, False otherwise.</returns>
        Task<bool> RemoveReplicationAsync(string source, string target, CouchReplication? replication = null,
            bool persistent = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get the database name for the given type.
        /// </summary>
        /// <param name="type">The type of database documents.</param>
        /// <returns></returns>
        string GetClassName(Type type);

        /// <summary>
        /// URI of the CouchDB endpoint.
        /// </summary>
        Uri Endpoint { get; }
    }
}