using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CouchDB.Driver.Settings;

namespace CouchDB.Driver
{
    public abstract class CouchDbContext : IAsyncDisposable
    {
        private readonly ICouchClient _client;
        protected abstract void OnConfiguring(ICouchContextConfiguration configuration);

        private static readonly MethodInfo GetDatabaseGenericMethod
            = typeof(CouchClient).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Single(mi => mi.Name == nameof(CouchClient.GetDatabase) && mi.GetParameters().Length == 0);

        protected CouchDbContext()
        {
#pragma warning disable CA2214 // Do not call overridable methods in constructors
            var settings = new CouchSettings();
            OnConfiguring(settings);
#pragma warning restore CA2214 // Do not call overridable methods in constructors

            _client = new CouchClient(settings);

            IEnumerable<PropertyInfo> properties = GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.PropertyType.IsGenericType &&
                            p.PropertyType.GetGenericTypeDefinition() == typeof(CouchDatabase<>));

            foreach (PropertyInfo propertyInfo in properties)
            {
                Type documentType = propertyInfo.PropertyType.GetGenericArguments()[0];
                MethodInfo getDatabaseMethod = GetDatabaseGenericMethod.MakeGenericMethod(documentType);
                var database = getDatabaseMethod.Invoke(_client, Array.Empty<object>());
                propertyInfo.SetValue(this, database);
            }
        }

        public ValueTask DisposeAsync()
        {
            return _client.DisposeAsync();
        }
    }
}
