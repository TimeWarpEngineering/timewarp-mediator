// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Examples.Windsor;

internal class Program
{
    private static Task Main(string[] args)
    {
        WrappingWriter writer = new(Console.Out);
        IMediator med = BuildMediator(writer);

        return Runner.Run(med, writer, "Castle.Windsor", true);
    }

    private static IMediator BuildMediator(WrappingWriter writer)
    {
        WindsorContainer container = new();
        container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel));
        container.Kernel.AddHandlersFilter(new ContravariantFilter());

        // *** The default lifestyle for Windsor is Singleton
        // *** If you are using ASP.net, it's better to register your services with 'Per Web Request LifeStyle'.

        FromAssemblyDescriptor fromAssemblyContainingPing = Classes.FromAssemblyContaining<Ping>();
        container.Register(fromAssemblyContainingPing.BasedOn(typeof(IRequestHandler<,>)).WithServiceAllInterfaces().AllowMultipleMatches());
        container.Register(fromAssemblyContainingPing.BasedOn(typeof(INotificationHandler<>)).WithServiceAllInterfaces().AllowMultipleMatches());
        container.Register(Component.For(typeof(IPipelineBehavior<,>)).ImplementedBy(typeof(RequestExceptionProcessorBehavior<,>)));
        container.Register(Component.For(typeof(IPipelineBehavior<,>)).ImplementedBy(typeof(RequestExceptionActionProcessorBehavior<,>)));
        container.Register(fromAssemblyContainingPing.BasedOn(typeof(IRequestExceptionAction<,>)).WithServiceAllInterfaces().AllowMultipleMatches());
        container.Register(fromAssemblyContainingPing.BasedOn(typeof(IRequestExceptionHandler<,,>)).WithServiceAllInterfaces().AllowMultipleMatches());
        container.Register(fromAssemblyContainingPing.BasedOn(typeof(IStreamRequestHandler<,>)).WithServiceAllInterfaces().AllowMultipleMatches());
        container.Register(fromAssemblyContainingPing.BasedOn(typeof(IRequestPreProcessor<>)).WithServiceAllInterfaces().AllowMultipleMatches());
        container.Register(fromAssemblyContainingPing.BasedOn(typeof(IRequestPostProcessor<,>)).WithServiceAllInterfaces().AllowMultipleMatches());

        container.Register(Component.For<IMediator>().ImplementedBy<Mediator>());
        container.Register(Component.For<TextWriter>().Instance(writer));
        container.Register(Component.For<ServiceFactory>().UsingFactoryMethod<ServiceFactory>(k => (type =>
        {
            Type enumerableType = type
                .GetInterfaces()
                .Concat(new[] { type })
                .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            Type service = enumerableType?.GetGenericArguments()?[0];
            object resolvedType = enumerableType != null ? k.ResolveAll(service) : k.Resolve(type);
            Type[] genericArguments = service?.GetGenericArguments();

            // Handle exceptions even using the base request types for IRequestExceptionHandler<,,>
            bool isRequestExceptionHandler = service?.GetGenericTypeDefinition()
                ?.IsAssignableTo(typeof(IRequestExceptionHandler<,,>)) ?? false;
            if (isRequestExceptionHandler)
                return ResolveRequestExceptionHandler(k, type, service, resolvedType, genericArguments);

            // Handle exceptions even using the base request types for IRequestExceptionAction<,>
            bool isRequestExceptionAction = service?.GetGenericTypeDefinition()
                ?.IsAssignableTo(typeof(IRequestExceptionAction<,>)) ?? false;
            if (isRequestExceptionAction)
                return ResolveRequestExceptionAction(k, type, service, resolvedType, genericArguments);
            
            return resolvedType;
        })));

        //Pipeline
        container.Register(Component.For(typeof(IStreamPipelineBehavior<,>)).ImplementedBy(typeof(GenericStreamPipelineBehavior<,>)));
        container.Register(Component.For(typeof(IPipelineBehavior<,>)).ImplementedBy(typeof(RequestPreProcessorBehavior<,>)).NamedAutomatically("PreProcessorBehavior"));
        container.Register(Component.For(typeof(IPipelineBehavior<,>)).ImplementedBy(typeof(RequestPostProcessorBehavior<,>)).NamedAutomatically("PostProcessorBehavior"));
        container.Register(Component.For(typeof(IPipelineBehavior<,>)).ImplementedBy(typeof(GenericPipelineBehavior<,>)).NamedAutomatically("Pipeline"));
        container.Register(Component.For(typeof(IRequestPreProcessor<>)).ImplementedBy(typeof(GenericRequestPreProcessor<>)).NamedAutomatically("PreProcessor"));
        container.Register(Component.For(typeof(IRequestPostProcessor<,>)).ImplementedBy(typeof(GenericRequestPostProcessor<,>)).NamedAutomatically("PostProcessor"));
        container.Register(Component.For(typeof(IRequestPostProcessor<,>), typeof(ConstrainedRequestPostProcessor<,>)).NamedAutomatically("ConstrainedRequestPostProcessor"));
        container.Register(Component.For(typeof(INotificationHandler<>), typeof(ConstrainedPingedHandler<>)).NamedAutomatically("ConstrainedPingedHandler"));

        IMediator med = container.Resolve<IMediator>();

        return med;
    }

    private static object ResolveRequestExceptionHandler(IKernel k, Type type, Type service, object resolvedType, Type[] genericArguments)
    {
        if (service == null
        || genericArguments == null
        || !service.IsInterface
        || !service.IsGenericType
        || !service.IsConstructedGenericType
        || !(service.GetGenericTypeDefinition()
        ?.IsAssignableTo(typeof(IRequestExceptionHandler<,,>)) ?? false)
        || genericArguments.Length != 3)
        {
            return resolvedType;
        }

        ServiceFactory serviceFactory = k.Resolve<ServiceFactory>();
        Type baseRequestType = genericArguments[0].BaseType;
        Type response = genericArguments[1];
        Type exceptionType = genericArguments[2];

        // Check if the base request type is valid
        if (baseRequestType == null
        || !baseRequestType.IsClass
        || baseRequestType == typeof(object)
        || ((!baseRequestType.GetInterfaces()
            ?.Any(i => i.IsAssignableFrom(typeof(IRequest<>)))) ?? true))
        {
            return resolvedType;
        }

        Type exceptionHandlerInterfaceType = typeof(IRequestExceptionHandler<,,>).MakeGenericType(baseRequestType, response, exceptionType);
        Type enumerableExceptionHandlerInterfaceType = typeof(IEnumerable<>).MakeGenericType(exceptionHandlerInterfaceType);
        Array resultArray = CreateArraysOutOfResolvedTypeAndEnumerableInterfaceTypes(type, resolvedType, serviceFactory, enumerableExceptionHandlerInterfaceType);

        return resultArray;
    }

    private static object ResolveRequestExceptionAction(IKernel k, Type type, Type service, object resolvedType, Type[] genericArguments)
    {
        if (service == null
        || genericArguments == null
        || !service.IsInterface
        || !service.IsGenericType
        || !service.IsConstructedGenericType
        || !(service.GetGenericTypeDefinition()
        ?.IsAssignableTo(typeof(IRequestExceptionAction<,>)) ?? false)
        || genericArguments.Length != 2)
        {
            return resolvedType;
        }

        ServiceFactory serviceFactory = k.Resolve<ServiceFactory>();
        Type baseRequestType = genericArguments[0].BaseType;
        Type exceptionType = genericArguments[1];

        // Check if the base request type is valid
        if (baseRequestType == null
        || !baseRequestType.IsClass
        || baseRequestType == typeof(object)
        || ((!baseRequestType.GetInterfaces()
            ?.Any(i => i.IsAssignableFrom(typeof(IRequest<>)))) ?? true))
        {
            return resolvedType;
        }

        Type exceptionHandlerInterfaceType = typeof(IRequestExceptionAction<,>).MakeGenericType(baseRequestType, exceptionType);
        Type enumerableExceptionHandlerInterfaceType = typeof(IEnumerable<>).MakeGenericType(exceptionHandlerInterfaceType);
        Array resultArray = CreateArraysOutOfResolvedTypeAndEnumerableInterfaceTypes(type, resolvedType, serviceFactory, enumerableExceptionHandlerInterfaceType);

        return resultArray;
    }

    private static Array CreateArraysOutOfResolvedTypeAndEnumerableInterfaceTypes(Type type, object resolvedType, ServiceFactory serviceFactory, Type enumerableExceptionHandlerInterfaceType)
    {
        Array firstArray = serviceFactory.Invoke(enumerableExceptionHandlerInterfaceType) as Array;
        Debug.Assert(firstArray != null, $"Array '{nameof(firstArray)}' should not be null because this method calls ResolveAll when a {typeof(IEnumerable<>).FullName} " +
            $"is passed as argument in argument named '{nameof(type)}'");

        Array secondArray = resolvedType is Array ? resolvedType as Array : new[] { resolvedType };
        Debug.Assert(secondArray != null, $"Array '{nameof(secondArray)}' should not be null because '{nameof(resolvedType)}' is an array or created as an array");

        Array resultArray = Array.CreateInstance(typeof(object), firstArray.Length + secondArray.Length);
        Array.Copy(firstArray, resultArray, firstArray.Length);
        Array.Copy(secondArray, 0, resultArray, firstArray.Length, secondArray.Length);
        return resultArray;
    }
}
