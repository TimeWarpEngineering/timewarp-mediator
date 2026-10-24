#region Purpose
// Unsupported or invalid shapes are TWM007–TWM011 diagnostics, not compile errors inside .g.cs (013-010).
#endregion

namespace TimeWarp.Mediator.Generators.Compilation.Tests;

public sealed class GeneratorDiagnosticsTests
{
  [Fact]
  public void InternalHandlerInMemberAssembly_IsTwm007_NotCs0122()
  {
    MetadataReference member = HostCompiler.CompileLibrary("InternalMember", InternalMemberSource);

    HostCompilation host = HostCompiler.Compile("InternalHost", HostSource, "Host", member);

    host.Diagnostics.ShouldNotContain(d => d.Id == "CS0122" || d.Id == "TWM001");
    Diagnostic error = Errors(host).ShouldHaveSingleItem();
    error.Id.ShouldBe("TWM007");
    error.GetMessage().ShouldContain("Members.HiddenHandler");
    error.GetMessage().ShouldContain("InternalMember");
    error.GetMessage().ShouldContain("InternalsVisibleTo(\"InternalHost\")");
    host.Manifest().Handlers.ShouldNotContain("Members.HiddenHandler");
  }

  [Fact]
  public void InternalHandlerWithInternalsVisibleTo_IsBound()
  {
    MetadataReference member = HostCompiler.CompileLibrary(
        "VisibleMember",
        InternalMemberSource.Replace(
            "[assembly: MediatorAssembly]",
            "[assembly: MediatorAssembly]\n[assembly: System.Runtime.CompilerServices.InternalsVisibleTo(\"VisibleHost\")]"));

    HostCompilation host = HostCompiler.Compile("VisibleHost", HostSource, "Host", member);

    Errors(host).ShouldBeEmpty();
    host.Manifest().Request("Members.Hidden").Handler.ShouldBe("Members.HiddenHandler");
  }

  [Fact]
  public void InternalScopeMarkerInMemberAssembly_IsTwm007_NotCs0122()
  {
    MetadataReference member = HostCompiler.CompileLibrary("ScopedMember", InternalScopeSource);

    HostCompilation host = HostCompiler.Compile("ScopeHost", "namespace App { public sealed class Program { } }", "Host", member);

    host.Diagnostics.ShouldNotContain(d => d.Id == "CS0122" || d.Id == "TWM001");
    Diagnostic error = Errors(host).ShouldHaveSingleItem();
    error.Id.ShouldBe("TWM007");
    error.GetMessage().ShouldContain("Members.HiddenPipeline");
    error.GetMessage().ShouldContain("InternalsVisibleTo(\"ScopeHost\")");
    ManifestDocument manifest = host.Manifest();
    manifest.Handlers.ShouldNotContain("Members.PayHandler");
    manifest.Handlers.ShouldNotContain("Members.PaidHandler");
  }

  [Fact]
  public void InternalScopeMarkerWithInternalsVisibleTo_IsBound()
  {
    MetadataReference member = HostCompiler.CompileLibrary(
        "VisibleScopeMember",
        InternalScopeSource.Replace(
            "[assembly: MediatorAssembly]",
            "[assembly: MediatorAssembly]\n[assembly: System.Runtime.CompilerServices.InternalsVisibleTo(\"VisibleScopeHost\")]"));

    HostCompilation host = HostCompiler.Compile(
        "VisibleScopeHost",
        "namespace App { public sealed class Program { } }",
        "Host",
        member);

    Errors(host).ShouldBeEmpty();
    host.Diagnostics.ShouldNotContain(d => d.Id == "CS0122");
    ManifestDocument manifest = host.Manifest();
    ManifestRequest pay = manifest.Request("Members.Pay");
    pay.Handler.ShouldBe("Members.PayHandler");
    pay.Scope.ShouldBe("Members.HiddenPipeline");
    ManifestNotification paid = manifest.Notification("Members.Paid");
    paid.Handlers.ShouldBe(new[] { "Members.PaidHandler" });
    paid.Scope.ShouldBe("Members.HiddenPipeline");
  }

  [Theory]
  [InlineData("Host")]
  [InlineData("Aot")]
  public void MissingContracts_ReportsOnlyTwm008_AndEmitsNothing(string profile)
  {
    HostCompilation host = HostCompiler.CompileWithReferences(
        "NoContracts" + profile,
        "namespace App { public sealed class Program { } }",
        profile,
        HostCompiler.GetSystemReferences());

    Diagnostic diagnostic = host.Diagnostics.Where(d => d.Severity >= DiagnosticSeverity.Warning).ShouldHaveSingleItem();
    diagnostic.Id.ShouldBe("TWM008");
    diagnostic.GetMessage().ShouldContain("TimeWarp.Mediator.IMediator");
    host.Compilation.SyntaxTrees.Count().ShouldBe(1);
  }

  [Fact]
  public void HostWithoutDependencyInjection_ReportsOnlyTwm008()
  {
    HostCompilation host = HostCompiler.CompileWithReferences(
        "NoDiHost",
        HostSource,
        "Host",
        ContractsOnly());

    Diagnostic diagnostic = host.Diagnostics.Where(d => d.Severity >= DiagnosticSeverity.Warning).ShouldHaveSingleItem();
    diagnostic.Id.ShouldBe("TWM008");
    diagnostic.GetMessage().ShouldContain("Microsoft.Extensions.DependencyInjection.IServiceCollection");
    host.Compilation.SyntaxTrees.Count().ShouldBe(1);
  }

  [Fact]
  public void AotWithoutDependencyInjection_Generates()
  {
    HostCompilation host = HostCompiler.CompileWithReferences("NoDiAot", HostSource, "Aot", ContractsOnly());

    host.Diagnostics.ShouldNotContain(d => d.Severity >= DiagnosticSeverity.Warning);
    host.GetType("TimeWarp.Mediator.Generated.Mediator");
  }

  [Fact]
  public void PreProcessor_IsTwm009Warning()
  {
    HostCompilation host = HostCompiler.Compile(
        "Unsupported",
        UnsupportedSource,
        "Host");

    Errors(host).ShouldBeEmpty();
    Diagnostic warning = host.Diagnostics.Where(d => d.Id == "TWM009").ShouldHaveSingleItem();
    warning.Severity.ShouldBe(DiagnosticSeverity.Warning);
    warning.GetMessage().ShouldContain("Shapes.AuditPreProcessor");
    warning.GetMessage().ShouldContain("IRequestPreProcessor<TRequest>");
    Span(warning).ShouldBe("AuditPreProcessor");
  }

  [Fact]
  public void BehaviorThatIsNotAPipelineBehavior_IsTwm010AtTheAttribute()
  {
    HostCompilation host = HostCompiler.Compile("BadBehavior", BehaviorSource(NotABehavior), "Host");

    Errors(host).ShouldBeEmpty();
    Diagnostic warning = host.Diagnostics.Single(d => d.Id == "TWM010");
    warning.GetMessage().ShouldContain("Pipes.NotABehavior");
    Span(warning).ShouldBe("MediatorBehavior(typeof(Pipes.NotABehavior))");
  }

  [Fact]
  public void BehaviorClosingOverNoRequest_IsTwm011()
  {
    HostCompilation host = HostCompiler.Compile("UnusedBehavior", BehaviorSource(ConstrainedBehavior), "Host");

    Errors(host).ShouldBeEmpty();
    Diagnostic warning = host.Diagnostics.Single(d => d.Id == "TWM011");
    warning.GetMessage().ShouldContain("Pipes.AuditedBehavior<TRequest, TResponse>");
    warning.GetMessage().ShouldContain("unscoped");
    Span(warning).ShouldBe("MediatorBehavior(typeof(Pipes.AuditedBehavior<,>))");
  }

  [Fact]
  public void ScopedBehaviorForAnEmptyPipeline_IsTwm011()
  {
    HostCompilation host = HostCompiler.Compile("EmptyPipeline", BehaviorSource(EmptyPipelineBehavior), "Host");

    Errors(host).ShouldBeEmpty();
    Diagnostic warning = host.Diagnostics.Single(d => d.Id == "TWM011");
    warning.GetMessage().ShouldContain("Pipes.EmptyPipeline");
  }

  [Fact]
  public void BehaviorThatApplies_HasNoBehaviorDiagnostics()
  {
    HostCompilation host = HostCompiler.Compile("GoodBehavior", BehaviorSource(OpenBehavior), "Host");

    host.Diagnostics.ShouldNotContain(d => d.Severity >= DiagnosticSeverity.Warning);
  }

  private static IEnumerable<MetadataReference> ContractsOnly()
  {
    return HostCompiler.GetSystemReferences()
        .Append(MetadataReference.CreateFromFile(typeof(IMediator).Assembly.Location));
  }

  private static Diagnostic[] Errors(HostCompilation host)
  {
    return host.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
  }

  private static string Span(Diagnostic diagnostic)
  {
    diagnostic.Location.SourceTree.ShouldNotBeNull();
    return diagnostic.Location.SourceTree!.GetText().ToString(diagnostic.Location.SourceSpan);
  }

  private static Assembly Load(HostCompilation host, string assemblyName)
  {
    using MemoryStream stream = new();
    EmitResult result = host.Compilation.Emit(stream);
    result.Success.ShouldBeTrue(string.Join(Environment.NewLine, result.Diagnostics));
    stream.Position = 0;
    return new AssemblyLoadContext(assemblyName, isCollectible: true).LoadFromStream(stream);
  }

  private const string HostSource = @"
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;

namespace App
{
    public sealed class Ping : IRequest<string>
    {
    }

    public sealed class PingHandler : IRequestHandler<Ping, string>
    {
        public ValueTask<string> Handle(Ping request, CancellationToken cancellationToken) => ValueTask.FromResult(""pong"");
    }
}
";

  private const string InternalMemberSource = @"
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;

[assembly: MediatorAssembly]

namespace Members
{
    public sealed class Hidden : IRequest<int>
    {
    }

    internal sealed class HiddenHandler : IRequestHandler<Hidden, int>
    {
        public ValueTask<int> Handle(Hidden request, CancellationToken cancellationToken) => ValueTask.FromResult(1);
    }
}
";

  private const string InternalScopeSource = @"
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;

[assembly: MediatorAssembly]
[assembly: MediatorScope(typeof(Members.HiddenPipeline))]

namespace Members
{
    internal sealed class HiddenPipeline
    {
    }

    public sealed class Pay : IRequest<int>
    {
    }

    public sealed class PayHandler : IRequestHandler<Pay, int>
    {
        public ValueTask<int> Handle(Pay request, CancellationToken cancellationToken) => ValueTask.FromResult(1);
    }

    public sealed class Paid : INotification
    {
    }

    public sealed class PaidHandler : INotificationHandler<Paid>
    {
        public ValueTask Handle(Paid notification, CancellationToken cancellationToken) => default;
    }
}
";

  private const string UnsupportedSource = @"
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;
using TimeWarp.Mediator.Pipeline;

namespace TimeWarp.Mediator.Pipeline
{
    public interface IRequestPreProcessor<TRequest>
    {
        Task Process(TRequest request, CancellationToken cancellationToken);
    }
}

namespace Shapes
{
    public sealed class Ping : IRequest<string>
    {
    }

    public sealed class PingHandler : IRequestHandler<Ping, string>
    {
        public ValueTask<string> Handle(Ping request, CancellationToken cancellationToken) => ValueTask.FromResult(""pong"");
    }

    public sealed class AuditPreProcessor : IRequestPreProcessor<Ping>
    {
        public Task Process(Ping request, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
";

  private static string BehaviorSource(string registration) => @"
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;

" + registration + @"

namespace Pipes
{
    public interface IAudited
    {
    }

    public sealed class EmptyPipeline
    {
    }

    public sealed class Ping : IRequest<string>
    {
    }

    public sealed class PingHandler : IRequestHandler<Ping, string>
    {
        public ValueTask<string> Handle(Ping request, CancellationToken cancellationToken) => ValueTask.FromResult(""pong"");
    }

    public sealed class NotABehavior
    {
    }

    public sealed class OpenBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        public ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken) => next(request, cancellationToken);
    }

    public sealed class AuditedBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IAudited
    {
        public ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken) => next(request, cancellationToken);
    }
}
";

  private const string NotABehavior = "[assembly: MediatorBehavior(typeof(Pipes.NotABehavior))]";
  private const string ConstrainedBehavior = "[assembly: MediatorBehavior(typeof(Pipes.AuditedBehavior<,>))]";
  private const string EmptyPipelineBehavior = "[assembly: MediatorBehavior(typeof(Pipes.OpenBehavior<,>), Scope = typeof(Pipes.EmptyPipeline))]";
  private const string OpenBehavior = "[assembly: MediatorBehavior(typeof(Pipes.OpenBehavior<,>))]";
}