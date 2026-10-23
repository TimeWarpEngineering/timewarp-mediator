// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Tests;

public class GenericTypeConstraintsTests
{
  public interface IGenericTypeRequestHandlerTestClass<TRequest>
      where TRequest : notnull
  {
    Type[] Handle(TRequest request);
  }

  public abstract class GenericTypeRequestHandlerTestClass<TRequest> : IGenericTypeRequestHandlerTestClass<TRequest>
      where TRequest : notnull
  {
    public bool IsIRequest { get; }

    public bool IsIRequestT { get; }

    public GenericTypeRequestHandlerTestClass()
    {
      IsIRequest = typeof(IRequest).IsAssignableFrom(typeof(TRequest));
      IsIRequestT = typeof(TRequest).GetInterfaces()
          .Any(x => x.GetTypeInfo().IsGenericType &&
                    x.GetGenericTypeDefinition() == typeof(IRequest<>));
    }

    public Type[] Handle(TRequest request)
    {
      return typeof(TRequest).GetInterfaces();
    }
  }

  public class GenericTypeConstraintPing : GenericTypeRequestHandlerTestClass<Ping>
  {

  }

  public class GenericTypeConstraintJing : GenericTypeRequestHandlerTestClass<Jing>
  {

  }

  public class Jing : IRequest
  {
    public string? Message { get; set; }
  }

  public class JingHandler : IRequestHandler<Jing>
  {
    public ValueTask Handle(Jing request, CancellationToken cancellationToken)
    {
      // empty handle
      return default;
    }
  }

  public class Ping : IRequest<Pong>
  {
    public string? Message { get; set; }
  }

  public class Pong
  {
    public string? Message { get; set; }
  }

  public class PingHandler : IRequestHandler<Ping, Pong>
  {
    public ValueTask<Pong> Handle(Ping request, CancellationToken cancellationToken)
    {
      return ValueTask.FromResult(new Pong { Message = request.Message + " Pong" });
    }
  }

  private readonly IMediator Mediator;

  public GenericTypeConstraintsTests()
  {
    Container container = new(cfg =>
    {
      cfg.Scan(scanner =>
          {
            scanner.AssemblyContainingType(typeof(GenericTypeConstraintsTests));
            scanner.IncludeNamespaceContainingType<Ping>();
            scanner.IncludeNamespaceContainingType<Jing>();
            scanner.WithDefaultConventions();
            scanner.AddAllTypesOf(typeof(IRequestHandler<,>));
            scanner.AddAllTypesOf(typeof(IRequestHandler<>));
          });
      cfg.For<IMediator>().Use<Mediator>();
    });

    Mediator = container.GetInstance<IMediator>();
  }

  [Fact]
  public async Task Should_Resolve_Void_Return_Request()
  {
    // Create Request
    Jing jing = new() { Message = "Jing" };

    // Test med still works sending request
    await Mediator.Send(jing);

    // Create new instance of type constrained class
    GenericTypeConstraintJing genericTypeConstraintsVoidReturn = new();

    // Assert it is of type IRequest and not IRequest<T>
    Assert.True(genericTypeConstraintsVoidReturn.IsIRequest);
    Assert.False(genericTypeConstraintsVoidReturn.IsIRequestT);

    Type[] results = genericTypeConstraintsVoidReturn.Handle(jing);

    Assert.Equal(1, results.Length);

    results.ShouldNotContain(typeof(IRequest<Unit>));
    results.ShouldContain(typeof(IRequest));
  }

  [Fact]
  public async Task Should_Resolve_Response_Return_Request()
  {
    // Create Request
    Ping ping = new() { Message = "Ping" };

    // Test med still works sending request and gets response
    Pong pingResponse = await Mediator.Send(ping);
    pingResponse.Message.ShouldBe("Ping Pong");

    // Create new instance of type constrained class
    GenericTypeConstraintPing genericTypeConstraintsResponseReturn = new();

    // Assert it is of type IRequest<T> but not IRequest
    Assert.False(genericTypeConstraintsResponseReturn.IsIRequest);
    Assert.True(genericTypeConstraintsResponseReturn.IsIRequestT);

    Type[] results = genericTypeConstraintsResponseReturn.Handle(ping);

    Assert.Equal(1, results.Length);

    results.ShouldContain(typeof(IRequest<Pong>));
    results.ShouldNotContain(typeof(IRequest));
  }
}