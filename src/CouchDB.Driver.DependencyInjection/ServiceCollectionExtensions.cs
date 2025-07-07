using System;
using CouchDB.Driver.Helpers;
using CouchDB.Driver.Options;
using Microsoft.Extensions.DependencyInjection;

namespace CouchDB.Driver.DependencyInjection
{
    /// <summary>
    ///     Provides extension methods for registering CouchDB contexts with the dependency injection container.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Registers a CouchDB context of type <typeparamref name="TContext" /> with the specified configuration delegate.
        /// </summary>
        /// <typeparam name="TContext">The type of the Couch context to register.</typeparam>
        /// <param name="services">The service collection to add the context to.</param>
        /// <param name="optionBuilderAction">
        ///     An action delegate that configures the <see cref="CouchOptionsBuilder{TContext}" /> used to create the context.
        /// </param>
        /// <returns>The modified <see cref="IServiceCollection" />.</returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if <paramref name="services" /> or <paramref name="optionBuilderAction" /> is <c>null</c>.
        /// </exception>
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

        /// <summary>
        ///     Registers a CouchDB context of type <typeparamref name="TContext" /> with a factory delegate that can resolve
        ///     services from the container.
        /// </summary>
        /// <typeparam name="TContext">The type of the Couch context to register.</typeparam>
        /// <param name="services">The service collection to add the context to.</param>
        /// <param name="optionBuilderFactory">
        ///     A factory delegate that takes an <see cref="IServiceProvider" /> and returns a configured
        ///     <see cref="CouchOptionsBuilder{TContext}" />.
        /// </param>
        /// <returns>The modified <see cref="IServiceCollection" />.</returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if <paramref name="services" /> or <paramref name="optionBuilderFactory" /> is <c>null</c>.
        /// </exception>
        public static IServiceCollection AddCouchContext<TContext>(
            this IServiceCollection services,
            Func<IServiceProvider, CouchOptionsBuilder<TContext>> optionBuilderFactory)
            where TContext : CouchContext
        {
            Check.NotNull(services, nameof(services));
            Check.NotNull(optionBuilderFactory, nameof(optionBuilderFactory));

            services.AddSingleton<CouchOptions<TContext>>(sp =>
            {
                CouchOptionsBuilder<TContext>? builder = optionBuilderFactory(sp);
                return builder.Options;
            });

            services.AddSingleton<TContext>();

            return services;
        }
    }
}