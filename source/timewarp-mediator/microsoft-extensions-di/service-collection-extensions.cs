// Modified by Steven T. Cramer

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extensions to scan for Mediator handlers and register them.
/// - Scans for handler interface implementations and registers them as <see cref="ServiceLifetime.Transient"/>
/// - Registers <see cref="IMediator"/>, <see cref="ISender"/>, and <see cref="IPublisher"/> under <see cref="MediatorServiceConfiguration.Lifetime"/>
/// - Registers <see cref="RequestPreProcessorBehavior{TRequest,TResponse}"/> and <see cref="RequestPostProcessorBehavior{TRequest,TResponse}"/> when pre- or post-processors are configured
/// The reflection runtime is not trim or NativeAOT safe; use the source-generated <c>AddGeneratedMediator()</c> for those hosts.
/// After calling AddMediator you can use the container to resolve an <see cref="IMediator"/> instance.
/// Other <see cref="IPipelineBehavior{TRequest,TResponse}"/> implementations are not scanned; register them through
/// <see cref="MediatorServiceConfiguration.AddBehavior(Type, ServiceLifetime)"/> or <see cref="MediatorServiceConfiguration.AddOpenBehavior(Type, ServiceLifetime)"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
  /// <summary>
  /// Registers handlers and mediator types from the specified assemblies
  /// </summary>
  /// <param name="services">Service collection</param>
  /// <param name="configuration">The action used to configure the options</param>
  /// <returns>Service collection</returns>
  [RequiresUnreferencedCode(TrimmingMessages.UnreferencedCode)]
  [RequiresDynamicCode(TrimmingMessages.DynamicCode)]
  public static IServiceCollection AddMediator(this IServiceCollection services,
      Action<MediatorServiceConfiguration> configuration)
  {
    if (services is null)
    {
      throw new ArgumentNullException(nameof(services));
    }

    if (configuration is null)
    {
      throw new ArgumentNullException(nameof(configuration));
    }

    MediatorServiceConfiguration serviceConfig = new();

    configuration.Invoke(serviceConfig);

    return services.AddMediator(serviceConfig);
  }

  /// <summary>
  /// Registers handlers and mediator types from the specified assemblies
  /// </summary>
  /// <param name="services">Service collection</param>
  /// <param name="configuration">Configuration options</param>
  /// <returns>Service collection</returns>
  [RequiresUnreferencedCode(TrimmingMessages.UnreferencedCode)]
  [RequiresDynamicCode(TrimmingMessages.DynamicCode)]
  public static IServiceCollection AddMediator(this IServiceCollection services,
      MediatorServiceConfiguration configuration)
  {
    if (services is null)
    {
      throw new ArgumentNullException(nameof(services));
    }

    if (configuration is null)
    {
      throw new ArgumentNullException(nameof(configuration));
    }

    if (!configuration.AssembliesToRegister.Any())
    {
      throw new ArgumentException("No assemblies found to scan. Supply at least one assembly to scan for handlers.");
    }

    ServiceRegistrar.AddMediatorClassesWithTimeout(services, configuration);

    ServiceRegistrar.AddRequiredServices(services, configuration);

    return services;
  }

  /// <summary>
  /// Gets information about the registered Mediator pipeline components (preprocessors, behaviors, and postprocessors).
  /// </summary>
  /// <param name="services">Service collection</param>
  /// <returns>A string containing the formatted pipeline information</returns>
  public static string GetPipelineInfo(this IServiceCollection services)
  {
    List<string> preprocessors = GetComponentOrder(services, typeof(IRequestPreProcessor<>));
    List<string> behaviors = GetComponentOrder(services, typeof(IPipelineBehavior<,>));
    List<string> postprocessors = GetComponentOrder(services, typeof(IRequestPostProcessor<,>));

    StringBuilder message = new("TimeWarp Mediator Pipeline Registrations:");
    message.AppendLine();
    message.AppendLine();

    AppendComponentOrder(message, "Preprocessors", preprocessors);
    AppendComponentOrder(message, "Behaviors", behaviors);
    AppendComponentOrder(message, "Postprocessors", postprocessors);

    return message.ToString();
  }

  private static List<string> GetComponentOrder(IServiceCollection services, Type componentType)
  {
    return services
        .Where(sd => sd.ServiceType.IsGenericType &&
            sd.ServiceType.GetGenericTypeDefinition() == componentType)
        .Select(sd => sd.ImplementationType?.Name ?? "Unknown")
        .Select(name => name.Split('`')[0])
        .ToList();
  }

  private static void AppendComponentOrder(StringBuilder message, string componentType, IReadOnlyList<string> order)
  {
    message.AppendLine($"{componentType}:");
    if (order.Count == 0)
    {
      message.AppendLine("  (none)");
    }
    else
    {
      for (int i = 0; i < order.Count; i++)
      {
        message.AppendLine($"  {i + 1}. {order[i]}");
      }
    }
    message.AppendLine();
  }
}