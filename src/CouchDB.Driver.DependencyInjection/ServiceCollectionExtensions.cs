using CouchDB.Driver.Options;
using Microsoft.Extensions.DependencyInjection;

namespace CouchDB.Driver.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static CouchRegistration AddCouchDb(this IServiceCollection services,
        string endpoint,
        CouchCredentials credentials,
        CouchOptions? options)
    {
        CouchClient client = new(endpoint, credentials, options);
        services.AddSingleton(client);
        return new CouchRegistration(services, client);
    }

    public class CouchRegistration(IServiceCollection services, CouchClient client)
    {
        public CouchRegistration AddDatabase<T>() where T : class
        {
            ICouchDatabase<T> database = client.GetDatabase<T>();
            services.AddSingleton(database);
            return this;
        }

        public CouchRegistration AddDatabase<T>(string databaseName) where T : class
        {
            ICouchDatabase<T> database = client.GetDatabase<T>(databaseName);
            services.AddSingleton(database);
            return this;
        }
    }
}