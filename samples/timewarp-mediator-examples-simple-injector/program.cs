// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Examples.SimpleInjector;

internal static class Program
{
    private static Task Main(string[] args)
    {
        WrappingWriter writer = new(Console.Out);
        IMediator med = BuildMediator(writer);

        return Runner.Run(med, writer, "SimpleInjector", true);
    }

    private static IMediator BuildMediator(WrappingWriter writer)
    {
        Container container = new();

        ServiceCollection services = new();

        services
            .AddSimpleInjector(container);

        Assembly[] assemblies = GetAssemblies().ToArray();
        container.RegisterSingleton<IMediator, Mediator>();
        container.Register(typeof(IRequestHandler<,>), assemblies);

        RegisterHandlers(container, typeof(INotificationHandler<>), assemblies);
        RegisterHandlers(container, typeof(IRequestExceptionAction<,>), assemblies);
        RegisterHandlers(container, typeof(IRequestExceptionHandler<,,>), assemblies);
        RegisterHandlers(container, typeof(IStreamRequestHandler<,>), assemblies);

        container.Register(() => (TextWriter) writer, Lifestyle.Singleton);

        //Pipeline
        container.Collection.Register(typeof(IPipelineBehavior<,>), new[]
        {
            typeof(RequestExceptionProcessorBehavior<,>),
            typeof(RequestExceptionActionProcessorBehavior<,>),
            typeof(RequestPreProcessorBehavior<,>),
            typeof(RequestPostProcessorBehavior<,>),
            typeof(GenericPipelineBehavior<,>)
        });
        container.Collection.Register(typeof(IRequestPreProcessor<>), new[] { typeof(GenericRequestPreProcessor<>) });
        container.Collection.Register(typeof(IRequestPostProcessor<,>), new[] { typeof(GenericRequestPostProcessor<,>), typeof(ConstrainedRequestPostProcessor<,>) });
        container.Collection.Register(typeof(IStreamPipelineBehavior<,>), new[]
        {
            typeof(GenericStreamPipelineBehavior<,>)
        });

        IServiceProvider serviceProvider = services.BuildServiceProvider().UseSimpleInjector(container);

        container.RegisterInstance<IServiceProvider>(container);

        IMediator med = container.GetRequiredService<IMediator>();

        return med;
    }

    private static void RegisterHandlers(Container container, Type collectionType, Assembly[] assemblies)
    {
        // we have to do this because by default, generic type definitions (such as the Constrained Notification Handler) won't be registered
        IEnumerable<Type> handlerTypes = container.GetTypesToRegister(collectionType, assemblies, new TypesToRegisterOptions
        {
            IncludeGenericTypeDefinitions = true,
            IncludeComposites = false,
        });

        container.Collection.Register(collectionType, handlerTypes);
    }

    private static IEnumerable<Assembly> GetAssemblies()
    {
        yield return typeof(IMediator).GetTypeInfo().Assembly;
        yield return typeof(Ping).GetTypeInfo().Assembly;
    }
}
