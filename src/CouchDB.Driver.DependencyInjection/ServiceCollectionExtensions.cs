using System;
using CouchDB.Driver.Options;
using Microsoft.Extensions.DependencyInjection;

namespace CouchDB.Driver.DependencyInjection
{
    public static  class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDbContext<TContext>(this IServiceCollection services,
            Action<CouchOptionsBuilder<TContext>> optionBuilderAction)
            where TContext: CouchContext
        {
            Console.WriteLine(optionBuilderAction);
            return services;
        }
    }
}
