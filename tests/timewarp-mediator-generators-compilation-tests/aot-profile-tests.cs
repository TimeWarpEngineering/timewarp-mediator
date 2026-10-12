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

    ManifestDocument manifest = host.Manifest();
    manifest.Profile.ShouldBe("Aot");
    manifest.Request("Pipes.Ping").Behaviors.ShouldBeEmpty();
  }

  [Fact]
  public void Behavior_IsListedInTheHostManifest_WithNoTwm005()
  {
    HostCompilation host = HostCompiler.Compile("HostManifest", BehaviorSource, "Host");

    Errors(host).ShouldBeEmpty();
    host.Diagnostics.ShouldNotContain(d => d.Id == "TWM005");
    host.Manifest().Request("Pipes.Ping").Behaviors.ShouldBe(new[] { "Pipes.LoggingBehavior<Pipes.Ping, string>" });
  }

  [Fact]
  public async Task Behavior_DoesNotRunUnderAot()
  {
    HostCompilation host = HostCompiler.Compile("AotBehaviorRuntime", BehaviorSource, "Aot");
    Assembly assembly = Load(host, "AotBehaviorRuntime");
    IMediator mediator = (IMediator) Activator.CreateInstance(assembly.GetType("TimeWarp.Mediator.Generated.Mediator", true)!)!;

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

  [Theory]
  [InlineData("Aot")]
  [InlineData("Link")]
  public void HandlerWithRequiredMember_ReportsTwm006AndNoCs9035(string profile)
  {
    HostCompilation host = HostCompiler.Compile("AotRequired" + profile, RequiredMemberSource, profile);

    Diagnostic[] errors = Errors(host);
    errors.Select(d => d.Id).OrderBy(id => id, StringComparer.Ordinal)
        .ShouldBe(new[] { "TWM006", "TWM006" });
    string[] flagged = errors
        .Select(d => d.Location.SourceTree!.GetText().ToString(d.Location.SourceSpan))
        .OrderBy(n => n, StringComparer.Ordinal)
        .ToArray();
    flagged.ShouldBe(new[] { "DerivedRequiredHandler", "RequiredMemberHandler" });
    errors.ShouldAllBe(d => d.GetMessage().Contains(profile));
  }

  [Fact]
  public void HandlerWithRequiredMember_IsFineUnderHost()
  {
    HostCompilation host = HostCompiler.Compile("HostRequired", RequiredMemberSource, "Host");

    Errors(host).ShouldBeEmpty();
    host.Diagnostics.ShouldNotContain(d => d.Id == "TWM006");
  }

  [Theory]
  [InlineData("Aot")]
  [InlineData("Link")]
  public void HandlerWithSetsRequiredMembers_IsConstructible(string profile)
  {
    HostCompilation host = HostCompiler.Compile("AotSetsRequired" + profile, SetsRequiredMembersSource, profile);

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
        public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
            => throw new System.InvalidOperationException(""behavior ran"");
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

  private const string RequiredMemberSource = @"
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;

[assembly: MediatorAssembly]

namespace Required
{
    public sealed class Ping : IRequest<string>
    {
    }

    public sealed class RequiredMemberHandler : IRequestHandler<Ping, string>
    {
        public required string Name { get; init; }

        public Task<string> Handle(Ping request, CancellationToken cancellationToken) => Task.FromResult(Name);
    }

    public class RequiredBase
    {
        public required string Name { get; init; }
    }

    public sealed class DerivedPing : IRequest<string>
    {
    }

    public sealed class DerivedRequiredHandler : RequiredBase, IRequestHandler<DerivedPing, string>
    {
        public Task<string> Handle(DerivedPing request, CancellationToken cancellationToken) => Task.FromResult(Name);
    }
}
";

  private const string SetsRequiredMembersSource = @"
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;

[assembly: MediatorAssembly]

namespace Sets
{
    public sealed class Ping : IRequest<string>
    {
    }

    public sealed class SetsRequiredHandler : IRequestHandler<Ping, string>
    {
        public required string Name { get; init; }

        [SetsRequiredMembers]
        public SetsRequiredHandler()
        {
            Name = ""set"";
        }

        public Task<string> Handle(Ping request, CancellationToken cancellationToken) => Task.FromResult(Name);
    }
}
";
}