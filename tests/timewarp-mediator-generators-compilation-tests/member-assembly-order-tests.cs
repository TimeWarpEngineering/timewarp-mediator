#region Purpose
// Generated output and TWM002 do not depend on the order member assemblies are referenced in.
#endregion

namespace TimeWarp.Mediator.Generators.Compilation.Tests;

public sealed class MemberAssemblyOrderTests
{
    [Fact]
    public void GeneratedSource_IsIdenticalForEitherReferenceOrder()
    {
        (MetadataReference zeta, MetadataReference alpha) = CompileMembers();

        string forward = GeneratedSource(HostCompiler.Compile("Host", HostSource, "Host", zeta, alpha));
        string reverse = GeneratedSource(HostCompiler.Compile("Host", HostSource, "Host", alpha, zeta));

        forward.ShouldBe(reverse);
    }

    [Fact]
    public void NotificationHandlers_AreOrderedByDisplayName()
    {
        (MetadataReference zeta, MetadataReference alpha) = CompileMembers();

        foreach (MetadataReference[] references in new[] { new[] { zeta, alpha }, new[] { alpha, zeta } })
        {
            string source = GeneratedSource(HostCompiler.Compile("Host", HostSource, "Host", references));
            source.ShouldContain("global::Alpha.NoteHandler handler0 =");
            source.ShouldContain("global::Host.NoteHandler handler1 =");
            source.ShouldContain("global::Zeta.NoteHandler handler2 =");
        }
    }

    [Fact]
    public void DuplicateHandlerDiagnostic_ListsHandlersByDisplayName()
    {
        (MetadataReference zeta, MetadataReference alpha) = CompileMembers();

        foreach (MetadataReference[] references in new[] { new[] { zeta, alpha }, new[] { alpha, zeta } })
        {
            HostCompilation host = HostCompiler.Compile("Host", HostSource, "Host", references);
            Diagnostic[] duplicates = host.Diagnostics.Where(d => d.Id == "TWM002").ToArray();

            duplicates.Length.ShouldBe(1, string.Join(Environment.NewLine, host.Diagnostics));
            duplicates[0].GetMessage().ShouldBe("Request 'Zeta.Ping' has multiple handlers: Alpha.PingHandler, Zeta.PingHandler");
        }
    }

    private static (MetadataReference Zeta, MetadataReference Alpha) CompileMembers()
    {
        MetadataReference zeta = HostCompiler.Compile("Zeta", ZetaSource).ToMetadataReference();
        MetadataReference alpha = HostCompiler.Compile("Alpha", AlphaSource, "Host", zeta).ToMetadataReference();
        return (zeta, alpha);
    }

    private static string GeneratedSource(HostCompilation host)
    {
        return string.Join(
            Environment.NewLine,
            host.Compilation.SyntaxTrees
                .Where(t => t.FilePath.Length > 0)
                .OrderBy(t => t.FilePath, StringComparer.Ordinal)
                .Select(t => t.FilePath + Environment.NewLine + t.GetText()));
    }

    private const string ZetaSource = @"
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;

[assembly: MediatorAssembly]

namespace Zeta
{
    public sealed class Note : INotification
    {
    }

    public sealed class NoteHandler : INotificationHandler<Note>
    {
        public Task Handle(Note notification, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    public sealed class Ping : IRequest<string>
    {
    }

    public sealed class PingHandler : IRequestHandler<Ping, string>
    {
        public Task<string> Handle(Ping request, CancellationToken cancellationToken) => Task.FromResult(""zeta"");
    }
}
";

    private const string AlphaSource = @"
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;

[assembly: MediatorAssembly]

namespace Alpha
{
    public sealed class NoteHandler : INotificationHandler<Zeta.Note>
    {
        public Task Handle(Zeta.Note notification, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    public sealed class PingHandler : IRequestHandler<Zeta.Ping, string>
    {
        public Task<string> Handle(Zeta.Ping request, CancellationToken cancellationToken) => Task.FromResult(""alpha"");
    }
}
";

    private const string HostSource = @"
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;

namespace Host
{
    public sealed class NoteHandler : INotificationHandler<Zeta.Note>
    {
        public Task Handle(Zeta.Note notification, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
";
}
