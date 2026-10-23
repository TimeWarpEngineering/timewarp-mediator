// Modified by Steven T. Cramer

namespace TimeWarp.Mediator;

/// <summary>
/// Legacy reflection dispatcher (MakeGenericType + wrapper caches). Prefer the source-generated
/// <c>TimeWarp.Mediator.Generated.Mediator</c> from TimeWarp.Mediator.Generators when present.
/// Not trim or NativeAOT safe.
/// </summary>
public class Mediator : IMediator
{
  private readonly IServiceProvider ServiceProvider;
  private readonly INotificationPublisher NotificationPublisher;
  // Typed caches are keyed by (request type, response type): a request type can implement several request
  // interfaces (or IRequest and IRequest<T>), so the request type alone does not identify the wrapper.
  // The untyped Send(object) path has its own cache keyed by request type.
  private static readonly ConcurrentDictionary<(Type RequestType, Type ResponseType), RequestHandlerBase> RequestHandlers = new();
  private static readonly ConcurrentDictionary<Type, RequestHandlerBase> ObjectRequestHandlers = new();
  private static readonly ConcurrentDictionary<Type, NotificationHandlerWrapper> NotificationHandlers = new();

  /// <summary>
  /// Initializes a new instance of the <see cref="Mediator"/> class.
  /// </summary>
  /// <param name="serviceProvider">Service provider. Can be a scoped or root provider</param>
  [RequiresUnreferencedCode(TrimmingMessages.UnreferencedCode)]
  [RequiresDynamicCode(TrimmingMessages.DynamicCode)]
  public Mediator(IServiceProvider serviceProvider)
      : this(serviceProvider, new ForeachAwaitPublisher()) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="Mediator"/> class.
  /// </summary>
  /// <param name="serviceProvider">Service provider. Can be a scoped or root provider</param>
  /// <param name="publisher">Notification publisher. Defaults to <see cref="ForeachAwaitPublisher"/>.</param>
  [RequiresUnreferencedCode(TrimmingMessages.UnreferencedCode)]
  [RequiresDynamicCode(TrimmingMessages.DynamicCode)]
  public Mediator(IServiceProvider serviceProvider, INotificationPublisher publisher)
  {
    ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    NotificationPublisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
  }

  /// <inheritdoc/>
  public ValueTask<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
  {
    if (request == null)
    {
      throw new ArgumentNullException(nameof(request));
    }

    RequestHandlerWrapper<TResponse> handler = (RequestHandlerWrapper<TResponse>) RequestHandlers.GetOrAdd((request.GetType(), typeof(TResponse)), static key =>
    {
      Type wrapperType = typeof(RequestHandlerWrapperImpl<,>).MakeGenericType(key.RequestType, key.ResponseType);
      object wrapper = Activator.CreateInstance(wrapperType) ?? throw new InvalidOperationException($"Could not create wrapper type for {key.RequestType}");
      return (RequestHandlerBase) wrapper;
    });

    return handler.Handle(request, ServiceProvider, cancellationToken);
  }

  /// <inheritdoc/>
  public async ValueTask Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
      where TRequest : IRequest
  {
    if (request == null)
    {
      throw new ArgumentNullException(nameof(request));
    }

    // typeof(void) marks the void IRequest wrapper so it cannot collide with an IRequest<T> wrapper.
    RequestHandlerWrapper handler = (RequestHandlerWrapper) RequestHandlers.GetOrAdd((request.GetType(), typeof(void)), static key =>
    {
      Type wrapperType = typeof(RequestHandlerWrapperImpl<>).MakeGenericType(key.RequestType);
      object wrapper = Activator.CreateInstance(wrapperType) ?? throw new InvalidOperationException($"Could not create wrapper type for {key.RequestType}");
      return (RequestHandlerBase) wrapper;
    });

    await handler.Handle(request, ServiceProvider, cancellationToken).ConfigureAwait(false);
  }

  /// <inheritdoc/>
  public ValueTask<object?> Send(object request, CancellationToken cancellationToken = default)
  {
    if (request == null)
    {
      throw new ArgumentNullException(nameof(request));
    }

    RequestHandlerBase handler = ObjectRequestHandlers.GetOrAdd(request.GetType(), static requestType =>
    {
      Type wrapperType;

      Type? requestInterfaceType = requestType.GetInterfaces().FirstOrDefault(static i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>));
      if (requestInterfaceType is null)
      {
        requestInterfaceType = requestType.GetInterfaces().FirstOrDefault(static i => i == typeof(IRequest));
        if (requestInterfaceType is null)
        {
          throw new ArgumentException($"{requestType.Name} does not implement {nameof(IRequest)}", nameof(request));
        }

        wrapperType = typeof(RequestHandlerWrapperImpl<>).MakeGenericType(requestType);
      }
      else
      {
        Type responseType = requestInterfaceType.GetGenericArguments()[0];
        wrapperType = typeof(RequestHandlerWrapperImpl<,>).MakeGenericType(requestType, responseType);
      }

      object wrapper = Activator.CreateInstance(wrapperType) ?? throw new InvalidOperationException($"Could not create wrapper for type {requestType}");
      return (RequestHandlerBase) wrapper;
    });

    // the cached wrapper is already closed over the request type, so this is a virtual call, not reflection
    return handler.Handle(request, ServiceProvider, cancellationToken);
  }

  /// <inheritdoc/>
  public ValueTask Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
      where TNotification : INotification
  {
    if (notification == null)
    {
      throw new ArgumentNullException(nameof(notification));
    }

    return new ValueTask(PublishNotification(notification, cancellationToken));
  }

  /// <inheritdoc/>
  public ValueTask Publish(object notification, CancellationToken cancellationToken = default)
  {
    switch (notification)
    {
      case null:
        throw new ArgumentNullException(nameof(notification));
      case INotification instance:
        return new ValueTask(PublishNotification(instance, cancellationToken));
      default:
        throw new ArgumentException($"{notification.GetType().Name} does not implement {nameof(INotification)}", nameof(notification));
    }
  }

  /// <summary>
  /// Override in a derived class to control how the tasks are awaited. By default the implementation calls the <see cref="INotificationPublisher"/>.
  /// </summary>
  /// <param name="handlerExecutors">Enumerable of tasks representing invoking each notification handler</param>
  /// <param name="notification">The notification being published</param>
  /// <param name="cancellationToken">The cancellation token</param>
  /// <returns>A task representing invoking all handlers</returns>
  protected virtual Task PublishCore(IEnumerable<NotificationHandlerExecutor> handlerExecutors, INotification notification, CancellationToken cancellationToken)
      => NotificationPublisher.Publish(handlerExecutors, notification, cancellationToken);

  private Task PublishNotification(INotification notification, CancellationToken cancellationToken = default)
  {
    NotificationHandlerWrapper handler = NotificationHandlers.GetOrAdd(notification.GetType(), static notificationType =>
    {
      Type wrapperType = typeof(NotificationHandlerWrapperImpl<>).MakeGenericType(notificationType);
      object wrapper = Activator.CreateInstance(wrapperType) ?? throw new InvalidOperationException($"Could not create wrapper for type {notificationType}");
      return (NotificationHandlerWrapper) wrapper;
    });

    return handler.Handle(notification, ServiceProvider, PublishCore, cancellationToken);
  }
}