// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Examples.LightInject;

class Program
{
    static Task Main(string[] args)
    {
        WrappingWriter writer = new(Console.Out);
        IMediator med = BuildMediator(writer);

        return Runner.Run(med, writer, "LightInject");
    }

    private static IMediator BuildMediator(WrappingWriter writer)
    {
        ServiceContainer serviceContainer = new(ContainerOptions.Default.WithMicrosoftSettings());
        serviceContainer.Register<IMediator, Mediator>();            
        serviceContainer.RegisterInstance<TextWriter>(writer);

        serviceContainer.RegisterAssembly(typeof(Ping).GetTypeInfo().Assembly, (serviceType, implementingType) =>
            serviceType.IsConstructedGenericType &&
            (
                serviceType.GetGenericTypeDefinition() == typeof(IRequestHandler<,>) ||
                serviceType.GetGenericTypeDefinition() == typeof(INotificationHandler<>)
            ));
                    
        serviceContainer.RegisterOrdered(typeof(IPipelineBehavior<,>),
            new[]
            {
                typeof(RequestPreProcessorBehavior<,>),
                typeof(RequestPostProcessorBehavior<,>),
                typeof(GenericPipelineBehavior<,>)
            }, type => null);

            
        serviceContainer.RegisterOrdered(typeof(IRequestPostProcessor<,>),
            new[]
            {
                typeof(GenericRequestPostProcessor<,>),
                typeof(ConstrainedRequestPostProcessor<,>)
            }, type => null);
                   
        serviceContainer.Register(typeof(IRequestPreProcessor<>), typeof(GenericRequestPreProcessor<>));

        ServiceCollection services = new();
        IServiceProvider provider = serviceContainer.CreateServiceProvider(services);
        return provider.GetRequiredService<IMediator>(); 
    }
}
