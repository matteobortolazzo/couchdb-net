using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CouchDB.Driver.Helpers;
using CouchDB.Driver.Indexes;
using CouchDB.Driver.Options;
using CouchDB.Driver.Types;

namespace CouchDB.Driver
{
    public abstract class CouchContext : IAsyncDisposable
    {
        public ICouchClient Client { get; }
        protected virtual void OnConfiguring(CouchOptionsBuilder optionsBuilder) { }
        protected virtual void OnDatabaseCreating(CouchDatabaseBuilder databaseBuilder) { }

        private static readonly MethodInfo InitDatabasesGenericMethod
            = typeof(CouchContext).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Single(mi => mi.Name == nameof(InitDatabaseAsync));

        private static readonly MethodInfo ApplyDatabaseChangesGenericMethod
            = typeof(CouchContext).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Single(mi => mi.Name == nameof(ApplyDatabaseChangesAsync));

        protected CouchContext() : this(new CouchOptions<CouchContext>()) { }

        protected CouchContext(CouchOptions options)
        {
            Check.NotNull(options, nameof(options));

            var optionsBuilder = new CouchOptionsBuilder(options);
            var databaseBuilder = new CouchDatabaseBuilder();

#pragma warning disable CA2214 // Do not call overridable methods in constructors
            OnConfiguring(optionsBuilder);
            OnDatabaseCreating(databaseBuilder);
#pragma warning restore CA2214 // Do not call overridable methods in constructors

            Client = new CouchClient(options);

            SetupDiscriminators(databaseBuilder);
            InitializeDatabases(options, databaseBuilder);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsync(true).ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }

        protected virtual async Task DisposeAsync(bool disposing)
        {
            if (disposing && Client != null)
            {
                await Client.DisposeAsync().ConfigureAwait(false);
            }
        }

        private static void SetupDiscriminators(CouchDatabaseBuilder databaseBuilder)
        {
            // Get all options that share the database with another one
            IEnumerable<KeyValuePair<Type, CouchDocumentBuilder>>? sharedDatabase = databaseBuilder.DocumentBuilders
                .Where(opt => opt.Value.Database != null)
                .GroupBy(v => v.Value.Database)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g);
            foreach (KeyValuePair<Type, CouchDocumentBuilder> option in sharedDatabase)
            {
                option.Value.Discriminator = option.Key.Name;
            }
        }

        private void InitializeDatabases(CouchOptions options, CouchDatabaseBuilder databaseBuilder)
        {
            foreach (PropertyInfo dbProperty in GetDatabaseProperties())
            {
                Type documentType = dbProperty.PropertyType.GetGenericArguments()[0];

                var initDatabasesTask = (Task)InitDatabasesGenericMethod.MakeGenericMethod(documentType)
                    .Invoke(this, new object[] { dbProperty, options, databaseBuilder });
                initDatabasesTask.ConfigureAwait(false).GetAwaiter().GetResult();

                var applyDatabaseChangesTask = (Task)ApplyDatabaseChangesGenericMethod.MakeGenericMethod(documentType)
                    .Invoke(this, new object[] { dbProperty, options, databaseBuilder });
                applyDatabaseChangesTask.ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        private async Task InitDatabaseAsync<TSource>(PropertyInfo propertyInfo, CouchOptions options, CouchDatabaseBuilder databaseBuilder)
            where TSource : CouchDocument
        {
            ICouchDatabase<TSource> database;
            Type documentType = typeof(TSource);

            if (databaseBuilder.DocumentBuilders.ContainsKey(documentType))
            {
                var documentBuilder = (CouchDocumentBuilder<TSource>)databaseBuilder.DocumentBuilders[documentType];
                var databaseName = documentBuilder.Database ?? Client.GetClassName(documentType);
                database = options.CheckDatabaseExists
                    ? await Client.GetOrCreateDatabaseAsync<TSource>(databaseName, documentBuilder.Shards, documentBuilder.Replicas, documentBuilder.Partitioned, documentBuilder.Discriminator).ConfigureAwait(false)
                    : Client.GetDatabase<TSource>(databaseName, documentBuilder.Discriminator);
            }
            else
            {
                database = options.CheckDatabaseExists
                    ? await Client.GetOrCreateDatabaseAsync<TSource>().ConfigureAwait(false)
                    : Client.GetDatabase<TSource>();
            }

            propertyInfo.SetValue(this, database);
        }

        private async Task ApplyDatabaseChangesAsync<TSource>(PropertyInfo propertyInfo, CouchOptions options, CouchDatabaseBuilder databaseBuilder)
            where TSource : CouchDocument
        {
            Type documentType = typeof(TSource);
            if (!databaseBuilder.DocumentBuilders.ContainsKey(documentType))
            {
                return;
            }

            var database = (CouchDatabase<TSource>)propertyInfo.GetValue(this);
            var documentBuilder = (CouchDocumentBuilder<TSource>)databaseBuilder.DocumentBuilders[documentType];

            if (!documentBuilder.IndexDefinitions.Any())
            {
                return;
            }

            List<IndexInfo> indexes = await database.GetIndexesAsync().ConfigureAwait(false);

            foreach (IndexSetupDefinition<TSource> indexSetup in documentBuilder.IndexDefinitions)
            {
                await TryCreateOrUpdateIndexAsync(options, indexes, indexSetup, database)
                    .ConfigureAwait(false);
            }
        }

        private static async Task TryCreateOrUpdateIndexAsync<TSource>(
            CouchOptions options,
            IEnumerable<IndexInfo> indexes,
            IndexSetupDefinition<TSource> indexSetup,
            CouchDatabase<TSource> database)
            where TSource : CouchDocument
        {
            IndexInfo? currentIndex = TryFindIndex(
                indexes,
                indexSetup.Name,
                indexSetup.Options?.DesignDocument);

            if (currentIndex == null)
            {
                await database.CreateIndexAsync(
                        indexSetup.Name,
                        indexSetup.IndexBuilderAction,
                        indexSetup.Options)
                    .ConfigureAwait(false);
                return;
            }

            if (!options.OverrideExistingIndexes)
            {
                return;
            }

            IndexDefinition indexDefinition = database.NewIndexBuilder(indexSetup.IndexBuilderAction).Build();
            if (!AreFieldsEqual(currentIndex.Fields, indexDefinition.Fields))
            {
                await database.DeleteIndexAsync(currentIndex)
                    .ConfigureAwait(false);
                await database.CreateIndexAsync(indexSetup.Name, indexDefinition, indexSetup.Options)
                    .ConfigureAwait(false);
            }
        }

        private static IndexInfo? TryFindIndex(IEnumerable<IndexInfo> indexes, string name, string? designDocument)
        {
            return indexes.SingleOrDefault(current =>
                current.Name == name &&
                (designDocument == null || current.DesignDocument == designDocument));
        }

        private static bool AreFieldsEqual(Dictionary<string, IndexFieldDirection> current,
            Dictionary<string, IndexFieldDirection> requested)
        {
            if (current.Count != requested.Count)
            {
                return false;
            }

            for (var i = 0; i < current.Count; i++)
            {
                (var currentField, IndexFieldDirection currentDirection) = current.ElementAt(i);
                (var requestedField, IndexFieldDirection requestedDirection) = requested.ElementAt(i);

                if (currentField != requestedField || currentDirection != requestedDirection)
                {
                    return false;
                }
            }

            return true;
        }

        protected virtual IEnumerable<PropertyInfo> GetDatabaseProperties() =>
            GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.PropertyType.IsGenericType &&
                            p.PropertyType.GetGenericTypeDefinition() == typeof(CouchDatabase<>))
                .ToArray();
    }
}