// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Tests;

public class PipelineTests
{
  public class Ping : IRequest<Pong>
  {
    public string? Message { get; set; }
  }

  public class Pong
  {
    public string? Message { get; set; }
  }

  public class VoidPing : IRequest
  {
    public string? Message { get; set; }
  }

  public class Zing : IRequest<Zong>
  {
    public string? Message { get; set; }
  }

  public class Zong
  {
    public string? Message { get; set; }
  }

  public class PingHandler : IRequestHandler<Ping, Pong>
  {
    private readonly Logger Logger;

    public PingHandler(Logger output)
    {
      Logger = output;
    }
    public Task<Pong> Handle(Ping request, CancellationToken cancellationToken)
    {
      Logger.Messages.Add("Handler");
      return Task.FromResult(new Pong { Message = request.Message + " Pong" });
    }
  }

  public class VoidPingHandler : IRequestHandler<VoidPing>
  {
    private readonly Logger Logger;

    public VoidPingHandler(Logger output)
    {
      Logger = output;
    }
    public Task Handle(VoidPing request, CancellationToken cancellationToken)
    {
      Logger.Messages.Add("Handler");
      return Task.CompletedTask;
    }
  }

  public class ZingHandler : IRequestHandler<Zing, Zong>
  {
    private readonly Logger Logger;

    public ZingHandler(Logger output)
    {
      Logger = output;
    }
    public Task<Zong> Handle(Zing request, CancellationToken cancellationToken)
    {
      Logger.Messages.Add("Handler");
      return Task.FromResult(new Zong { Message = request.Message + " Zong" });
    }
  }

  public class OuterBehavior : IPipelineBehavior<Ping, Pong>
  {
    private readonly Logger Logger;

    public OuterBehavior(Logger output)
    {
      Logger = output;
    }

    public async Task<Pong> Handle(Ping request, RequestHandlerDelegate<Pong> next, CancellationToken cancellationToken)
    {
      Logger.Messages.Add("Outer before");
      Pong response = await next();
      Logger.Messages.Add("Outer after");

      return response;
    }
  }

  public class OuterVoidBehavior : IPipelineBehavior<VoidPing, Unit>
  {
    private readonly Logger Logger;

    public OuterVoidBehavior(Logger output)
    {
      Logger = output;
    }

    public async Task<Unit> Handle(VoidPing request, RequestHandlerDelegate<Unit> next, CancellationToken cancellationToken)
    {
      Logger.Messages.Add("Outer before");
      Unit response = await next();
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

    public async Task<Pong> Handle(Ping request, RequestHandlerDelegate<Pong> next, CancellationToken cancellationToken)
    {
      Logger.Messages.Add("Inner before");
      Pong response = await next();
      Logger.Messages.Add("Inner after");

      return response;
    }
  }

  public class InnerVoidBehavior : IPipelineBehavior<VoidPing, Unit>
  {
    private readonly Logger Logger;

    public InnerVoidBehavior(Logger output)
    {
      Logger = output;
    }

    public async Task<Unit> Handle(VoidPing request, RequestHandlerDelegate<Unit> next, CancellationToken cancellationToken)
    {
      Logger.Messages.Add("Inner before");
      Unit response = await next();
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

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
      Logger.Messages.Add("Inner generic before");
      TResponse? response = await next();
      Logger.Messages.Add("Inner generic after");

      return response;
    }
  }

  public class OuterBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
  {
    private readonly Logger Logger;

    public OuterBehavior(Logger output)
    {
      Logger = output;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
      Logger.Messages.Add("Outer generic before");
      TResponse? response = await next();
      Logger.Messages.Add("Outer generic after");

      return response;
    }
  }

  public class ConstrainedBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
      where TRequest : Ping
      where TResponse : Pong
  {
    private readonly Logger Logger;

    public ConstrainedBehavior(Logger output)
    {
      Logger = output;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
      Logger.Messages.Add("Constrained before");
      TResponse response = await next();
      Logger.Messages.Add("Constrained after");

      return response;
    }
  }

  public class ConcreteBehavior : IPipelineBehavior<Ping, Pong>
  {
    private readonly Logger Logger;

    public ConcreteBehavior(Logger output)
    {
      Logger = output;
    }

    public async Task<Pong> Handle(Ping request, RequestHandlerDelegate<Pong> next, CancellationToken cancellationToken)
    {
      Logger.Messages.Add("Concrete before");
      Pong response = await next();
      Logger.Messages.Add("Concrete after");

      return response;
    }
  }

  public class Logger
  {
    public IList<string> Messages { get; } = new List<string>();
  }

  [Fact]
  public async Task Should_wrap_with_behavior()
  {
    Logger output = new();
    Container container = new(cfg =>
    {
      cfg.Scan(scanner =>
          {
          scanner.AssemblyContainingType(typeof(PublishTests));
          scanner.IncludeNamespaceContainingType<Ping>();
          scanner.WithDefaultConventions();
          scanner.AddAllTypesOf(typeof(IRequestHandler<,>));
        });
      cfg.For<Logger>().Use(output);
      cfg.For<IPipelineBehavior<Ping, Pong>>().Add<OuterBehavior>();
      cfg.For<IPipelineBehavior<Ping, Pong>>().Add<InnerBehavior>();
      cfg.For<IMediator>().Use<Mediator>();
    });

    IMediator med = container.GetInstance<IMediator>();

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
  public async Task Should_wrap_void_with_behavior()
  {
    Logger output = new();
    Container container = new(cfg =>
    {
      cfg.Scan(scanner =>
          {
          scanner.AssemblyContainingType(typeof(PublishTests));
          scanner.IncludeNamespaceContainingType<Ping>();
          scanner.WithDefaultConventions();
          scanner.AddAllTypesOf(typeof(IRequestHandler<>));
        });
      cfg.For<Logger>().Use(output);
      cfg.For<IPipelineBehavior<VoidPing, Unit>>().Add<OuterVoidBehavior>();
      cfg.For<IPipelineBehavior<VoidPing, Unit>>().Add<InnerVoidBehavior>();
      cfg.For<IMediator>().Use<Mediator>();
    });

    IMediator med = container.GetInstance<IMediator>();

    await med.Send(new VoidPing { Message = "Ping" });

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
    Container container = new(cfg =>
    {
      cfg.Scan(scanner =>
          {
          scanner.AssemblyContainingType(typeof(PublishTests));
          scanner.IncludeNamespaceContainingType<Ping>();
          scanner.WithDefaultConventions();
          scanner.AddAllTypesOf(typeof(IRequestHandler<,>));
        });
      cfg.For<Logger>().Use(output);

      cfg.For(typeof(IPipelineBehavior<,>)).Add(typeof(OuterBehavior<,>));
      cfg.For(typeof(IPipelineBehavior<,>)).Add(typeof(InnerBehavior<,>));

      cfg.For<IMediator>().Use<Mediator>();
    });

    IMediator med = container.GetInstance<IMediator>();

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
  public async Task Should_wrap_void_generics_with_behavior()
  {
    Logger output = new();
    Container container = new(cfg =>
    {
      cfg.Scan(scanner =>
          {
          scanner.AssemblyContainingType(typeof(PublishTests));
          scanner.IncludeNamespaceContainingType<Ping>();
          scanner.WithDefaultConventions();
          scanner.AddAllTypesOf(typeof(IRequestHandler<,>));
          scanner.AddAllTypesOf(typeof(IRequestHandler<>));
        });
      cfg.For<Logger>().Use(output);

      cfg.For(typeof(IPipelineBehavior<,>)).Add(typeof(OuterBehavior<,>));
      cfg.For(typeof(IPipelineBehavior<,>)).Add(typeof(InnerBehavior<,>));

      cfg.For<IMediator>().Use<Mediator>();
    });

    IMediator med = container.GetInstance<IMediator>();

    await med.Send(new VoidPing { Message = "Ping" });

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
  public async Task Should_handle_constrained_generics()
  {
    Logger output = new();
    Container container = new(cfg =>
    {
      cfg.Scan(scanner =>
          {
          scanner.AssemblyContainingType(typeof(PublishTests));
          scanner.IncludeNamespaceContainingType<Ping>();
          scanner.WithDefaultConventions();
          scanner.AddAllTypesOf(typeof(IRequestHandler<,>));
        });
      cfg.For<Logger>().Use(output);

      cfg.For(typeof(IPipelineBehavior<,>)).Add(typeof(OuterBehavior<,>));
      cfg.For(typeof(IPipelineBehavior<,>)).Add(typeof(InnerBehavior<,>));
      cfg.For(typeof(IPipelineBehavior<,>)).Add(typeof(ConstrainedBehavior<,>));

      cfg.For<IMediator>().Use<Mediator>();
    });

    container.GetAllInstances<IPipelineBehavior<Ping, Pong>>();

    IMediator med = container.GetInstance<IMediator>();

    Pong response = await med.Send(new Ping { Message = "Ping" });

    response.Message.ShouldBe("Ping Pong");

    output.Messages.ShouldBe(new[]
    {
            "Outer generic before",
            "Inner generic before",
            "Constrained before",
            "Handler",
            "Constrained after",
            "Inner generic after",
            "Outer generic after",
        });

    output.Messages.Clear();

    Zong zingResponse = await med.Send(new Zing { Message = "Zing" });

    zingResponse.Message.ShouldBe("Zing Zong");

    output.Messages.ShouldBe(new[]
    {
            "Outer generic before",
            "Inner generic before",
            "Handler",
            "Inner generic after",
            "Outer generic after",
        });
  }

  // Lamar does not mix concrete and open generic registrations for one service type;
  // Microsoft.Extensions.DependencyInjection does, in registration order.
  [Fact]
  public async Task Should_handle_concrete_and_open_generics()
  {
    Logger output = new();
    ServiceCollection services = new();
    services.AddSingleton(output);
    services.AddTransient<IRequestHandler<Ping, Pong>, PingHandler>();
    services.AddTransient<IRequestHandler<Zing, Zong>, ZingHandler>();
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(OuterBehavior<,>));
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(InnerBehavior<,>));
    services.AddTransient<IPipelineBehavior<Ping, Pong>, ConcreteBehavior>();
    services.AddTransient<IMediator>(static serviceProvider => new Mediator(serviceProvider));

    IMediator med = services.BuildServiceProvider().GetRequiredService<IMediator>();

    Pong response = await med.Send(new Ping { Message = "Ping" });

    response.Message.ShouldBe("Ping Pong");

    output.Messages.ShouldBe(new[]
    {
            "Outer generic before",
            "Inner generic before",
            "Concrete before",
            "Handler",
            "Concrete after",
            "Inner generic after",
            "Outer generic after",
        });

    output.Messages.Clear();

    Zong zingResponse = await med.Send(new Zing { Message = "Zing" });

    zingResponse.Message.ShouldBe("Zing Zong");

    output.Messages.ShouldBe(new[]
    {
            "Outer generic before",
            "Inner generic before",
            "Handler",
            "Inner generic after",
            "Outer generic after",
        });
  }
}