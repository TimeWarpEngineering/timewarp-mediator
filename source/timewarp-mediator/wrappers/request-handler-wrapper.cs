// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Wrappers;

internal abstract class RequestHandlerBase
{
  public abstract ValueTask<object?> Handle(object request, IServiceProvider serviceProvider,
      CancellationToken cancellationToken);
}

internal abstract class RequestHandlerWrapper<TResponse> : RequestHandlerBase
{
  public abstract ValueTask<TResponse> Handle(IRequest<TResponse> request, IServiceProvider serviceProvider,
      CancellationToken cancellationToken);
}

internal abstract class RequestHandlerWrapper : RequestHandlerBase
{
  public abstract ValueTask<Unit> Handle(IRequest request, IServiceProvider serviceProvider,
      CancellationToken cancellationToken);
}

internal class RequestHandlerWrapperImpl<TRequest, TResponse> : RequestHandlerWrapper<TResponse>
    where TRequest : IRequest<TResponse>
{
  public override async ValueTask<object?> Handle(object request, IServiceProvider serviceProvider,
      CancellationToken cancellationToken) =>
      await Handle((IRequest<TResponse>) request, serviceProvider, cancellationToken).ConfigureAwait(false);

  public override ValueTask<TResponse> Handle(IRequest<TResponse> request, IServiceProvider serviceProvider,
      CancellationToken cancellationToken)
  {
    TRequest typed = (TRequest) request;
    ValueTask<TResponse> Handler(TRequest message, CancellationToken token) =>
        serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>()
            .Handle(message, token);

    return serviceProvider
        .GetServices<IPipelineBehavior<TRequest, TResponse>>()
        .Reverse()
        .Aggregate((MessageHandlerDelegate<TRequest, TResponse>) Handler,
            (next, pipeline) => (message, token) => pipeline.Handle(message, next, token))(typed, cancellationToken);
  }
}

internal class RequestHandlerWrapperImpl<TRequest> : RequestHandlerWrapper
    where TRequest : IRequest
{
  public override async ValueTask<object?> Handle(object request, IServiceProvider serviceProvider,
      CancellationToken cancellationToken)
  {
    await Handle((IRequest) request, serviceProvider, cancellationToken).ConfigureAwait(false);
    return Unit.Value;
  }

  public override ValueTask<Unit> Handle(IRequest request, IServiceProvider serviceProvider,
      CancellationToken cancellationToken)
  {
    TRequest typed = (TRequest) request;

    async ValueTask<Unit> Handler(TRequest message, CancellationToken token)
    {
      await serviceProvider.GetRequiredService<IRequestHandler<TRequest>>()
          .Handle(message, token).ConfigureAwait(false);

      return Unit.Value;
    }

    return serviceProvider
        .GetServices<IPipelineBehavior<TRequest, Unit>>()
        .Reverse()
        .Aggregate((MessageHandlerDelegate<TRequest, Unit>) Handler,
            (next, pipeline) => (message, token) => pipeline.Handle(message, next, token))(typed, cancellationToken);
  }
}
