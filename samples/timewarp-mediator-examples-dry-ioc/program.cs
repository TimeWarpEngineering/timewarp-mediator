// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Examples.DryIoc;

class Program
{
    static Task Main()
    {
        WrappingWriter writer = new(Console.Out);
        IMediator med = BuildMediator(writer);

        return Runner.Run(med, writer, "DryIoc");
    }

    private static IMediator BuildMediator(WrappingWriter writer)
    {
        Container container = new();
        // Since Mediator has multiple constructors, consider adding rule to allow that
        // var container = new Container(rules => rules.With(FactoryMethod.ConstructorWithResolvableArguments))

        container.Use<TextWriter>(writer);

        //Pipeline works out of the box here

        container.RegisterMany(new[] { typeof(IMediator).GetAssembly(), typeof(Ping).GetAssembly() }, Registrator.Interfaces);
        //Without the container having FactoryMethod.ConstructorWithResolvableArguments commented above
        //You must select the desired constructor
        container.Register<IMediator, Mediator>(made: Made.Of(() => new Mediator(Arg.Of<IServiceProvider>())));

        ServiceCollection services = new();

        IContainer adapterContainer = container.WithDependencyInjectionAdapter(services);

        return adapterContainer.GetRequiredService<IMediator>();
    }
}
