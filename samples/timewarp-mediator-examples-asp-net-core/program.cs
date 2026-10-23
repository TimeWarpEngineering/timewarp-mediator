// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Examples.AspNetCore;

public static class Program
{
  public static Task Main(string[] args)
  {
    WrappingWriter writer = new(Console.Out);
    IMediator med = BuildMediator(writer);
    return Runner.Run(med, writer, "ASP.NET Core DI");
  }

  private static IMediator BuildMediator(WrappingWriter writer)
  {
    ServiceCollection services = new();

    services.AddSingleton<TextWriter>(writer);

    services.AddMediator(cfg =>
    {
      cfg.RegisterServicesFromAssembly(typeof(Ping).Assembly);
    });

    services.AddScoped(typeof(IPipelineBehavior<,>), typeof(GenericPipelineBehavior<,>));
    services.AddScoped(typeof(IRequestPreProcessor<>), typeof(GenericRequestPreProcessor<>));
    services.AddScoped(typeof(IRequestPostProcessor<,>), typeof(GenericRequestPostProcessor<,>));

    // Demonstrate GetPipelineInfo extension
    string pipelineInfo = services.GetPipelineInfo();
    writer.WriteLine(pipelineInfo);

    ServiceProvider provider = services.BuildServiceProvider();

    return provider.GetRequiredService<IMediator>();
  }
}