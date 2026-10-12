// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Examples.PublishStrategies;

public class CustomMediator : Mediator
{
  private readonly Func<IEnumerable<NotificationHandlerExecutor>, INotification, CancellationToken, Task> PublishStrategy;

  public CustomMediator(IServiceProvider serviceFactory, Func<IEnumerable<NotificationHandlerExecutor>, INotification, CancellationToken, Task> publish) : base(serviceFactory)
      => PublishStrategy = publish;

  protected override Task PublishCore(IEnumerable<NotificationHandlerExecutor> handlerExecutors, INotification notification, CancellationToken cancellationToken)
      => PublishStrategy(handlerExecutors, notification, cancellationToken);
}