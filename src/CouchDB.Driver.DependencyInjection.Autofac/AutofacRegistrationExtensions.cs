using Autofac;
using CouchDB.Driver.Options;
using System;
using CouchDB.Driver.Helpers;

namespace CouchDB.Driver.DependencyInjection.Autofac
{
    public static class AutofacRegistrationExtensions
    {
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
    }
}
