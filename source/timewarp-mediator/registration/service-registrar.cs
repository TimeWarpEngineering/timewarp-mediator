// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Registration;

internal static class ServiceRegistrar
{
  public static void AddMediatorClassesWithTimeout(IServiceCollection services, MediatorServiceConfiguration configuration)
  {
    if (configuration.RegistrationTimeout <= 0)
    {
      AddMediatorClasses(services, configuration);
      return;
    }

    using CancellationTokenSource cts = new(configuration.RegistrationTimeout);
    try
    {
      AddMediatorClasses(services, configuration, cts.Token);
    }
    catch (OperationCanceledException exception) when (cts.IsCancellationRequested)
    {
      throw new TimeoutException("The generic handler registration process timed out.", exception);
    }
  }

  public static void AddMediatorClasses(IServiceCollection services, MediatorServiceConfiguration configuration, CancellationToken cancellationToken = default)
  {

    Assembly[] assembliesToScan = configuration.AssembliesToRegister.Distinct().ToArray();

    ConnectImplementationsToTypesClosing(typeof(IRequestHandler<,>), services, assembliesToScan, false, configuration, cancellationToken);
    ConnectImplementationsToTypesClosing(typeof(IRequestHandler<>), services, assembliesToScan, false, configuration, cancellationToken);
    ConnectImplementationsToTypesClosing(typeof(INotificationHandler<>), services, assembliesToScan, true, configuration, cancellationToken);
    ConnectImplementationsToTypesClosing(typeof(IStreamRequestHandler<,>), services, assembliesToScan, false, configuration, cancellationToken);
    ConnectImplementationsToTypesClosing(typeof(IRequestExceptionHandler<,,>), services, assembliesToScan, true, configuration, cancellationToken);
    ConnectImplementationsToTypesClosing(typeof(IRequestExceptionAction<,>), services, assembliesToScan, true, configuration, cancellationToken);

    if (configuration.AutoRegisterRequestProcessors)
    {
      ConnectImplementationsToTypesClosing(typeof(IRequestPreProcessor<>), services, assembliesToScan, true, configuration, cancellationToken);
      ConnectImplementationsToTypesClosing(typeof(IRequestPostProcessor<,>), services, assembliesToScan, true, configuration, cancellationToken);
    }

    List<Type> multiOpenInterfaces = new()
        {
            typeof(INotificationHandler<>),
            typeof(IRequestExceptionHandler<,,>),
            typeof(IRequestExceptionAction<,>)
        };

    if (configuration.AutoRegisterRequestProcessors)
    {
      multiOpenInterfaces.Add(typeof(IRequestPreProcessor<>));
      multiOpenInterfaces.Add(typeof(IRequestPostProcessor<,>));
    }

    foreach (Type multiOpenInterface in multiOpenInterfaces)
    {
      int arity = multiOpenInterface.GetGenericArguments().Length;

      List<Type> concretions = assembliesToScan
          .SelectMany(a => a.DefinedTypes)
          .Where(type => type.FindInterfacesThatClose(multiOpenInterface).Any())
          .Where(type => type.IsConcrete() && type.IsOpenGeneric())
          .Where(type => type.GetGenericArguments().Length == arity)
          .Where(configuration.TypeEvaluator)
          .ToList();

      foreach (Type type in concretions)
      {
        services.AddTransient(multiOpenInterface, type);
      }
    }
  }

  private static void ConnectImplementationsToTypesClosing(Type openRequestInterface,
      IServiceCollection services,
      IEnumerable<Assembly> assembliesToScan,
      bool addIfAlreadyExists,
      MediatorServiceConfiguration configuration,
      CancellationToken cancellationToken = default)
  {
    List<Type> concretions = new();
    List<Type> interfaces = new();
    List<Type> genericConcretions = new();
    List<Type> genericInterfaces = new();

    List<Type> types = assembliesToScan
        .SelectMany(a => a.DefinedTypes)
        .Where(t => !t.ContainsGenericParameters || configuration.RegisterGenericHandlers)
        .Where(t => t.IsConcrete() && t.FindInterfacesThatClose(openRequestInterface).Any())
        .Where(configuration.TypeEvaluator)
        .ToList();

    foreach (Type type in types)
    {
      cancellationToken.ThrowIfCancellationRequested();

      Type[] interfaceTypes = type.FindInterfacesThatClose(openRequestInterface).ToArray();

      if (!type.IsOpenGeneric())
      {
        concretions.Add(type);

        foreach (Type? interfaceType in interfaceTypes)
        {
          interfaces.Fill(interfaceType);
        }
      }
      else
      {
        genericConcretions.Add(type);
        foreach (Type? interfaceType in interfaceTypes)
        {
          genericInterfaces.Fill(interfaceType);
        }
      }
    }

    foreach (Type @interface in interfaces)
    {
      cancellationToken.ThrowIfCancellationRequested();

      List<Type> exactMatches = concretions.Where(x => x.CanBeCastTo(@interface)).ToList();
      if (addIfAlreadyExists)
      {
        foreach (Type type in exactMatches)
        {
          services.AddTransient(@interface, type);
        }
      }
      else
      {
        if (exactMatches.Count > 1)
        {
          exactMatches.RemoveAll(m => !IsMatchingWithInterface(m, @interface));
        }

        foreach (Type type in exactMatches)
        {
          services.TryAddTransient(@interface, type);
        }
      }

      if (!@interface.IsOpenGeneric())
      {
        AddConcretionsThatCouldBeClosed(@interface, concretions, services);
      }
    }

    foreach (Type @interface in genericInterfaces)
    {
      cancellationToken.ThrowIfCancellationRequested();

      List<Type> exactMatches = genericConcretions.Where(x => x.CanBeCastTo(@interface)).ToList();
      AddAllConcretionsThatClose(@interface, exactMatches, services, assembliesToScan, configuration, cancellationToken);
    }
  }

  private static bool IsMatchingWithInterface(Type? handlerType, Type handlerInterface)
  {
    if (handlerType == null || handlerInterface == null)
    {
      return false;
    }

    if (!handlerInterface.IsGenericType)
    {
      return false;
    }

    // Match on generic definition and arguments; a name lookup is ambiguous when the handler
    // implements the same interface for several requests.
    Type handlerInterfaceDefinition = handlerInterface.GetGenericTypeDefinition();

    return handlerType.GetInterfaces()
        .Any(type => type.IsGenericType
            && type.GetGenericTypeDefinition() == handlerInterfaceDefinition
            && type.GenericTypeArguments.SequenceEqual(handlerInterface.GenericTypeArguments));
  }

  private static void AddConcretionsThatCouldBeClosed(Type @interface, List<Type> concretions, IServiceCollection services)
  {
    foreach (Type? type in concretions
                 .Where(x => x.IsOpenGeneric() && x.CouldCloseTo(@interface)))
    {
      try
      {
        services.TryAddTransient(@interface, type.MakeGenericType(@interface.GenericTypeArguments));
      }
      catch (ArgumentException)
      {
        // MakeGenericType throws when the arguments violate the concretion's generic constraints;
        // such a concretion cannot close this interface, so skip it.
      }
    }
  }

  private static (Type Service, Type Implementation) GetConcreteRegistrationTypes(Type openRequestHandlerInterface, Type concreteGenericTRequest, Type openRequestHandlerImplementation)
  {
    Type[] closingTypes = concreteGenericTRequest.GetGenericArguments();

    Type? concreteTResponse = concreteGenericTRequest.GetInterfaces()
        .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IRequest<>))
        ?.GetGenericArguments()
        .FirstOrDefault();

    Type typeDefinition = openRequestHandlerInterface.GetGenericTypeDefinition();

    Type serviceType = concreteTResponse != null ?
        typeDefinition.MakeGenericType(concreteGenericTRequest, concreteTResponse) :
        typeDefinition.MakeGenericType(concreteGenericTRequest);

    return (serviceType, openRequestHandlerImplementation.MakeGenericType(closingTypes));
  }

  private static List<Type>? GetConcreteRequestTypes(Type openRequestHandlerInterface, Type openRequestHandlerImplementation, List<Type> candidateClosingTypes, MediatorServiceConfiguration configuration, CancellationToken cancellationToken)
  {
    Type requestType = openRequestHandlerInterface.GenericTypeArguments.First();

    // Only an open generic request (e.g. Request<T>) can be closed from the handler's parameters;
    // a bare generic parameter or a closed request (e.g. Ping) has no definition to close.
    if (requestType.IsGenericParameter || !requestType.IsGenericType || !requestType.ContainsGenericParameters)
      return null;

    Type requestGenericTypeDefinition = requestType.GetGenericTypeDefinition();

    //request generic type constraints
    List<List<Type>> typesThatCanCloseForEachParameter = openRequestHandlerImplementation
        .GetGenericArguments()
        .Select(x => x.GetGenericParameterConstraints())
        .Select(constraints => candidateClosingTypes
            .Where(type => constraints.All(constraint => constraint.IsAssignableFrom(type)))
            .ToList())
        .ToList();

    List<List<Type>> combinations = GenerateCombinations(requestType, typesThatCanCloseForEachParameter, configuration, 0, cancellationToken);

    return combinations.Select(types => requestGenericTypeDefinition.MakeGenericType(types.ToArray())).ToList();
  }

  // Method to generate combinations recursively
  public static List<List<Type>> GenerateCombinations(Type requestType, List<List<Type>> lists, MediatorServiceConfiguration configuration, int depth = 0, CancellationToken cancellationToken = default)
  {
    if (depth == 0)
    {
      int maxGenericTypeParameters = configuration.MaxGenericTypeParameters;
      int maxTypesClosing = configuration.MaxTypesClosing;
      int maxGenericTypeRegistrations = configuration.MaxGenericTypeRegistrations;

      // Initial checks
      if (maxGenericTypeParameters > 0 && lists.Count > maxGenericTypeParameters)
        throw new ArgumentException($"Error registering the generic type: {requestType.FullName}. The number of generic type parameters exceeds the maximum allowed ({maxGenericTypeParameters}).");

      foreach (List<Type> list in lists)
      {
        if (maxTypesClosing > 0 && list.Count > maxTypesClosing)
          throw new ArgumentException($"Error registering the generic type: {requestType.FullName}. One of the generic type parameter's count of types that can close exceeds the maximum length allowed ({maxTypesClosing}).");
      }

      // Calculate the total number of combinations
      if (maxGenericTypeRegistrations > 0)
      {
        long totalCombinations = 1;
        foreach (List<Type> list in lists)
        {
          // Saturate rather than overflow; any value past the cap fails the check below.
          totalCombinations = list.Count == 0 || totalCombinations <= long.MaxValue / list.Count
              ? totalCombinations * list.Count
              : long.MaxValue;
          if (totalCombinations > maxGenericTypeRegistrations)
            throw new ArgumentException($"Error registering the generic type: {requestType.FullName}. The total number of generic type registrations exceeds the maximum allowed ({maxGenericTypeRegistrations}).");
        }
      }
    }

    if (depth >= lists.Count)
      return new List<List<Type>> { new() };

    cancellationToken.ThrowIfCancellationRequested();

    List<Type> currentList = lists[depth];
    List<List<Type>> childCombinations = GenerateCombinations(requestType, lists, configuration, depth + 1, cancellationToken);
    List<List<Type>> combinations = new();

    foreach (Type item in currentList)
    {
      foreach (List<Type> childCombination in childCombinations)
      {
        List<Type> currentCombination = new()
                { item };
        currentCombination.AddRange(childCombination);
        combinations.Add(currentCombination);
      }
    }

    return combinations;
  }

  private static void AddAllConcretionsThatClose(Type openRequestInterface, List<Type> concretions, IServiceCollection services, IEnumerable<Assembly> assembliesToScan, MediatorServiceConfiguration configuration, CancellationToken cancellationToken)
  {
    // Scan the assemblies once; each generic parameter filters this list by its constraints.
    List<Type>? candidateClosingTypes = null;

    foreach (Type concretion in concretions)
    {
      candidateClosingTypes ??= assembliesToScan
          .SelectMany(assembly => assembly.DefinedTypes)
          .Where(type => type.IsClass && !type.IsAbstract)
          .Select(type => type.AsType())
          .ToList();

      List<Type>? concreteRequests = GetConcreteRequestTypes(openRequestInterface, concretion, candidateClosingTypes, configuration, cancellationToken);

      if (concreteRequests is null)
        continue;

      IEnumerable<(Type Service, Type Implementation)> registrationTypes = concreteRequests
          .Select(concreteRequest => GetConcreteRegistrationTypes(openRequestInterface, concreteRequest, concretion));

      foreach ((Type? service, Type? implementation) in registrationTypes)
      {
        cancellationToken.ThrowIfCancellationRequested();
        services.AddTransient(service, implementation);
      }
    }
  }

  internal static bool CouldCloseTo(this Type openConcretion, Type closedInterface)
  {
    Type openInterface = closedInterface.GetGenericTypeDefinition();
    Type[] arguments = closedInterface.GenericTypeArguments;

    Type[] concreteArguments = openConcretion.GenericTypeArguments;
    return arguments.Length == concreteArguments.Length && openConcretion.CanBeCastTo(openInterface);
  }

  private static bool CanBeCastTo(this Type pluggedType, Type pluginType)
  {
    if (pluggedType == null) return false;

    if (pluggedType == pluginType) return true;

    return pluginType.IsAssignableFrom(pluggedType);
  }

  private static bool IsOpenGeneric(this Type type)
  {
    return type.IsGenericTypeDefinition || type.ContainsGenericParameters;
  }

  internal static IEnumerable<Type> FindInterfacesThatClose(this Type pluggedType, Type templateType)
  {
    return FindInterfacesThatClosesCore(pluggedType, templateType).Distinct();
  }

  private static IEnumerable<Type> FindInterfacesThatClosesCore(Type pluggedType, Type templateType)
  {
    if (pluggedType == null) yield break;

    if (!pluggedType.IsConcrete()) yield break;

    foreach (
        Type? interfaceType in
        pluggedType.GetInterfaces()
            .Where(type => type.IsGenericType && (type.GetGenericTypeDefinition() == templateType)))
    {
      yield return interfaceType;
    }

    if (pluggedType.BaseType == typeof(object)) yield break;

    foreach (Type interfaceType in FindInterfacesThatClosesCore(pluggedType.BaseType!, templateType))
    {
      yield return interfaceType;
    }
  }

  private static bool IsConcrete(this Type type)
  {
    return !type.IsAbstract && !type.IsInterface;
  }

  private static void Fill<T>(this IList<T> list, T value)
  {
    if (list.Contains(value)) return;
    list.Add(value);
  }

  public static void AddRequiredServices(IServiceCollection services, MediatorServiceConfiguration serviceConfiguration)
  {
    // Use TryAdd, so any existing IMediator/ISender/IPublisher registration doesn't get overridden
    services.TryAdd(new ServiceDescriptor(typeof(IMediator), serviceConfiguration.MediatorImplementationType, serviceConfiguration.Lifetime));
    services.TryAdd(new ServiceDescriptor(typeof(ISender), sp => sp.GetRequiredService<IMediator>(), serviceConfiguration.Lifetime));
    services.TryAdd(new ServiceDescriptor(typeof(IPublisher), sp => sp.GetRequiredService<IMediator>(), serviceConfiguration.Lifetime));

    ServiceDescriptor notificationPublisherServiceDescriptor = serviceConfiguration.NotificationPublisherType != null
        ? new ServiceDescriptor(typeof(INotificationPublisher), serviceConfiguration.NotificationPublisherType, serviceConfiguration.Lifetime)
        : new ServiceDescriptor(typeof(INotificationPublisher), serviceConfiguration.NotificationPublisher);

    services.TryAdd(notificationPublisherServiceDescriptor);

    // Register pre processors, then post processors, then behaviors
    if (serviceConfiguration.RequestExceptionActionProcessorStrategy == RequestExceptionActionProcessorStrategy.ApplyForUnhandledExceptions)
    {
      RegisterBehaviorIfImplementationsExist(services, typeof(RequestExceptionActionProcessorBehavior<,>), typeof(IRequestExceptionAction<,>));
      RegisterBehaviorIfImplementationsExist(services, typeof(RequestExceptionProcessorBehavior<,>), typeof(IRequestExceptionHandler<,,>));
    }
    else
    {
      RegisterBehaviorIfImplementationsExist(services, typeof(RequestExceptionProcessorBehavior<,>), typeof(IRequestExceptionHandler<,,>));
      RegisterBehaviorIfImplementationsExist(services, typeof(RequestExceptionActionProcessorBehavior<,>), typeof(IRequestExceptionAction<,>));
    }

    if (serviceConfiguration.RequestPreProcessorsToRegister.Any())
    {
      services.TryAddEnumerable(new ServiceDescriptor(typeof(IPipelineBehavior<,>), typeof(RequestPreProcessorBehavior<,>), ServiceLifetime.Transient));
      services.TryAddEnumerable(serviceConfiguration.RequestPreProcessorsToRegister);
    }

    if (serviceConfiguration.RequestPostProcessorsToRegister.Any())
    {
      services.TryAddEnumerable(new ServiceDescriptor(typeof(IPipelineBehavior<,>), typeof(RequestPostProcessorBehavior<,>), ServiceLifetime.Transient));
      services.TryAddEnumerable(serviceConfiguration.RequestPostProcessorsToRegister);
    }

    foreach (ServiceDescriptor serviceDescriptor in serviceConfiguration.BehaviorsToRegister)
    {
      services.TryAddEnumerable(serviceDescriptor);
    }

    foreach (ServiceDescriptor serviceDescriptor in serviceConfiguration.StreamBehaviorsToRegister)
    {
      services.TryAddEnumerable(serviceDescriptor);
    }
  }

  /// <summary>
  /// Registers <paramref name="behaviorType"/> only when an implementation of <paramref name="subBehaviorType"/> is already
  /// in <paramref name="services"/>. The check runs once, at <c>AddMediator</c> time: exception handlers or actions registered
  /// after <c>AddMediator</c> do not cause the behavior to be added. Register them before calling <c>AddMediator</c>, or add
  /// the behavior explicitly with <c>AddOpenBehavior</c>.
  /// </summary>
  private static void RegisterBehaviorIfImplementationsExist(IServiceCollection services, Type behaviorType, Type subBehaviorType)
  {
    bool hasAnyRegistrationsOfSubBehaviorType = services
        .Where(service => !service.IsKeyedService)
        .Select(service => service.ImplementationType)
        .OfType<Type>()
        .SelectMany(type => type.GetInterfaces())
        .Where(type => type.IsGenericType)
        .Select(type => type.GetGenericTypeDefinition())
        .Any(type => type == subBehaviorType);

    if (hasAnyRegistrationsOfSubBehaviorType)
    {
      services.TryAddEnumerable(new ServiceDescriptor(typeof(IPipelineBehavior<,>), behaviorType, ServiceLifetime.Transient));
    }
  }
}