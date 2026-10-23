// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Pipeline;

/// <summary>
/// Behavior for executing all <see cref="IRequestExceptionHandler{TRequest,TResponse,TException}"/> instances
///     after an exception is thrown by the following pipeline steps
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
public class RequestExceptionProcessorBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
  // Static on the closed generic, so each entry is keyed by (TRequest, TResponse, exception type).
  private static readonly ConcurrentDictionary<Type, (Type EnumerableHandlerType, MethodInfo HandleMethod)> HandlerReflectionCache = new();
  private readonly IServiceProvider ServiceProvider;

  /// <summary>
  /// Initializes a new instance of the <see cref="RequestExceptionProcessorBehavior{TRequest,TResponse}"/> class.
  /// </summary>
  /// <param name="serviceProvider">Service provider used to resolve the <see cref="IRequestExceptionHandler{TRequest,TResponse,TException}"/> instances for each exception type</param>
  [RequiresUnreferencedCode(TrimmingMessages.UnreferencedCode)]
  [RequiresDynamicCode(TrimmingMessages.DynamicCode)]
  public RequestExceptionProcessorBehavior(IServiceProvider serviceProvider) => ServiceProvider = serviceProvider;

  /// <inheritdoc/>
  public async ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
  {
    try
    {
      return await next(request, cancellationToken).ConfigureAwait(false);
    }
    catch (Exception exception)
    {
      RequestExceptionHandlerState<TResponse> state = new();

      IEnumerable<Type> exceptionTypes = GetExceptionTypes(exception.GetType());

      List<(MethodInfo MethodInfo, object Handler)> handlersForException = exceptionTypes
          .SelectMany(exceptionType => GetHandlersForException(exceptionType, request))
          .GroupBy(static handlerForException => handlerForException.Handler.GetType())
          .Select(static handlerForException => handlerForException.First())
          .Select(static handlerForException => (MethodInfo: GetMethodInfoForHandler(handlerForException.ExceptionType), handlerForException.Handler))
          .ToList();

      foreach ((MethodInfo MethodInfo, object Handler) handlerForException in handlersForException)
      {
        try
        {
          await ((Task) (handlerForException.MethodInfo.Invoke(handlerForException.Handler, new object[] { request, exception, state, cancellationToken })
                         ?? throw new InvalidOperationException("Did not return a Task from the exception handler."))).ConfigureAwait(false);
        }
        catch (TargetInvocationException invocationException) when (invocationException.InnerException != null)
        {
          // Unwrap invocation exception to throw the actual error
          ExceptionDispatchInfo.Capture(invocationException.InnerException).Throw();
        }

        if (state.Handled)
        {
          break;
        }
      }

      if (!state.Handled)
      {
        throw;
      }

      // Handled is authoritative: a handler may legitimately mark the exception handled with a null response.
      return state.Response!;
    }
  }
  private static IEnumerable<Type> GetExceptionTypes(Type? exceptionType)
  {
    while (exceptionType != null && exceptionType != typeof(object))
    {
      yield return exceptionType;
      exceptionType = exceptionType.BaseType;
    }
  }

  private IEnumerable<(Type ExceptionType, object Handler)> GetHandlersForException(Type exceptionType, TRequest request)
  {
    Type enumerableExceptionHandlerInterfaceType = GetHandlerReflection(exceptionType).EnumerableHandlerType;

    IEnumerable<object> exceptionHandlers = (IEnumerable<object>) ServiceProvider.GetRequiredService(enumerableExceptionHandlerInterfaceType);

    return HandlersOrderer.Prioritize(exceptionHandlers.ToList(), request)
        .Select(handler => (exceptionType, action: handler));
  }

  private static MethodInfo GetMethodInfoForHandler(Type exceptionType) => GetHandlerReflection(exceptionType).HandleMethod;

  private static (Type EnumerableHandlerType, MethodInfo HandleMethod) GetHandlerReflection(Type exceptionType) =>
      HandlerReflectionCache.GetOrAdd(exceptionType, static exceptionType =>
      {
        Type exceptionHandlerInterfaceType = typeof(IRequestExceptionHandler<,,>).MakeGenericType(typeof(TRequest), typeof(TResponse), exceptionType);

        MethodInfo handleMethodInfo = exceptionHandlerInterfaceType.GetMethod(nameof(IRequestExceptionHandler<TRequest, TResponse, Exception>.Handle))
                             ?? throw new InvalidOperationException($"Could not find method {nameof(IRequestExceptionHandler<TRequest, TResponse, Exception>.Handle)} on type {exceptionHandlerInterfaceType}");

        return (typeof(IEnumerable<>).MakeGenericType(exceptionHandlerInterfaceType), handleMethodInfo);
      });
}