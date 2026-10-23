// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Extensions.Microsoft.DependencyInjection.Tests;

public class ServiceCollectionPipelineTests
{
  public class OuterBehavior : IPipelineBehavior<Ping, Pong>
  {
    private readonly Logger Logger;

    public OuterBehavior(Logger output)
    {
      Logger = output;
    }

    public async ValueTask<Pong> Handle(Ping request, MessageHandlerDelegate<Ping, Pong> next, CancellationToken cancellationToken)
    {
      Logger.Messages.Add("Outer before");
      Pong response = await next(request, cancellationToken);
      Logger.Messages.Add("Outer after");

      return response;
    }
  }

  public class InnerBehavior : IPipelineBehavior<Ping, Pong>
  {
    private readonly Logger Logger;

    public InnerBehavior(Logger output)
    {
      Logger = output;
    }

    public async ValueTask<Pong> Handle(Ping request, MessageHandlerDelegate<Ping, Pong> next, CancellationToken cancellationToken)
    {
      Logger.Messages.Add("Inner before");
      Pong response = await next(request, cancellationToken);
      Logger.Messages.Add("Inner after");

      return response;
    }
  }

  public class InnerBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
      where TRequest : notnull
  {
    private readonly Logger Logger;

    public InnerBehavior(Logger output)
    {
      Logger = output;
    }

    public async ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
      Logger.Messages.Add("Inner generic before");
      TResponse? response = await next(request, cancellationToken);
      Logger.Messages.Add("Inner generic after");

      return response;
    }
  }

  public class OuterBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
      where TRequest : notnull
  {
    private readonly Logger Logger;

    public OuterBehavior(Logger output)
    {
      Logger = output;
    }

    public async ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
      Logger.Messages.Add("Outer generic before");
      TResponse? response = await next(request, cancellationToken);
      Logger.Messages.Add("Outer generic after");

      return response;
    }
  }

  public class ConstrainedBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
      where TRequest : notnull
      where TResponse : Pong
  {
    private readonly Logger Logger;

    public ConstrainedBehavior(Logger output)
    {
      Logger = output;
    }

    public async ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
      Logger.Messages.Add("Constrained before");
      TResponse response = await next(request, cancellationToken);
      Logger.Messages.Add("Constrained after");

      return response;
    }
  }

  public class FirstPreProcessor<TRequest> : IRequestPreProcessor<TRequest> where TRequest : notnull
  {
    private readonly Logger Logger;

    public FirstPreProcessor(Logger output)
    {
      Logger = output;
    }
    public Task Process(TRequest request, CancellationToken cancellationToken)
    {
      Logger.Messages.Add("First pre processor");
      return Task.FromResult(0);
    }
  }

  public class FirstConcretePreProcessor : IRequestPreProcessor<Ping>
  {
    private readonly Logger Logger;

    public FirstConcretePreProcessor(Logger output)
    {
      Logger = output;
    }
    public Task Process(Ping request, CancellationToken cancellationToken)
    {
      Logger.Messages.Add("First concrete pre processor");
      return Task.FromResult(0);
    }
  }

  public class NextPreProcessor<TRequest> : IRequestPreProcessor<TRequest> where TRequest : notnull
  {
    private readonly Logger Logger;

    public NextPreProcessor(Logger output)
    {
      Logger = output;
    }
    public Task Process(TRequest request, CancellationToken cancellationToken)
    {
      Logger.Messages.Add("Next pre processor");
      return Task.FromResult(0);
    }
  }

  public class NextConcretePreProcessor : IRequestPreProcessor<Ping>
  {
    private readonly Logger Logger;

    public NextConcretePreProcessor(Logger output)
    {
      Logger = output;
    }
    public Task Process(Ping request, CancellationToken cancellationToken)
    {
      Logger.Messages.Add("Next concrete pre processor");
      return Task.FromResult(0);
    }
  }

  public class FirstPostProcessor<TRequest, TResponse> : IRequestPostProcessor<TRequest, TResponse>
      where TRequest : notnull
  {
    private readonly Logger Logger;

    public FirstPostProcessor(Logger output)
    {
      Logger = output;
    }
    public Task Process(TRequest request, TResponse response, CancellationToken cancellationToken)
    {
      Logger.Messages.Add("First post processor");
      return Task.FromResult(0);
    }
  }

  public class FirstConcretePostProcessor : IRequestPostProcessor<Ping, Pong>
  {
    private readonly Logger Logger;

    public FirstConcretePostProcessor(Logger output)
    {
      Logger = output;
    }
    public Task Process(Ping request, Pong response, CancellationToken cancellationToken)
    {
      Logger.Messages.Add("First concrete post processor");
      return Task.FromResult(0);
    }
  }

  public class NextPostProcessor<TRequest, TResponse> : IRequestPostProcessor<TRequest, TResponse>
      where TRequest : notnull
  {
    private readonly Logger Logger;

    public NextPostProcessor(Logger output)
    {
      Logger = output;
    }
    public Task Process(TRequest request, TResponse response, CancellationToken cancellationToken)
    {
      Logger.Messages.Add("Next post processor");
      return Task.FromResult(0);
    }
  }

  public class NextConcretePostProcessor : IRequestPostProcessor<Ping, Pong>
  {
    private readonly Logger Logger;

    public NextConcretePostProcessor(Logger output)
    {
      Logger = output;
    }
    public Task Process(Ping request, Pong response, CancellationToken cancellationToken)
    {
      Logger.Messages.Add("Next concrete post processor");
      return Task.FromResult(0);
    }
  }

  public class PingPongGenericExceptionAction : IRequestExceptionAction<Ping, Exception>
  {
    private readonly Logger Logger;

    public PingPongGenericExceptionAction(Logger output) => Logger = output;

    public Task Execute(Ping request, Exception exception, CancellationToken cancellationToken)
    {
      Logger.Messages.Add("Logging generic exception");

      return Task.CompletedTask;
    }
  }

  public class PingPongApplicationExceptionAction : IRequestExceptionAction<Ping, ApplicationException>
  {
    private readonly Logger Logger;

    public PingPongApplicationExceptionAction(Logger output) => Logger = output;

    public Task Execute(Ping request, ApplicationException exception, CancellationToken cancellationToken)
    {
      Logger.Messages.Add("Logging ApplicationException exception");

      return Task.CompletedTask;
    }
  }

  public class PingPongExceptionActionForType1 : IRequestExceptionAction<Ping, SystemException>
  {
    private readonly Logger Logger;

    public PingPongExceptionActionForType1(Logger output) => Logger = output;

    public Task Execute(Ping request, SystemException exception, CancellationToken cancellationToken)
    {
      Logger.Messages.Add("Logging exception 1");

      return Task.CompletedTask;
    }
  }

  public class PingPongExceptionActionForType2 : IRequestExceptionAction<Ping, SystemException>
  {
    private readonly Logger Logger;

    public PingPongExceptionActionForType2(Logger output) => Logger = output;

    public Task Execute(Ping request, SystemException exception, CancellationToken cancellationToken)
    {
      Logger.Messages.Add("Logging exception 2");

      return Task.CompletedTask;
    }
  }

  public class PingPongExceptionHandlerForType : IRequestExceptionHandler<Ping, Pong, ApplicationException>
  {
    public Task Handle(Ping request, ApplicationException exception, RequestExceptionHandlerState<Pong> state, CancellationToken cancellationToken)
    {
      state.SetHandled(new Pong { Message = exception.Message + " Handled by Specific Type" });

      return Task.CompletedTask;
    }
  }

  public class PingPongGenericExceptionHandler : IRequestExceptionHandler<Ping, Pong, Exception>
  {
    private readonly Logger Logger;

    public PingPongGenericExceptionHandler(Logger output) => Logger = output;

    public Task Handle(Ping request, Exception exception, RequestExceptionHandlerState<Pong> state, CancellationToken cancellationToken)
    {
      Logger.Messages.Add(exception.Message + " Logged by Generic Type");

      return Task.CompletedTask;
    }
  }

  public class NotAnOpenBehavior : IPipelineBehavior<Ping, Pong>
  {
    public ValueTask<Pong> Handle(Ping request, MessageHandlerDelegate<Ping, Pong> next, CancellationToken cancellationToken) => next(request, cancellationToken);
  }

  public class ThrowingBehavior : IPipelineBehavior<Ping, Pong>
  {
    public ValueTask<Pong> Handle(Ping request, MessageHandlerDelegate<Ping, Pong> next, CancellationToken cancellationToken) => throw new Exception(request.Message);
  }

  public class OpenBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
      where TRequest : notnull
  {
    public ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken) => next(request, cancellationToken);
  }

  public class MultiOpenBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
      where TRequest : notnull
  {
    public ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken) => next(request, cancellationToken);
  }

  [Fact]
  public async Task Should_wrap_with_behavior()
  {
    Logger output = new();
    IServiceCollection services = new ServiceCollection();
    services.AddSingleton(output);
    services.AddMediator(cfg =>
    {
      cfg.RegisterServicesFromAssembly(typeof(Ping).Assembly);
      cfg.AddBehavior<IPipelineBehavior<Ping, Pong>, OuterBehavior>();
      cfg.AddBehavior<IPipelineBehavior<Ping, Pong>, InnerBehavior>();
    });
    ServiceProvider provider = services.BuildServiceProvider();

    IMediator med = provider.GetRequiredService<IMediator>();

    Pong response = await med.Send(new Ping { Message = "Ping" });

    response.Message.ShouldBe("Ping Pong");

    output.Messages.ShouldBe(new[]
    {
            "Outer before",
            "Inner before",
            "Handler",
            "Inner after",
            "Outer after"
        });
  }

  [Fact]
  public async Task Should_wrap_generics_with_behavior()
  {
    Logger output = new();
    IServiceCollection services = new ServiceCollection();
    services.AddSingleton(output);
    services.AddMediator(cfg =>
    {
      // Call these registration methods multiple times to prove we don't register a service if it is already registered
      for (int i = 0; i < 3; i++)
      {
        cfg.AddOpenBehavior(typeof(OuterBehavior<,>));
        cfg.AddOpenBehavior(typeof(InnerBehavior<,>));
        cfg.RegisterServicesFromAssembly(typeof(Ping).Assembly);
      }
    });
    ServiceProvider provider = services.BuildServiceProvider();

    IMediator med = provider.GetRequiredService<IMediator>();

    Pong response = await med.Send(new Ping { Message = "Ping" });

    response.Message.ShouldBe("Ping Pong");

    output.Messages.ShouldBe(new[]
    {
            "Outer generic before",
            "Inner generic before",
            "Handler",
            "Inner generic after",
            "Outer generic after",
        });
  }

  [Fact]
  public async Task Should_register_pre_and_post_processors()
  {
    Logger output = new();
    IServiceCollection services = new ServiceCollection();
    services.AddSingleton(output);
    services.AddMediator(cfg =>
    {
      cfg.RegisterServicesFromAssembly(typeof(Ping).Assembly);
      cfg.AddRequestPreProcessor<IRequestPreProcessor<Ping>, FirstConcretePreProcessor>();
      cfg.AddRequestPreProcessor<IRequestPreProcessor<Ping>, NextConcretePreProcessor>();
      cfg.AddOpenRequestPreProcessor(typeof(FirstPreProcessor<>));
      cfg.AddOpenRequestPreProcessor(typeof(NextPreProcessor<>));
      cfg.AddRequestPostProcessor<IRequestPostProcessor<Ping, Pong>, FirstConcretePostProcessor>();
      cfg.AddRequestPostProcessor<IRequestPostProcessor<Ping, Pong>, NextConcretePostProcessor>();
      cfg.AddOpenRequestPostProcessor(typeof(FirstPostProcessor<,>));
      cfg.AddOpenRequestPostProcessor(typeof(NextPostProcessor<,>));
    });
    ServiceProvider provider = services.BuildServiceProvider();

    IMediator med = provider.GetRequiredService<IMediator>();

    Pong response = await med.Send(new Ping { Message = "Ping" });

    response.Message.ShouldBe("Ping Pong");

    output.Messages.ShouldBe(new[]
    {
            "First concrete pre processor",
            "Next concrete pre processor",
            "First pre processor",
            "Next pre processor",
            "Handler",
            "First concrete post processor",
            "Next concrete post processor",
            "First post processor",
            "Next post processor",
        });
  }

  [Fact]
  public async Task Should_pick_up_specific_exception_behaviors()
  {
    Logger output = new();
    IServiceCollection services = new ServiceCollection();
    services.AddSingleton(output);
    services.AddMediator(cfg => cfg.RegisterServicesFromAssembly(typeof(Ping).Assembly));
    ServiceProvider provider = services.BuildServiceProvider();

    IMediator med = provider.GetRequiredService<IMediator>();

    Pong response = await med.Send(new Ping { Message = "Ping", ThrowAction = msg => throw new ApplicationException(msg.Message + " Thrown") });

    response.Message.ShouldBe("Ping Thrown Handled by Specific Type");
    output.Messages.ShouldNotContain("Logging ApplicationException exception");
  }

  [Fact]
  public void Should_pick_up_base_exception_behaviors()
  {
    Logger output = new();
    IServiceCollection services = new ServiceCollection();
    services.AddSingleton(output);
    services.AddMediator(cfg => cfg.RegisterServicesFromAssembly(typeof(Ping).Assembly));
    ServiceProvider provider = services.BuildServiceProvider();

    IMediator med = provider.GetRequiredService<IMediator>();

    Should.Throw<Exception>(async () => await med.Send(new Ping { Message = "Ping", ThrowAction = msg => throw new Exception(msg.Message + " Thrown") }));

    output.Messages.ShouldContain("Ping Thrown Logged by Generic Type");
    output.Messages.ShouldContain("Logging generic exception");
  }

  [Fact]
  public void Should_handle_exceptions_from_behaviors()
  {
    Logger output = new();
    IServiceCollection services = new ServiceCollection();
    services.AddSingleton(output);
    services.AddMediator(cfg =>
    {
      cfg.RegisterServicesFromAssembly(typeof(Ping).Assembly);
      cfg.AddBehavior<ThrowingBehavior>();
    });
    ServiceProvider provider = services.BuildServiceProvider();

    IMediator med = provider.GetRequiredService<IMediator>();

    Should.Throw<Exception>(async () => await med.Send(new Ping { Message = "Ping" }));

    output.Messages.ShouldContain("Ping Logged by Generic Type");
    output.Messages.ShouldContain("Logging generic exception");
  }

  [Fact]
  public void Should_pick_up_exception_actions()
  {
    Logger output = new();
    IServiceCollection services = new ServiceCollection();
    services.AddSingleton(output);
    services.AddMediator(cfg => cfg.RegisterServicesFromAssembly(typeof(Ping).Assembly));
    ServiceProvider provider = services.BuildServiceProvider();

    IMediator med = provider.GetRequiredService<IMediator>();

    Should.Throw<SystemException>(async () => await med.Send(new Ping { Message = "Ping", ThrowAction = msg => throw new SystemException(msg.Message + " Thrown") }));

    output.Messages.ShouldContain("Logging exception 1");
    output.Messages.ShouldContain("Logging exception 2");
  }

  [Fact]
  public async Task Should_handle_constrained_generics()
  {
    Logger output = new();
    IServiceCollection services = new ServiceCollection();
    services.AddSingleton(output);
    services.AddMediator(cfg =>
    {
      cfg.RegisterServicesFromAssembly(typeof(Ping).Assembly);
      cfg.AddOpenBehavior(typeof(OuterBehavior<,>));
      cfg.AddOpenBehavior(typeof(InnerBehavior<,>));
      cfg.AddOpenBehavior(typeof(ConstrainedBehavior<,>));
      cfg.AddRequestPreProcessor<IRequestPreProcessor<Ping>, FirstConcretePreProcessor>();
      cfg.AddRequestPreProcessor<IRequestPreProcessor<Ping>, NextConcretePreProcessor>();
      cfg.AddOpenRequestPreProcessor(typeof(FirstPreProcessor<>));
      cfg.AddOpenRequestPreProcessor(typeof(NextPreProcessor<>));
      cfg.AddRequestPostProcessor<IRequestPostProcessor<Ping, Pong>, FirstConcretePostProcessor>();
      cfg.AddRequestPostProcessor<IRequestPostProcessor<Ping, Pong>, NextConcretePostProcessor>();
      cfg.AddOpenRequestPostProcessor(typeof(FirstPostProcessor<,>));
      cfg.AddOpenRequestPostProcessor(typeof(NextPostProcessor<,>));
    });
    ServiceProvider provider = services.BuildServiceProvider();

    IMediator med = provider.GetRequiredService<IMediator>();

    Pong response = await med.Send(new Ping { Message = "Ping" });

    response.Message.ShouldBe("Ping Pong");

    output.Messages.ShouldBe(new[]
    {
            "First concrete pre processor",
            "Next concrete pre processor",
            "First pre processor",
            "Next pre processor",
            "Outer generic before",
            "Inner generic before",
            "Constrained before",
            "Handler",
            "Constrained after",
            "Inner generic after",
            "Outer generic after",
            "First concrete post processor",
            "Next concrete post processor",
            "First post processor",
            "Next post processor"
        });

    output.Messages.Clear();

    Zong zingResponse = await med.Send(new Zing { Message = "Zing" });

    zingResponse.Message.ShouldBe("Zing Zong");

    output.Messages.ShouldBe(new[]
    {
            "First pre processor",
            "Next pre processor",
            "Outer generic before",
            "Inner generic before",
            "Handler",
            "Inner generic after",
            "Outer generic after",
            "First post processor",
            "Next post processor"
        });
  }

  [Fact]
  public void Should_throw_when_adding_non_open_behavior()
  {
    Should.Throw<InvalidOperationException>(() => new MediatorServiceConfiguration().AddOpenBehavior(typeof(NotAnOpenBehavior)));
  }

  [Fact]
  public void Should_throw_when_adding_random_generic_type_as_open_behavior()
  {
    Should.Throw<InvalidOperationException>(() => new MediatorServiceConfiguration().AddOpenBehavior(typeof(List<string>)));
  }

  [Fact]
  public void Should_handle_open_behavior_registration()
  {
    MediatorServiceConfiguration cfg = new();
    cfg.AddOpenBehavior(typeof(OpenBehavior<,>));

    cfg.BehaviorsToRegister.Count.ShouldBe(1);

    cfg.BehaviorsToRegister[0].ServiceType.ShouldBe(typeof(IPipelineBehavior<,>));
    cfg.BehaviorsToRegister[0].ImplementationType.ShouldBe(typeof(OpenBehavior<,>));
    cfg.BehaviorsToRegister[0].ImplementationFactory.ShouldBeNull();
    cfg.BehaviorsToRegister[0].ImplementationInstance.ShouldBeNull();
    cfg.BehaviorsToRegister[0].Lifetime.ShouldBe(ServiceLifetime.Transient);

    ServiceCollection services = new();

    cfg.RegisterServicesFromAssemblyContaining<Ping>();

    Should.NotThrow(() =>
    {
      services.AddMediator(cfg);
      services.BuildServiceProvider();
    });
  }

  [Fact]
  public void Should_handle_inferred_behavior_registration()
  {
    MediatorServiceConfiguration cfg = new();
    cfg.AddBehavior<InnerBehavior>();
    cfg.AddBehavior(typeof(OuterBehavior));

    cfg.BehaviorsToRegister.Count.ShouldBe(2);

    cfg.BehaviorsToRegister[0].ServiceType.ShouldBe(typeof(IPipelineBehavior<Ping, Pong>));
    cfg.BehaviorsToRegister[0].ImplementationType.ShouldBe(typeof(InnerBehavior));
    cfg.BehaviorsToRegister[0].ImplementationFactory.ShouldBeNull();
    cfg.BehaviorsToRegister[0].ImplementationInstance.ShouldBeNull();
    cfg.BehaviorsToRegister[0].Lifetime.ShouldBe(ServiceLifetime.Transient);
    cfg.BehaviorsToRegister[1].ServiceType.ShouldBe(typeof(IPipelineBehavior<Ping, Pong>));
    cfg.BehaviorsToRegister[1].ImplementationType.ShouldBe(typeof(OuterBehavior));
    cfg.BehaviorsToRegister[1].ImplementationFactory.ShouldBeNull();
    cfg.BehaviorsToRegister[1].ImplementationInstance.ShouldBeNull();
    cfg.BehaviorsToRegister[1].Lifetime.ShouldBe(ServiceLifetime.Transient);

    ServiceCollection services = new();

    cfg.RegisterServicesFromAssemblyContaining<Ping>();

    Should.NotThrow(() =>
    {
      services.AddMediator(cfg);
      services.BuildServiceProvider();
    });
  }

  [Fact]
  public void Should_handle_inferred_pre_processor_registration()
  {
    MediatorServiceConfiguration cfg = new();
    cfg.AddRequestPreProcessor<FirstConcretePreProcessor>();
    cfg.AddRequestPreProcessor(typeof(NextConcretePreProcessor));

    cfg.RequestPreProcessorsToRegister.Count.ShouldBe(2);

    cfg.RequestPreProcessorsToRegister[0].ServiceType.ShouldBe(typeof(IRequestPreProcessor<Ping>));
    cfg.RequestPreProcessorsToRegister[0].ImplementationType.ShouldBe(typeof(FirstConcretePreProcessor));
    cfg.RequestPreProcessorsToRegister[0].ImplementationFactory.ShouldBeNull();
    cfg.RequestPreProcessorsToRegister[0].ImplementationInstance.ShouldBeNull();
    cfg.RequestPreProcessorsToRegister[0].Lifetime.ShouldBe(ServiceLifetime.Transient);
    cfg.RequestPreProcessorsToRegister[1].ServiceType.ShouldBe(typeof(IRequestPreProcessor<Ping>));
    cfg.RequestPreProcessorsToRegister[1].ImplementationType.ShouldBe(typeof(NextConcretePreProcessor));
    cfg.RequestPreProcessorsToRegister[1].ImplementationFactory.ShouldBeNull();
    cfg.RequestPreProcessorsToRegister[1].ImplementationInstance.ShouldBeNull();
    cfg.RequestPreProcessorsToRegister[1].Lifetime.ShouldBe(ServiceLifetime.Transient);

    ServiceCollection services = new();

    cfg.RegisterServicesFromAssemblyContaining<Ping>();

    Should.NotThrow(() =>
    {
      services.AddMediator(cfg);
      services.BuildServiceProvider();
    });
  }

  [Fact]
  public void Should_handle_inferred_post_processor_registration()
  {
    MediatorServiceConfiguration cfg = new();
    cfg.AddRequestPostProcessor<FirstConcretePostProcessor>();
    cfg.AddRequestPostProcessor(typeof(NextConcretePostProcessor));

    cfg.RequestPostProcessorsToRegister.Count.ShouldBe(2);

    cfg.RequestPostProcessorsToRegister[0].ServiceType.ShouldBe(typeof(IRequestPostProcessor<Ping, Pong>));
    cfg.RequestPostProcessorsToRegister[0].ImplementationType.ShouldBe(typeof(FirstConcretePostProcessor));
    cfg.RequestPostProcessorsToRegister[0].ImplementationFactory.ShouldBeNull();
    cfg.RequestPostProcessorsToRegister[0].ImplementationInstance.ShouldBeNull();
    cfg.RequestPostProcessorsToRegister[0].Lifetime.ShouldBe(ServiceLifetime.Transient);
    cfg.RequestPostProcessorsToRegister[1].ServiceType.ShouldBe(typeof(IRequestPostProcessor<Ping, Pong>));
    cfg.RequestPostProcessorsToRegister[1].ImplementationType.ShouldBe(typeof(NextConcretePostProcessor));
    cfg.RequestPostProcessorsToRegister[1].ImplementationFactory.ShouldBeNull();
    cfg.RequestPostProcessorsToRegister[1].ImplementationInstance.ShouldBeNull();
    cfg.RequestPostProcessorsToRegister[1].Lifetime.ShouldBe(ServiceLifetime.Transient);

    ServiceCollection services = new();

    cfg.RegisterServicesFromAssemblyContaining<Ping>();

    Should.NotThrow(() =>
    {
      services.AddMediator(cfg);
      services.BuildServiceProvider();
    });
  }

  [Fact]
  public void Should_handle_open_behaviors_registration_from_a_single_type()
  {
    MediatorServiceConfiguration cfg = new();
    cfg.AddOpenBehavior(typeof(MultiOpenBehavior<,>), ServiceLifetime.Singleton);

    cfg.BehaviorsToRegister.Count.ShouldBe(1);

    cfg.BehaviorsToRegister[0].ServiceType.ShouldBe(typeof(IPipelineBehavior<,>));
    cfg.BehaviorsToRegister[0].ImplementationType.ShouldBe(typeof(MultiOpenBehavior<,>));
    cfg.BehaviorsToRegister[0].ImplementationFactory.ShouldBeNull();
    cfg.BehaviorsToRegister[0].ImplementationInstance.ShouldBeNull();
    cfg.BehaviorsToRegister[0].Lifetime.ShouldBe(ServiceLifetime.Singleton);

    ServiceCollection services = new();

    cfg.RegisterServicesFromAssemblyContaining<Ping>();

    Should.NotThrow(() =>
    {
      services.AddMediator(cfg);
      services.BuildServiceProvider();
    });
  }

  [Fact]
  public void Should_auto_register_processors_when_configured_including_all_concrete_types()
  {
    MediatorServiceConfiguration cfg = new()
    {
      AutoRegisterRequestProcessors = true
    };

    Logger output = new();
    IServiceCollection services = new ServiceCollection();
    services.AddSingleton(output);

    cfg.RegisterServicesFromAssemblyContaining<Ping>();

    services.AddMediator(cfg);

    ServiceProvider provider = services.BuildServiceProvider();

    List<object?> preProcessors = provider.GetServices(typeof(IRequestPreProcessor<Ping>)).ToList();
    preProcessors.Count.ShouldBeGreaterThan(0);
    preProcessors.ShouldContain(p => p != null && p.GetType() == typeof(FirstConcretePreProcessor));
    preProcessors.ShouldContain(p => p != null && p.GetType() == typeof(NextConcretePreProcessor));

    List<object?> postProcessors = provider.GetServices(typeof(IRequestPostProcessor<Ping, Pong>)).ToList();
    postProcessors.Count.ShouldBeGreaterThan(0);
    postProcessors.ShouldContain(p => p != null && p.GetType() == typeof(FirstConcretePostProcessor));
    postProcessors.ShouldContain(p => p != null && p.GetType() == typeof(NextConcretePostProcessor));
  }

  public sealed record FooRequest : IRequest;

  public interface IBlogger<T>
  {
    IList<string> Messages { get; }
  }

  public class Blogger<T> : IBlogger<T>
  {
    private readonly Logger Logger;

    public Blogger(Logger logger)
    {
      Logger = logger;
    }

    public IList<string> Messages => Logger.Messages;
  }

  public sealed class FooRequestHandler : IRequestHandler<FooRequest>
  {
    public FooRequestHandler(IBlogger<FooRequestHandler> logger)
    {
      Blogger = logger;
    }

    readonly IBlogger<FooRequestHandler> Blogger;

    public ValueTask Handle(FooRequest request, CancellationToken cancellationToken)
    {
      Blogger.Messages.Add("Invoked Handler");
      return default;
    }
  }

  sealed class ClosedBehavior : IPipelineBehavior<FooRequest, Unit>
  {
    public ClosedBehavior(IBlogger<ClosedBehavior> logger)
    {
      Blogger = logger;
    }

    readonly IBlogger<ClosedBehavior> Blogger;

    public ValueTask<Unit> Handle(FooRequest request, MessageHandlerDelegate<FooRequest, Unit> next, CancellationToken cancellationToken)
    {
      Blogger.Messages.Add("Invoked Closed Behavior");
      return next(request, cancellationToken);
    }
  }

  sealed class Open2Behavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
      where TRequest : notnull
  {
    public Open2Behavior(IBlogger<Open2Behavior<TRequest, TResponse>> logger)
    {
      Blogger = logger;
    }

    readonly IBlogger<Open2Behavior<TRequest, TResponse>> Blogger;

    public ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
      Blogger.Messages.Add("Invoked Open Behavior");
      return next(request, cancellationToken);
    }
  }
  [Fact]
  public async Task Should_register_correctly()
  {
    ServiceCollection services = new();
    services.AddMediator(cfg =>
    {
      cfg.RegisterServicesFromAssemblyContaining<FooRequest>();
      cfg.AddBehavior<ClosedBehavior>();
      cfg.AddOpenBehavior(typeof(Open2Behavior<,>));
    });
    Logger logger = new();
    services.AddSingleton(logger);
    services.AddSingleton(new TimeWarp.Mediator.Tests.PipelineTests.Logger());
    services.AddSingleton(new TimeWarp.Mediator.Tests.SendTests.Dependency());
    services.AddSingleton(new TimeWarp.Mediator.Tests.NotificationPublisherTests.HandlerGate());
    services.AddSingleton(new TimeWarp.Mediator.Tests.VarianceTests.EventLog());
    services.AddSingleton(new PipelineMultiCallToConstructorTests.ConstructorCallCounter());
    services.AddSingleton<System.IO.TextWriter>(new System.IO.StringWriter());
    services.AddTransient(typeof(IBlogger<>), typeof(Blogger<>));
    ServiceProvider provider = services.BuildServiceProvider(new ServiceProviderOptions
    {
      ValidateOnBuild = true
    });

    IMediator med = provider.GetRequiredService<IMediator>();
    FooRequest request = new();
    await med.Send(request);

    logger.Messages.ShouldBe(new[]
    {
            "Invoked Closed Behavior",
            "Invoked Open Behavior",
            "Invoked Handler",
        });
  }

  #region OpenBehaviorsForMultipleRegistration
  sealed class OpenBehaviorMultipleRegistration0<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
      where TRequest : notnull
  {
    public OpenBehaviorMultipleRegistration0(IBlogger<OpenBehaviorMultipleRegistration0<TRequest, TResponse>> logger)
    {
      Blogger = logger;
    }

    readonly IBlogger<OpenBehaviorMultipleRegistration0<TRequest, TResponse>> Blogger;

    public ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
      Blogger.Messages.Add("Invoked OpenBehaviorMultipleRegistration0");
      return next(request, cancellationToken);
    }
  }
  sealed class OpenBehaviorMultipleRegistration1<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
      where TRequest : notnull
  {
    public OpenBehaviorMultipleRegistration1(IBlogger<OpenBehaviorMultipleRegistration1<TRequest, TResponse>> logger)
    {
      Blogger = logger;
    }

    readonly IBlogger<OpenBehaviorMultipleRegistration1<TRequest, TResponse>> Blogger;

    public ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
      Blogger.Messages.Add("Invoked OpenBehaviorMultipleRegistration1");
      return next(request, cancellationToken);
    }
  }
  sealed class OpenBehaviorMultipleRegistration2<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
      where TRequest : notnull
  {
    public OpenBehaviorMultipleRegistration2(IBlogger<OpenBehaviorMultipleRegistration2<TRequest, TResponse>> logger)
    {
      Blogger = logger;
    }

    readonly IBlogger<OpenBehaviorMultipleRegistration2<TRequest, TResponse>> Blogger;

    public ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
      Blogger.Messages.Add("Invoked OpenBehaviorMultipleRegistration2");
      return next(request, cancellationToken);
    }
  }
  #endregion OpenBehaviorsForMultipleRegistration

  [Fact]
  public async Task Should_register_open_behaviors_correctly()
  {
    List<Type> behaviorTypeList = new()
        {
            typeof(OpenBehaviorMultipleRegistration0<,>),
            typeof(OpenBehaviorMultipleRegistration1<,>),
            typeof(OpenBehaviorMultipleRegistration2<,>)
        };
    ServiceCollection services = new();
    services.AddMediator(cfg =>
    {
      cfg.RegisterServicesFromAssemblyContaining<FooRequest>();
      cfg.AddOpenBehaviors(behaviorTypeList);
    });
    Logger logger = new();
    services.AddSingleton(logger);
    services.AddSingleton(new TimeWarp.Mediator.Tests.PipelineTests.Logger());
    services.AddSingleton(new TimeWarp.Mediator.Tests.SendTests.Dependency());
    services.AddSingleton(new TimeWarp.Mediator.Tests.NotificationPublisherTests.HandlerGate());
    services.AddSingleton(new TimeWarp.Mediator.Tests.VarianceTests.EventLog());
    services.AddSingleton(new PipelineMultiCallToConstructorTests.ConstructorCallCounter());
    services.AddSingleton<System.IO.TextWriter>(new System.IO.StringWriter());
    services.AddTransient(typeof(IBlogger<>), typeof(Blogger<>));
    ServiceProvider provider = services.BuildServiceProvider(new ServiceProviderOptions
    {
      ValidateOnBuild = true
    });

    IMediator med = provider.GetRequiredService<IMediator>();
    FooRequest request = new();
    await med.Send(request);

    logger.Messages.ShouldBe(new[]
    {
            "Invoked OpenBehaviorMultipleRegistration0",
            "Invoked OpenBehaviorMultipleRegistration1",
            "Invoked OpenBehaviorMultipleRegistration2",
            "Invoked Handler",
        });
  }
}