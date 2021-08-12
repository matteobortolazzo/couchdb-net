using System;
using CouchDB.Driver.Helpers;
using CouchDB.Driver.Options;
using Microsoft.Extensions.DependencyInjection;

namespace CouchDB.Driver.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCouchContext<TContext>(this IServiceCollection services,
            Action<CouchOptionsBuilder<TContext>> optionBuilderAction)
            where TContext : CouchContext
        {
            Check.NotNull(services, nameof(services));
            Check.NotNull(optionBuilderAction, nameof(optionBuilderAction));
            
            var builder = new CouchOptionsBuilder<TContext>();
            optionBuilderAction?.Invoke(builder);
            return services
                .AddSingleton(builder.Options)
                .AddSingleton<TContext>();
        }
    }
}
