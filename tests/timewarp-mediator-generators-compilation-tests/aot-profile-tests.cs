#region Purpose
// Aot profile: behaviors are diagnosed (TWM005) and left out of the manifest, and handlers without
// an accessible parameterless constructor are TWM006 at the handler, not CS7036 in generated code (013-009).
#endregion

namespace TimeWarp.Mediator.Generators.Compilation.Tests;

public sealed class AotProfileTests
{
    [Theory]
    [InlineData("Aot")]
    [InlineData("Link")]
    public void Behavior_ReportsTwm005AtTheAttribute(string profile)
    {
        HostCompilation host = HostCompiler.Compile("AotBehavior" + profile, BehaviorSource, profile);

        Errors(host).ShouldBeEmpty();
        Diagnostic warning = host.Diagnostics.Single(d => d.Id == "TWM005");
        warning.Severity.ShouldBe(DiagnosticSeverity.Warning);
        warning.GetMessage().ShouldContain("Pipes.LoggingBehavior<TRequest, TResponse>");
        warning.GetMessage().ShouldContain(profile);
        warning.Location.SourceTree.ShouldNotBeNull();
        warning.Location.SourceTree!.GetText().ToString(warning.Location.SourceSpan)
            .ShouldBe("MediatorBehavior(typeof(Pipes.LoggingBehavior<,>))");
    }

    [Fact]
    public void Behavior_IsOmittedFromTheAotManifest()
    {
        HostCompilation host = HostCompiler.Compile("AotManifest", BehaviorSource, "Aot");

        string json = ManifestJson(host);
        json.ShouldContain("\"behaviors\":[]");
        json.ShouldNotContain("LoggingBehavior");
    }

    [Fact]
    public void Behavior_IsListedInTheHostManifest_WithNoTwm005()
    {
        HostCompilation host = HostCompiler.Compile("HostManifest", BehaviorSource, "Host");

        Errors(host).ShouldBeEmpty();
        host.Diagnostics.ShouldNotContain(d => d.Id == "TWM005");
        ManifestJson(host).ShouldContain("Pipes.LoggingBehavior<Pipes.Ping, string>");
    }

    [Fact]
    public async Task Behavior_DoesNotRunUnderAot()
    {
        HostCompilation host = HostCompiler.Compile("AotBehaviorRuntime", BehaviorSource, "Aot");
        Assembly assembly = Load(host, "AotBehaviorRuntime");
        IMediator mediator = (IMediator)Activator.CreateInstance(assembly.GetType("TimeWarp.Mediator.Generated.Mediator", true)!)!;

        object? response = await mediator.Send(Activator.CreateInstance(assembly.GetType("Pipes.Ping", true)!)!);

        response.ShouldBe("pong");
    }

    [Theory]
    [InlineData("Aot")]
    [InlineData("Link")]
    public void HandlerWithConstructorParameters_ReportsTwm006AtTheHandler(string profile)
    {
        HostCompilation host = HostCompiler.Compile("AotCtor" + profile, ConstructorSource, profile);

        Diagnostic[] errors = Errors(host);
        errors.Select(d => d.Id).OrderBy(id => id, StringComparer.Ordinal)
            .ShouldBe(new[] { "TWM006", "TWM006", "TWM006" });
        string[] flagged = errors
            .Select(d => d.Location.SourceTree!.GetText().ToString(d.Location.SourceSpan))
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToArray();
        flagged.ShouldBe(new[] { "DependentHandler", "DependentNoteHandler", "PrivateCtorHandler" });
        errors.ShouldAllBe(d => d.GetMessage().Contains(profile));
    }

    [Fact]
    public void HandlerWithConstructorParameters_IsFineUnderHost()
    {
        HostCompilation host = HostCompiler.Compile("HostCtor", ConstructorSource, "Host");

        Errors(host).ShouldBeEmpty();
        host.Diagnostics.ShouldNotContain(d => d.Id == "TWM006");
    }

    private static Diagnostic[] Errors(HostCompilation host)
    {
        return host.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
    }

    private static Assembly Load(HostCompilation host, string assemblyName)
    {
        using MemoryStream stream = new();
        EmitResult result = host.Compilation.Emit(stream);
        result.Success.ShouldBeTrue(string.Join(Environment.NewLine, result.Diagnostics));
        stream.Position = 0;
        return new AssemblyLoadContext(assemblyName, isCollectible: true).LoadFromStream(stream);
    }

    private static string ManifestJson(HostCompilation host)
    {
        IFieldSymbol json = host.GetType("TimeWarp.Mediator.Generated.MediatorManifest")
            .GetMembers("Json")
            .OfType<IFieldSymbol>()
            .Single();
        return (string)json.ConstantValue!;
    }

    private const string BehaviorSource = @"
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;

[assembly: MediatorAssembly]
[assembly: MediatorBehavior(typeof(Pipes.LoggingBehavior<,>))]

namespace Pipes
{
    public sealed class Ping : IRequest<string>
    {
    }

    public sealed class PingHandler : IRequestHandler<Ping, string>
    {
        public Task<string> Handle(Ping request, CancellationToken cancellationToken) => Task.FromResult(""pong"");
    }

    public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            TResponse response = await next(cancellationToken);
            return response;
        }
    }
}
";

    private const string ConstructorSource = @"
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;

[assembly: MediatorAssembly]

namespace Ctors
{
    public sealed class Clock
    {
    }

    public sealed class Dependent : IRequest<string>
    {
    }

    public sealed class DependentHandler : IRequestHandler<Dependent, string>
    {
        private readonly Clock Clock;

        public DependentHandler(Clock clock)
        {
            Clock = clock;
        }

        public Task<string> Handle(Dependent request, CancellationToken cancellationToken) => Task.FromResult(Clock.ToString()!);
    }

    public sealed class Hidden : IRequest<string>
    {
    }

    public sealed class PrivateCtorHandler : IRequestHandler<Hidden, string>
    {
        private PrivateCtorHandler()
        {
        }

        public Task<string> Handle(Hidden request, CancellationToken cancellationToken) => Task.FromResult(""hidden"");
    }

    public sealed class Optional : IRequest<string>
    {
    }

    public sealed class OptionalCtorHandler : IRequestHandler<Optional, string>
    {
        public OptionalCtorHandler(int retries = 3)
        {
        }

        public Task<string> Handle(Optional request, CancellationToken cancellationToken) => Task.FromResult(""optional"");
    }

    public sealed class Note : INotification
    {
    }

    public sealed class DependentNoteHandler : INotificationHandler<Note>
    {
        public DependentNoteHandler(Clock clock)
        {
        }

        public Task Handle(Note notification, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
";
}
