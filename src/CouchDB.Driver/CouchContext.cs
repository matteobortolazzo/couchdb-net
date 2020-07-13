using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CouchDB.Driver.Helpers;
using CouchDB.Driver.Options;

namespace CouchDB.Driver
{
    public abstract class CouchContext : IAsyncDisposable
    {
        public ICouchClient Client { get; }
        protected virtual void OnConfiguring(CouchOptionsBuilder optionsBuilder) { }

        private static readonly MethodInfo GetDatabaseGenericMethod
            = typeof(CouchClient).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Single(mi => mi.Name == nameof(CouchClient.GetDatabase) && mi.GetParameters().Length == 0);

        private static readonly MethodInfo GetOrCreateDatabaseAsyncGenericMethod
            = typeof(CouchClient).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Single(mi => mi.Name == nameof(CouchClient.GetOrCreateDatabaseAsync) && mi.GetParameters().Length == 3);

        protected CouchContext() : this(new CouchOptionsBuilder()) { }

        protected CouchContext(CouchOptionsBuilder optionsBuilder)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));

#pragma warning disable CA2214 // Do not call overridable methods in constructors
            OnConfiguring(optionsBuilder);
#pragma warning restore CA2214 // Do not call overridable methods in constructors

            Client = new CouchClient(optionsBuilder.Options);

            PropertyInfo[] databasePropertyInfos = GetDatabaseProperties();

            foreach (PropertyInfo propertyInfo in databasePropertyInfos)
            {
                Type documentType = propertyInfo.PropertyType.GetGenericArguments()[0];
                object? database;
                if (optionsBuilder.Options.CheckDatabaseExists)
                {
                    MethodInfo getOrCreateDatabaseMethod = GetOrCreateDatabaseAsyncGenericMethod.MakeGenericMethod(documentType);
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                    var parameters = new[] {(object)null, null, default(CancellationToken)};
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                    var task = (Task)getOrCreateDatabaseMethod.Invoke(Client, parameters);
                    task.ConfigureAwait(false).GetAwaiter().GetResult();
                    PropertyInfo resultProperty = task.GetType().GetProperty(nameof(Task<object>.Result));
                    database = resultProperty.GetValue(task);
                }
                else
                {
                    MethodInfo getDatabaseMethod = GetDatabaseGenericMethod.MakeGenericMethod(documentType);
                    database = getDatabaseMethod.Invoke(Client, Array.Empty<object>());
                }
                propertyInfo.SetValue(this, database);
            }
        }

        public ValueTask DisposeAsync()
        {
            return Client.DisposeAsync();
        }

        private PropertyInfo[] GetDatabaseProperties() =>
            GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.PropertyType.IsGenericType &&
                            p.PropertyType.GetGenericTypeDefinition() == typeof(CouchDatabase<>))
                .ToArray();
    }
}
