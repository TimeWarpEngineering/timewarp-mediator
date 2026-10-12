// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Tests;

public class StreamPipelineTests
{
  public class Ping : IStreamRequest<Pong>
  {
    public string? Message { get; set; }
  }

  public class Pong
  {
    public string? Message { get; set; }
  }

  public class Zing : IStreamRequest<Zong>
  {
    public string? Message { get; set; }
  }

  public class Zong
  {
    public string? Message { get; set; }
  }

  public class PingHandler : IStreamRequestHandler<Ping, Pong>
  {
    private readonly Logger Logger;

    public PingHandler(Logger output)
    {
      Logger = output;
    }

    public async IAsyncEnumerable<Pong> Handle(Ping request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
      Logger.Messages.Add("Handler");
      yield return await Task.FromResult(new Pong { Message = request.Message + " Pong" });
    }
  }

  public class ZingHandler : IStreamRequestHandler<Zing, Zong>
  {
    private readonly Logger Logger;

    public ZingHandler(Logger output)
    {
      Logger = output;
    }

    public async IAsyncEnumerable<Zong> Handle(Zing request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
      Logger.Messages.Add("Handler");
      yield return await Task.FromResult(new Zong { Message = request.Message + " Zong" });
    }
  }

  public class OuterBehavior : IStreamPipelineBehavior<Ping, Pong>
  {
    private readonly Logger Logger;

    public OuterBehavior(Logger output)
    {
      Logger = output;
    }

    public async IAsyncEnumerable<Pong> Handle(Ping request, StreamHandlerDelegate<Pong> next, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
      Logger.Messages.Add("Outer before");
      await foreach (Pong result in next())
      {
        yield return result;
      }
      Logger.Messages.Add("Outer after");
    }
  }

  public class InnerBehavior : IStreamPipelineBehavior<Ping, Pong>
  {
    private readonly Logger Logger;

    public InnerBehavior(Logger output)
    {
      Logger = output;
    }

    public async IAsyncEnumerable<Pong> Handle(Ping request, StreamHandlerDelegate<Pong> next, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
      Logger.Messages.Add("Inner before");
      await foreach (Pong result in next())
      {
        yield return result;
      }
      Logger.Messages.Add("Inner after");
    }
  }

  public class InnerBehavior<TRequest, TResponse> : IStreamPipelineBehavior<TRequest, TResponse>
      where TRequest : IStreamRequest<TResponse>
  {
    private readonly Logger Logger;

    public InnerBehavior(Logger output)
    {
      Logger = output;
    }

    public async IAsyncEnumerable<TResponse> Handle(TRequest request, StreamHandlerDelegate<TResponse> next, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
      Logger.Messages.Add("Inner generic before");
      await foreach (TResponse? result in next())
      {
        yield return result;
      }
      Logger.Messages.Add("Inner generic after");
    }
  }

  public class OuterBehavior<TRequest, TResponse> : IStreamPipelineBehavior<TRequest, TResponse>
      where TRequest : IStreamRequest<TResponse>
  {
    private readonly Logger Logger;

    public OuterBehavior(Logger output)
    {
      Logger = output;
    }

    public async IAsyncEnumerable<TResponse> Handle(TRequest request, StreamHandlerDelegate<TResponse> next, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
      Logger.Messages.Add("Outer generic before");
      await foreach (TResponse? result in next())
      {
        yield return result;
      }
      Logger.Messages.Add("Outer generic after");
    }
  }

  public class ConstrainedBehavior<TRequest, TResponse> : IStreamPipelineBehavior<TRequest, TResponse>
      where TRequest : Ping, IStreamRequest<TResponse>
      where TResponse : Pong
  {
    private readonly Logger Logger;

    public ConstrainedBehavior(Logger output)
    {
      Logger = output;
    }
    public async IAsyncEnumerable<TResponse> Handle(TRequest request, StreamHandlerDelegate<TResponse> next, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
      Logger.Messages.Add("Constrained before");
      await foreach (TResponse result in next())
      {
        yield return result;
      }
      Logger.Messages.Add("Constrained after");
    }
  }

  public class ConcreteBehavior : IStreamPipelineBehavior<Ping, Pong>
  {
    private readonly Logger Logger;

    public ConcreteBehavior(Logger output)
    {
      Logger = output;
    }

    public async IAsyncEnumerable<Pong> Handle(Ping request, StreamHandlerDelegate<Pong> next, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
      Logger.Messages.Add("Concrete before");
      await foreach (Pong result in next())
      {
        yield return result;
      }
      Logger.Messages.Add("Concrete after");
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
          scanner.AddAllTypesOf(typeof(IStreamRequestHandler<,>));
        });
      cfg.For<Logger>().Use(output);
      cfg.For<IStreamPipelineBehavior<Ping, Pong>>().Add<OuterBehavior>();
      cfg.For<IStreamPipelineBehavior<Ping, Pong>>().Add<InnerBehavior>();
      cfg.For<IMediator>().Use<Mediator>();
    });

    IMediator med = container.GetInstance<IMediator>();

    await foreach (Pong? response in med.CreateStream(new Ping { Message = "Ping" }))
    {
      response.Message.ShouldBe("Ping Pong");
    }

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
          scanner.AddAllTypesOf(typeof(IStreamRequestHandler<,>));
        });
      cfg.For<Logger>().Use(output);

      cfg.For(typeof(IStreamPipelineBehavior<,>)).Add(typeof(OuterBehavior<,>));
      cfg.For(typeof(IStreamPipelineBehavior<,>)).Add(typeof(InnerBehavior<,>));

      cfg.For<IMediator>().Use<Mediator>();
    });

    IMediator med = container.GetInstance<IMediator>();

    await foreach (Pong? response in med.CreateStream(new Ping { Message = "Ping" }))
    {
      response.Message.ShouldBe("Ping Pong");
    }

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
          scanner.AddAllTypesOf(typeof(IStreamRequestHandler<,>));
        });
      cfg.For<Logger>().Use(output);

      cfg.For(typeof(IStreamPipelineBehavior<,>)).Add(typeof(OuterBehavior<,>));
      cfg.For(typeof(IStreamPipelineBehavior<,>)).Add(typeof(InnerBehavior<,>));
      cfg.For(typeof(IStreamPipelineBehavior<,>)).Add(typeof(ConstrainedBehavior<,>));

      cfg.For<IMediator>().Use<Mediator>();
    });

    IMediator med = container.GetInstance<IMediator>();

    await foreach (Pong? response in med.CreateStream(new Ping { Message = "Ping" }))
    {
      response.Message.ShouldBe("Ping Pong");
    }

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

    await foreach (Zong? response in med.CreateStream(new Zing { Message = "Zing" }))
    {
      response.Message.ShouldBe("Zing Zong");
    }

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
    services.AddTransient<IStreamRequestHandler<Ping, Pong>, PingHandler>();
    services.AddTransient<IStreamRequestHandler<Zing, Zong>, ZingHandler>();
    services.AddTransient(typeof(IStreamPipelineBehavior<,>), typeof(OuterBehavior<,>));
    services.AddTransient(typeof(IStreamPipelineBehavior<,>), typeof(InnerBehavior<,>));
    services.AddTransient<IStreamPipelineBehavior<Ping, Pong>, ConcreteBehavior>();
    services.AddTransient<IMediator>(static serviceProvider => new Mediator(serviceProvider));

    IMediator med = services.BuildServiceProvider().GetRequiredService<IMediator>();

    await foreach (Pong? response in med.CreateStream(new Ping { Message = "Ping" }))
    {
      response.Message.ShouldBe("Ping Pong");
    }

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

    await foreach (Zong? response in med.CreateStream(new Zing { Message = "Zing" }))
    {
      response.Message.ShouldBe("Zing Zong");
    }

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