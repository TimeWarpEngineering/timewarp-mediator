// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Extensions.Microsoft.DependencyInjection.Tests;

public class OpenGenericRegistrationTests
{
  public class OpenBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
      where TRequest : notnull
  {
    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        => next(cancellationToken);
  }

  public class OpenStreamBehavior<TRequest, TResponse> : IStreamPipelineBehavior<TRequest, TResponse>
      where TRequest : notnull
  {
    public IAsyncEnumerable<TResponse> Handle(TRequest request, StreamHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        => next();
  }

  public class OpenPreProcessor<TRequest> : IRequestPreProcessor<TRequest>
      where TRequest : notnull
  {
    public Task Process(TRequest request, CancellationToken cancellationToken) => Task.CompletedTask;
  }

  public class OpenPostProcessor<TRequest, TResponse> : IRequestPostProcessor<TRequest, TResponse>
      where TRequest : notnull
  {
    public Task Process(TRequest request, TResponse response, CancellationToken cancellationToken) => Task.CompletedTask;
  }

  public static IEnumerable<object[]> ClosedGenericRegistrations()
  {
    yield return new object[] { (Action<MediatorServiceConfiguration>) (cfg => cfg.AddOpenBehavior(typeof(OpenBehavior<Ping, Pong>))) };
    yield return new object[] { (Action<MediatorServiceConfiguration>) (cfg => cfg.AddOpenStreamBehavior(typeof(OpenStreamBehavior<Ping, Pong>))) };
    yield return new object[] { (Action<MediatorServiceConfiguration>) (cfg => cfg.AddOpenRequestPreProcessor(typeof(OpenPreProcessor<Ping>))) };
    yield return new object[] { (Action<MediatorServiceConfiguration>) (cfg => cfg.AddOpenRequestPostProcessor(typeof(OpenPostProcessor<Ping, Pong>))) };
  }

  public static IEnumerable<object[]> NullRegistrations()
  {
    yield return new object[] { (Action<MediatorServiceConfiguration>) (cfg => cfg.AddOpenBehavior(null!)) };
    yield return new object[] { (Action<MediatorServiceConfiguration>) (cfg => cfg.AddOpenStreamBehavior(null!)) };
    yield return new object[] { (Action<MediatorServiceConfiguration>) (cfg => cfg.AddOpenRequestPreProcessor(null!)) };
    yield return new object[] { (Action<MediatorServiceConfiguration>) (cfg => cfg.AddOpenRequestPostProcessor(null!)) };
  }

  [Theory]
  [MemberData(nameof(ClosedGenericRegistrations))]
  public void Should_reject_closed_generic_types(Action<MediatorServiceConfiguration> register)
  {
    MediatorServiceConfiguration configuration = new();

    InvalidOperationException exception = Should.Throw<InvalidOperationException>(() => register(configuration));

    exception.Message.ShouldContain("must be an open generic type definition");
  }

  [Theory]
  [MemberData(nameof(NullRegistrations))]
  public void Should_reject_null_types(Action<MediatorServiceConfiguration> register)
  {
    MediatorServiceConfiguration configuration = new();

    ArgumentNullException exception = Should.Throw<ArgumentNullException>(() => register(configuration));

    exception.ParamName.ShouldBe("openBehaviorType");
  }

  [Fact]
  public void Should_register_open_generic_types_once_against_their_interface()
  {
    MediatorServiceConfiguration configuration = new MediatorServiceConfiguration()
        .AddOpenBehavior(typeof(OpenBehavior<,>))
        .AddOpenStreamBehavior(typeof(OpenStreamBehavior<,>))
        .AddOpenRequestPreProcessor(typeof(OpenPreProcessor<>))
        .AddOpenRequestPostProcessor(typeof(OpenPostProcessor<,>));

    configuration.BehaviorsToRegister.ShouldHaveSingleItem().ServiceType.ShouldBe(typeof(IPipelineBehavior<,>));
    configuration.StreamBehaviorsToRegister.ShouldHaveSingleItem().ServiceType.ShouldBe(typeof(IStreamPipelineBehavior<,>));
    configuration.RequestPreProcessorsToRegister.ShouldHaveSingleItem().ServiceType.ShouldBe(typeof(IRequestPreProcessor<>));
    configuration.RequestPostProcessorsToRegister.ShouldHaveSingleItem().ServiceType.ShouldBe(typeof(IRequestPostProcessor<,>));
  }
}