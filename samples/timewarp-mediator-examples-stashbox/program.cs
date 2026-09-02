// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Examples.Stashbox;

class Program
{
    static Task Main()
    {
        WrappingWriter writer = new(Console.Out);
        IMediator med = BuildMediator(writer);
        return Runner.Run(med, writer, "Stashbox", testStreams: true);
    }

    private static IMediator BuildMediator(WrappingWriter writer)
    {
        IStashboxContainer container = new StashboxContainer()
            .RegisterInstance<TextWriter>(writer)
            .RegisterAssemblies(new[] { typeof(Mediator).Assembly, typeof(Ping).Assembly }, 
                serviceTypeSelector: Rules.ServiceRegistrationFilters.Interfaces, registerSelf: false);

        return container.GetRequiredService<IMediator>();
    }
}
