// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Examples.Autofac;

internal static class Program
{
    public static Task Main(string[] args)
    {
        WrappingWriter writer = new(Console.Out);
        IMediator med = BuildMediator(writer);

        return Runner.Run(med, writer, "Autofac", testStreams: true);
    }

    private static IMediator BuildMediator(WrappingWriter writer)
    {

        ContainerBuilder builder = new();

        builder.RegisterAssemblyTypes(typeof(IMediator).GetTypeInfo().Assembly).AsImplementedInterfaces();

        Type[] mediatorOpenTypes = new[]
        {
                typeof(IRequestHandler<,>),
                typeof(IRequestExceptionHandler<,,>),
                typeof(IRequestExceptionAction<,>),
                typeof(INotificationHandler<>),
                typeof(IStreamRequestHandler<,>)
            };

        foreach (Type mediatorOpenType in mediatorOpenTypes)
        {
            builder
                .RegisterAssemblyTypes(typeof(Ping).GetTypeInfo().Assembly)
                .AsClosedTypesOf(mediatorOpenType)
                // when having a single class implementing several handler types
                // this call will cause a handler to be called twice
                // in general you should try to avoid having a class implementing for instance `IRequestHandler<,>` and `INotificationHandler<>`
                // the other option would be to remove this call
                // see also https://github.com/jbogard/MediatR/issues/462
                .AsImplementedInterfaces();
        }

        builder.RegisterInstance(writer).As<TextWriter>();

        // It appears Autofac returns the last registered types first
        builder.RegisterGeneric(typeof(GenericStreamPipelineBehavior<,>)).As(typeof(IStreamPipelineBehavior<,>));

        builder.RegisterGeneric(typeof(RequestPostProcessorBehavior<,>)).As(typeof(IPipelineBehavior<,>));
        builder.RegisterGeneric(typeof(RequestPreProcessorBehavior<,>)).As(typeof(IPipelineBehavior<,>));
        builder.RegisterGeneric(typeof(RequestExceptionActionProcessorBehavior<,>))
            .As(typeof(IPipelineBehavior<,>));
        builder.RegisterGeneric(typeof(RequestExceptionProcessorBehavior<,>)).As(typeof(IPipelineBehavior<,>));
        builder.RegisterGeneric(typeof(GenericRequestPreProcessor<>)).As(typeof(IRequestPreProcessor<>));
        builder.RegisterGeneric(typeof(GenericRequestPostProcessor<,>)).As(typeof(IRequestPostProcessor<,>));
        builder.RegisterGeneric(typeof(GenericPipelineBehavior<,>)).As(typeof(IPipelineBehavior<,>));
        builder.RegisterGeneric(typeof(ConstrainedRequestPostProcessor<,>)).As(typeof(IRequestPostProcessor<,>));
        builder.RegisterGeneric(typeof(ConstrainedPingedHandler<>)).As(typeof(INotificationHandler<>));


        ServiceCollection services = new();
        
        builder.Populate(services);

        // The below returns:
        //  - RequestPreProcessorBehavior
        //  - RequestPostProcessorBehavior
        //  - GenericPipelineBehavior
        //  - GenericStreamPipelineBehavior
        //  - RequestExceptionActionProcessorBehavior
        //  - RequestExceptionProcessorBehavior

        //var behaviors = container
        //    .Resolve<IEnumerable<IPipelineBehavior<Ping, Pong>>>()
        //    .ToList();

        IContainer container = builder.Build();
        AutofacServiceProvider serviceProvider = new(container);
        IMediator med = serviceProvider.GetRequiredService<IMediator>();

        return med;
    }
}
