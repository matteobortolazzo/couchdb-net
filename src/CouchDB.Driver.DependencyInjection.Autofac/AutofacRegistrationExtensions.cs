using Autofac;
using CouchDB.Driver.Options;
using System;

namespace CouchDB.Driver.DependencyInjection.Autofac;

public static class AutofacRegistrationExtensions
{
    public static ContainerBuilder AddCouchContext<TContext>(this ContainerBuilder builder,
        Action<CouchOptionsBuilder<TContext>> optionBuilderAction)
        where TContext : CouchContext
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(optionBuilderAction);

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