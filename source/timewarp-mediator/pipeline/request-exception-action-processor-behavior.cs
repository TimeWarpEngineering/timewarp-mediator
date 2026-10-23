// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Pipeline;

/// <summary>
/// Behavior for executing all <see cref="IRequestExceptionAction{TRequest,TException}"/> instances
///     after an exception is thrown by the following pipeline steps
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
public class RequestExceptionActionProcessorBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
  // Static on the closed generic, so each entry is keyed by (TRequest, TResponse, exception type).
  private static readonly ConcurrentDictionary<Type, (Type EnumerableActionType, MethodInfo ExecuteMethod)> ActionReflectionCache = new();
  private readonly IServiceProvider ServiceProvider;

  /// <summary>
  /// Initializes a new instance of the <see cref="RequestExceptionActionProcessorBehavior{TRequest,TResponse}"/> class.
  /// </summary>
  /// <param name="serviceProvider">Service provider used to resolve the <see cref="IRequestExceptionAction{TRequest,TException}"/> instances for each exception type</param>
  [RequiresUnreferencedCode(TrimmingMessages.UnreferencedCode)]
  [RequiresDynamicCode(TrimmingMessages.DynamicCode)]
  public RequestExceptionActionProcessorBehavior(IServiceProvider serviceProvider) => ServiceProvider = serviceProvider;

  /// <inheritdoc/>
  public async ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
  {
    try
    {
      return await next(request, cancellationToken).ConfigureAwait(false);
    }
    catch (Exception exception)
    {
      IEnumerable<Type> exceptionTypes = GetExceptionTypes(exception.GetType());

      List<(MethodInfo MethodInfo, object Action)> actionsForException = exceptionTypes
          .SelectMany(exceptionType => GetActionsForException(exceptionType, request))
          .GroupBy(static actionForException => actionForException.Action.GetType())
          .Select(static actionForException => actionForException.First())
          .Select(static actionForException => (MethodInfo: GetMethodInfoForAction(actionForException.ExceptionType), actionForException.Action))
          .ToList();

      foreach ((MethodInfo MethodInfo, object Action) actionForException in actionsForException)
      {
        try
        {
          await ((Task) (actionForException.MethodInfo.Invoke(actionForException.Action, new object[] { request, exception, cancellationToken })
                        ?? throw new InvalidOperationException($"Could not create task for action method {actionForException.MethodInfo}."))).ConfigureAwait(false);
        }
        catch (TargetInvocationException invocationException) when (invocationException.InnerException != null)
        {
          // Unwrap invocation exception to throw the actual error
          ExceptionDispatchInfo.Capture(invocationException.InnerException).Throw();
        }
      }

      throw;
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

  private IEnumerable<(Type ExceptionType, object Action)> GetActionsForException(Type exceptionType, TRequest request)
  {
    Type enumerableExceptionActionInterfaceType = GetActionReflection(exceptionType).EnumerableActionType;

    IEnumerable<object> actionsForException = (IEnumerable<object>) ServiceProvider.GetRequiredService(enumerableExceptionActionInterfaceType);

    return HandlersOrderer.Prioritize(actionsForException.ToList(), request)
        .Select(action => (exceptionType, action));
  }

  private static MethodInfo GetMethodInfoForAction(Type exceptionType) => GetActionReflection(exceptionType).ExecuteMethod;

  private static (Type EnumerableActionType, MethodInfo ExecuteMethod) GetActionReflection(Type exceptionType) =>
      ActionReflectionCache.GetOrAdd(exceptionType, static exceptionType =>
      {
        Type exceptionActionInterfaceType = typeof(IRequestExceptionAction<,>).MakeGenericType(typeof(TRequest), exceptionType);

        MethodInfo actionMethodInfo =
              exceptionActionInterfaceType.GetMethod(nameof(IRequestExceptionAction<TRequest, Exception>.Execute))
              ?? throw new InvalidOperationException(
                  $"Could not find method {nameof(IRequestExceptionAction<TRequest, Exception>.Execute)} on type {exceptionActionInterfaceType}");

        return (typeof(IEnumerable<>).MakeGenericType(exceptionActionInterfaceType), actionMethodInfo);
      });
}