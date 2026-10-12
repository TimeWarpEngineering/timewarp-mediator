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

        foreach (Type implementingType in typeof(Ping).GetTypeInfo().Assembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && !type.IsGenericTypeDefinition))
        {
            foreach (Type serviceType in implementingType.GetInterfaces().Where(type =>
                type.IsConstructedGenericType &&
                (
                    type.GetGenericTypeDefinition() == typeof(IRequestHandler<,>) ||
                    type.GetGenericTypeDefinition() == typeof(IRequestHandler<>) ||
                    type.GetGenericTypeDefinition() == typeof(INotificationHandler<>)
                )))
            {
                // Notification handlers are many per service type, so each needs its own name;
                // request handlers stay the unnamed default the mediator resolves.
                if (serviceType.GetGenericTypeDefinition() == typeof(INotificationHandler<>))
                    serviceContainer.Register(serviceType, implementingType, implementingType.FullName);
                else
                    serviceContainer.Register(serviceType, implementingType);
            }
        }
                    
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
