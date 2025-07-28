// Modified by Steven T. Cramer
using System;
using System.IO;
using System.Threading.Tasks;
using TimeWarp.Mediator.Pipeline;
using Microsoft.Extensions.DependencyInjection;


namespace TimeWarp.Mediator.Examples.AspNetCore;

public static class Program
{
    public static Task Main(string[] args)
    {
        var writer = new WrappingWriter(Console.Out);
        var med = BuildMediator(writer);
        return Runner.Run(med, writer, "ASP.NET Core DI", testStreams: true);
    }

    private static IMediator BuildMediator(WrappingWriter writer)
    {
        var services = new ServiceCollection();

        services.AddSingleton<TextWriter>(writer);

        services.AddMediator(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(typeof(Ping).Assembly, typeof(Sing).Assembly);
        });

        services.AddScoped(typeof(IStreamRequestHandler<Sing, Song>), typeof(SingHandler));

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(GenericPipelineBehavior<,>));
        services.AddScoped(typeof(IRequestPreProcessor<>), typeof(GenericRequestPreProcessor<>));
        services.AddScoped(typeof(IRequestPostProcessor<,>), typeof(GenericRequestPostProcessor<,>));
        services.AddScoped(typeof(IStreamPipelineBehavior<,>), typeof(GenericStreamPipelineBehavior<,>));

        var provider = services.BuildServiceProvider();

        return provider.GetRequiredService<IMediator>();
    }
}