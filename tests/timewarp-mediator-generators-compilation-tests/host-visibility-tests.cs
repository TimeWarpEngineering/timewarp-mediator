#region Purpose
// Generated mediator types stay internal so a compilation referencing two hosts sees no duplicates (CS0436).
#endregion

namespace TimeWarp.Mediator.Generators.Compilation.Tests;

public sealed class HostVisibilityTests
{
    private const string GeneratedNamespace = "TimeWarp.Mediator.Generated";
    private const string RegistrationType = "Microsoft.Extensions.DependencyInjection.GeneratedMediatorServiceCollectionExtensions";

    [Theory]
    [InlineData("Host")]
    [InlineData("Aot")]
    [InlineData("Link")]
    public void GeneratedTypes_AreInternal(string profile)
    {
        HostCompilation host = HostCompiler.Compile("HostA", HostSource("HostA"), profile);

        ShouldBeClean(host);
        host.GetType(GeneratedNamespace + ".Mediator").DeclaredAccessibility.ShouldBe(Accessibility.Internal);
        host.GetType(GeneratedNamespace + ".MediatorManifest").DeclaredAccessibility.ShouldBe(Accessibility.Internal);
        host.GetType(GeneratedNamespace + ".Sender_AppPipeline").DeclaredAccessibility.ShouldBe(Accessibility.Internal);
        host.GetType(GeneratedNamespace + ".Publisher_AppPipeline").DeclaredAccessibility.ShouldBe(Accessibility.Internal);
        if (profile == "Host")
        {
            host.GetType(RegistrationType).DeclaredAccessibility.ShouldBe(Accessibility.Internal);
        }
        else
        {
            host.Compilation.Assembly.GetTypeByMetadataName(RegistrationType).ShouldBeNull();
        }
    }

    [Theory]
    [InlineData("Host")]
    [InlineData("Aot")]
    [InlineData("Link")]
    public void ThirdHost_ReferencingTwoHosts_CompilesWithoutCS0436(string profile)
    {
        HostCompilation hostA = HostCompiler.Compile("HostA", HostSource("HostA"), profile);
        HostCompilation hostB = HostCompiler.Compile("HostB", HostSource("HostB"), profile);
        ShouldBeClean(hostA);
        ShouldBeClean(hostB);

        string source = HostSource("HostC") + (profile == "Host" ? HostCRegistrationSource : HostCAotSource);
        HostCompilation hostC = HostCompiler.Compile(
            "HostC",
            source,
            profile,
            hostA.ToMetadataReference(),
            hostB.ToMetadataReference());

        hostC.Diagnostics.ShouldNotContain(d => d.Id == "CS0436" || d.Id == "CS0433" || d.Id == "CS0121");
        ShouldBeClean(hostC);
        hostC.ToMetadataReference();
    }

    [Fact]
    public void AddGeneratedMediator_IsNotVisibleOutsideTheHost()
    {
        HostCompilation hostA = HostCompiler.Compile("HostA", HostSource("HostA"));
        CSharpCompilation consumer = CSharpCompilation.Create(
            "Consumer",
            syntaxTrees: new[] { CSharpSyntaxTree.ParseText(ConsumerSource, HostCompiler.ParseOptions) },
            references: HostCompiler.GetBaseReferences().Append(hostA.ToMetadataReference()),
            options: HostCompiler.CompilationOptions);

        Diagnostic[] errors = consumer.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        errors.Select(d => d.Id).ShouldBe(new[] { "CS1061" }, string.Join(Environment.NewLine, errors.Select(d => d.ToString())));
    }

    private static void ShouldBeClean(HostCompilation host)
    {
        Diagnostic[] problems = host.Diagnostics
            .Where(d => d.Severity >= DiagnosticSeverity.Warning)
            .ToArray();
        problems.ShouldBeEmpty(string.Join(Environment.NewLine, problems.Select(d => d.ToString())));
    }

    private static string HostSource(string hostNamespace)
    {
        return @"
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;

namespace " + hostNamespace + @"
{
    public sealed class AppPipeline
    {
    }

    public sealed class Ping : IRequest<string>
    {
    }

    public sealed class PingHandler : IRequestHandler<Ping, string>
    {
        public Task<string> Handle(Ping request, CancellationToken cancellationToken) => Task.FromResult(""" + hostNamespace + @""");
    }

    [MediatorScope(typeof(AppPipeline))]
    public sealed class ScopedPing : IRequest<int>
    {
    }

    [MediatorScope(typeof(AppPipeline))]
    public sealed class ScopedPingHandler : IRequestHandler<ScopedPing, int>
    {
        public Task<int> Handle(ScopedPing request, CancellationToken cancellationToken) => Task.FromResult(1);
    }

    [MediatorScope(typeof(AppPipeline))]
    public sealed class ScopedNote : INotification
    {
    }

    [MediatorScope(typeof(AppPipeline))]
    public sealed class ScopedNoteHandler : INotificationHandler<ScopedNote>
    {
        public Task Handle(ScopedNote notification, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
";
    }

    private const string ConsumerSource = @"
using Microsoft.Extensions.DependencyInjection;

namespace Consumer
{
    public static class Startup
    {
        public static IServiceCollection Configure(IServiceCollection services) => services.AddGeneratedMediator();
    }
}
";

    private const string HostCRegistrationSource = @"
namespace HostC.Composition
{
    using Microsoft.Extensions.DependencyInjection;

    public static class Startup
    {
        public static Microsoft.Extensions.DependencyInjection.IServiceCollection Configure(Microsoft.Extensions.DependencyInjection.IServiceCollection services)
        {
            services.AddGeneratedMediator();
            return services.AddGeneratedMediator<global::HostC.AppPipeline>();
        }

        internal static string Manifest => global::TimeWarp.Mediator.Generated.MediatorManifest.Json;

        internal static System.Type MediatorType => typeof(global::TimeWarp.Mediator.Generated.Mediator);
    }
}
";

    private const string HostCAotSource = @"
namespace HostC.Composition
{
    public static class Startup
    {
        public static global::TimeWarp.Mediator.IMediator Create() => new global::TimeWarp.Mediator.Generated.Mediator();

        internal static string Manifest => global::TimeWarp.Mediator.Generated.MediatorManifest.Json;
    }
}
";
}
