using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CouchDB.Driver.Helpers;
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
                .Single(mi => mi.Name == nameof(InitDatabasesAsync));

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
            IEnumerable<PropertyInfo> databasePropertyInfos = GetDatabaseProperties();

            foreach (PropertyInfo dbProperty in databasePropertyInfos)
            {
                Type documentType = dbProperty.PropertyType.GetGenericArguments()[0];

                var initDatabasesTask = (Task)InitDatabasesGenericMethod.MakeGenericMethod(documentType)
                    .Invoke(this, new object[] {dbProperty, options});
                initDatabasesTask.ConfigureAwait(false).GetAwaiter().GetResult();

                var applyDatabaseChangesTask = (Task)ApplyDatabaseChangesGenericMethod.MakeGenericMethod(documentType)
                    .Invoke(this, new object[] { dbProperty, options, databaseBuilder });
                applyDatabaseChangesTask.ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        public ValueTask DisposeAsync()
        {
            return Client.DisposeAsync();
        }

        private async Task InitDatabasesAsync<TSource>(PropertyInfo propertyInfo, CouchOptions options)
            where TSource : CouchDocument
        {
            ICouchDatabase<TSource> database = options.CheckDatabaseExists
                ? await Client.GetOrCreateDatabaseAsync<TSource>().ConfigureAwait(false)
                : Client.GetDatabase<TSource>();

            propertyInfo.SetValue(this, database);
        }

        private async Task ApplyDatabaseChangesAsync<TSource>(PropertyInfo propertyInfo, CouchOptions options, CouchDatabaseBuilder databaseBuilder)
            where TSource: CouchDocument
        {
            if (!databaseBuilder.DocumentBuilders.ContainsKey(typeof(TSource)))
            {
                return;
            }

            var database = (CouchDatabase<TSource>)propertyInfo.GetValue(this);
            var documentBuilder = (CouchDocumentBuilder<TSource>)databaseBuilder.DocumentBuilders[typeof(TSource)];

            List<IndexInfo> indexes = await database.GetIndexesAsync().ConfigureAwait(false);

            foreach (IndexDefinition<TSource> indexDefinition in documentBuilder.IndexDefinitions)
            {
                // Delete the index if it already exists
                if (options.OverrideExistingIndexes && indexDefinition.Options?.DesignDocument != null)
                {
                    IndexInfo existingIndex = indexes.FirstOrDefault(i =>
                        i.DesignDocument.Equals(indexDefinition.Options.DesignDocument, StringComparison.InvariantCultureIgnoreCase));

                    if (existingIndex != null)
                    {
                        await database.DeleteIndexAsync(existingIndex).ConfigureAwait(false);
                    }
                }

                await database.CreateIndexAsync(
                        indexDefinition.Name,
                        indexDefinition.IndexBuilderAction,
                        indexDefinition.Options)
                    .ConfigureAwait(false);
            }
        }

        private IEnumerable<PropertyInfo> GetDatabaseProperties() =>
            GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.PropertyType.IsGenericType &&
                            p.PropertyType.GetGenericTypeDefinition() == typeof(CouchDatabase<>))
                .ToArray();
    }
}