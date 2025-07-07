using System;
using Autofac;
using CouchDB.Driver.Helpers;
using CouchDB.Driver.Options;

namespace CouchDB.Driver.DependencyInjection.Autofac
{
    /// <summary>
    ///     Provides extension methods to register CouchDB contexts with Autofac.
    /// </summary>
    public static class AutofacRegistrationExtensions
    {
        /// <summary>
        ///     Registers a CouchDB context of type <typeparamref name="TContext" /> with the specified configuration delegate.
        /// </summary>
        /// <typeparam name="TContext">The type of the CouchDB context to register.</typeparam>
        /// <param name="builder">The Autofac container builder.</param>
        /// <param name="optionBuilderAction">
        ///     An action delegate that configures the <see cref="CouchOptionsBuilder{TContext}" />.
        /// </param>
        /// <returns>The modified <see cref="ContainerBuilder" />.</returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if <paramref name="builder" /> or <paramref name="optionBuilderAction" /> is <c>null</c>.
        /// </exception>
        public static ContainerBuilder AddCouchContext<TContext>(this ContainerBuilder builder,
            Action<CouchOptionsBuilder<TContext>> optionBuilderAction)
            where TContext : CouchContext
        {
            Check.NotNull(builder, nameof(builder));
            Check.NotNull(optionBuilderAction, nameof(optionBuilderAction));

            var optionsBuilder = new CouchOptionsBuilder<TContext>();
            optionBuilderAction?.Invoke(optionsBuilder);

            builder
                .RegisterInstance(optionsBuilder.Options)
                .AsSelf();

            builder
                .RegisterType<TContext>()
                .SingleInstance();

            return builder;
        }

        /// <summary>
        ///     Registers a CouchDB context of type <typeparamref name="TContext" /> using a factory that can resolve services from
        ///     the Autofac container.
        /// </summary>
        /// <typeparam name="TContext">The type of the CouchDB context to register.</typeparam>
        /// <param name="builder">The Autofac container builder.</param>
        /// <param name="optionBuilderFactory">
        ///     A factory function that receives an <see cref="IComponentContext" /> and returns a configured
        ///     <see cref="CouchOptionsBuilder{TContext}" />.
        /// </param>
        /// <returns>The modified <see cref="ContainerBuilder" />.</returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if <paramref name="builder" /> or <paramref name="optionBuilderFactory" /> is <c>null</c>.
        /// </exception>
        public static ContainerBuilder AddCouchContext<TContext>(
            this ContainerBuilder builder,
            Func<IComponentContext, CouchOptionsBuilder<TContext>> optionBuilderFactory)
            where TContext : CouchContext
        {
            Check.NotNull(builder, nameof(builder));
            Check.NotNull(optionBuilderFactory, nameof(optionBuilderFactory));

            builder
                .Register(optionBuilderFactory)
                .SingleInstance()
                .AsSelf()
                .As<CouchOptionsBuilder<TContext>>();

            builder
                .Register(ctx => ctx.Resolve<CouchOptionsBuilder<TContext>>().Options)
                .As<CouchOptions<TContext>>()
                .SingleInstance();

            builder
                .RegisterType<TContext>()
                .AsSelf()
                .SingleInstance();

            return builder;
        }
    }
}