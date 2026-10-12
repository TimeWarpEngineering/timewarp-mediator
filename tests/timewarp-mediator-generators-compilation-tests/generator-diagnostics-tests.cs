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
        ManifestJson(host).ShouldNotContain("HiddenHandler");
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
        ManifestJson(host).ShouldContain("Members.HiddenHandler");
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
    public void StreamHandlerAndPreProcessor_AreTwm009Warnings()
    {
        HostCompilation host = HostCompiler.Compile(
            "Unsupported",
            UnsupportedSource,
            "Host",
            MetadataReference.CreateFromFile(typeof(TimeWarp.Mediator.Pipeline.IRequestPreProcessor<>).Assembly.Location));

        Errors(host).ShouldBeEmpty();
        Diagnostic[] warnings = host.Diagnostics.Where(d => d.Id == "TWM009").OrderBy(d => d.GetMessage(), StringComparer.Ordinal).ToArray();
        warnings.Length.ShouldBe(2);
        warnings.ShouldAllBe(d => d.Severity == DiagnosticSeverity.Warning);
        warnings[0].GetMessage().ShouldContain("Shapes.AuditPreProcessor");
        warnings[0].GetMessage().ShouldContain("IRequestPreProcessor<TRequest>");
        warnings[1].GetMessage().ShouldContain("Shapes.CountHandler");
        warnings[1].GetMessage().ShouldContain("IStreamRequestHandler<TRequest, TResponse>");
        Span(warnings[1]).ShouldBe("CountHandler");
    }

    [Fact]
    public void CreateStream_MessageHasNoInternalTaskIds()
    {
        HostCompilation host = HostCompiler.Compile("StreamMessage", HostSource, "Aot");
        Assembly assembly = Load(host, "StreamMessage");
        IMediator mediator = (IMediator)Activator.CreateInstance(assembly.GetType("TimeWarp.Mediator.Generated.Mediator", true)!)!;

        NotSupportedException exception = Should.Throw<NotSupportedException>(() => mediator.CreateStream(new object()));

        exception.Message.ShouldNotContain("004-");
        exception.Message.ShouldContain("stream");
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

    private static string ManifestJson(HostCompilation host)
    {
        IFieldSymbol json = host.GetType("TimeWarp.Mediator.Generated.MediatorManifest")
            .GetMembers("Json")
            .OfType<IFieldSymbol>()
            .Single();
        return (string)json.ConstantValue!;
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
        public Task<string> Handle(Ping request, CancellationToken cancellationToken) => Task.FromResult(""pong"");
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
        public Task<int> Handle(Hidden request, CancellationToken cancellationToken) => Task.FromResult(1);
    }
}
";

    private const string UnsupportedSource = @"
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;
using TimeWarp.Mediator.Pipeline;

namespace Shapes
{
    public sealed class Ping : IRequest<string>
    {
    }

    public sealed class PingHandler : IRequestHandler<Ping, string>
    {
        public Task<string> Handle(Ping request, CancellationToken cancellationToken) => Task.FromResult(""pong"");
    }

    public sealed class Count : IStreamRequest<int>
    {
    }

    public sealed class CountHandler : IStreamRequestHandler<Count, int>
    {
        public async IAsyncEnumerable<int> Handle(Count request, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await Task.Yield();
            yield return 1;
        }
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
        public Task<string> Handle(Ping request, CancellationToken cancellationToken) => Task.FromResult(""pong"");
    }

    public sealed class NotABehavior
    {
    }

    public sealed class OpenBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken) => next(cancellationToken);
    }

    public sealed class AuditedBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IAudited
    {
        public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken) => next(cancellationToken);
    }
}
";

    private const string NotABehavior = "[assembly: MediatorBehavior(typeof(Pipes.NotABehavior))]";
    private const string ConstrainedBehavior = "[assembly: MediatorBehavior(typeof(Pipes.AuditedBehavior<,>))]";
    private const string EmptyPipelineBehavior = "[assembly: MediatorBehavior(typeof(Pipes.OpenBehavior<,>), Scope = typeof(Pipes.EmptyPipeline))]";
    private const string OpenBehavior = "[assembly: MediatorBehavior(typeof(Pipes.OpenBehavior<,>))]";
}
