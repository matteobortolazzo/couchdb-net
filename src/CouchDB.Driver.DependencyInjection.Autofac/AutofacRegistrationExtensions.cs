using Autofac;
using CouchDB.Driver;
using CouchDB.Driver.Options;
using System;

namespace CouchDB.Driver.DependencyInjection.Autofac
{
    public static class AutofacRegistrationExtensions
    {
        public static ContainerBuilder AddCouchContext<TContext>(
            this ContainerBuilder containerBuilder,
            Action<CouchOptionsBuilder<TContext>> optionBuilderAction)
            where TContext : CouchContext
        {
            var optionsBuilder = new CouchOptionsBuilder<TContext>();
            optionBuilderAction?.Invoke(optionsBuilder);

            containerBuilder
                .RegisterInstance(optionsBuilder.Options)
                .AsSelf();

            containerBuilder
                .RegisterType<TContext>()
                .SingleInstance();

            return containerBuilder;
        }
    }
}
