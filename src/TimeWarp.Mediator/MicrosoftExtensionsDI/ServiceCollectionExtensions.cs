// Modified by Steven T. Cramer
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TimeWarp.Mediator;
using TimeWarp.Mediator.Pipeline;
using TimeWarp.Mediator.Registration;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extensions to scan for Mediator handlers and registers them.
/// - Scans for any handler interface implementations and registers them as <see cref="ServiceLifetime.Transient"/>
/// - Scans for any <see cref="IRequestPreProcessor{TRequest}"/> and <see cref="IRequestPostProcessor{TRequest,TResponse}"/> implementations and registers them as transient instances
/// Registers <see cref="IMediator"/> as a transient instance
/// After calling AddMediator you can use the container to resolve an <see cref="IMediator"/> instance.
/// This does not scan for any <see cref="IPipelineBehavior{TRequest,TResponse}"/> instances including <see cref="RequestPreProcessorBehavior{TRequest,TResponse}"/> and <see cref="RequestPreProcessorBehavior{TRequest,TResponse}"/>.
/// To register behaviors, use the <see cref="ServiceCollectionServiceExtensions.AddTransient(IServiceCollection,Type,Type)"/> with the open generic or closed generic types.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers handlers and med types from the specified assemblies
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">The action used to configure the options</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddMediator(this IServiceCollection services, 
        Action<MediatorServiceConfiguration> configuration)
    {
        var serviceConfig = new MediatorServiceConfiguration();

        configuration.Invoke(serviceConfig);

        return services.AddMediator(serviceConfig);
    }
    
    /// <summary>
    /// Registers handlers and med types from the specified assemblies
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration options</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddMediator(this IServiceCollection services, 
        MediatorServiceConfiguration configuration)
    {
        if (!configuration.AssembliesToRegister.Any())
        {
            throw new ArgumentException("No assemblies found to scan. Supply at least one assembly to scan for handlers.");
        }

        ServiceRegistrar.SetGenericRequestHandlerRegistrationLimitations(configuration);

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

        var message = new StringBuilder("TimeWarp Mediator Pipeline Registrations:");
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