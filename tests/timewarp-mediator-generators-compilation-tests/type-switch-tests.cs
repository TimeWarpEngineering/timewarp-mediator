#region Purpose
// Send(object) / Publish(object) emission: most-derived case order, contravariant notification
// delivery, fixed local names, and collision-safe sanitized member names (013-008).
#endregion

namespace TimeWarp.Mediator.Generators.Compilation.Tests;

public sealed class TypeSwitchTests
{
    [Theory]
    [InlineData("Host")]
    [InlineData("Aot")]
    public void BaseRequestSortingBeforeDerived_Compiles(string profile)
    {
        HostCompilation host = HostCompiler.Compile("BaseBeforeDerived" + profile, HierarchySource, profile);

        Errors(host).ShouldBeEmpty();
    }

    [Fact]
    public async Task DerivedRequest_DispatchesToItsOwnHandler()
    {
        IMediator mediator = LoadAotMediator("RequestHierarchy", HierarchySource, out Assembly assembly);

        object? derived = await mediator.Send(Activator.CreateInstance(assembly.GetType("Shapes.ZDerivedRequest", true)!)!);
        object? baseRequest = await mediator.Send(Activator.CreateInstance(assembly.GetType("Shapes.ABaseRequest", true)!)!);

        derived.ShouldBe("derived");
        baseRequest.ShouldBe("base");
    }

    [Fact]
    public async Task DerivedNotification_ReachesBaseAndInterfaceHandlers()
    {
        IMediator mediator = LoadAotMediator("NotificationHierarchy", HierarchySource, out Assembly assembly);

        object derived = Activator.CreateInstance(assembly.GetType("Shapes.ZDerivedNote", true)!)!;
        await mediator.Publish(derived);
        Seen(derived).ShouldBe(new[] { "derived", "base", "audit" });

        object baseNote = Activator.CreateInstance(assembly.GetType("Shapes.ABaseNote", true)!)!;
        await mediator.Publish(baseNote);
        Seen(baseNote).ShouldBe(new[] { "base" });
    }

    [Fact]
    public async Task UnboundDerivedNotification_ReachesEveryBoundSupertype()
    {
        IMediator mediator = LoadAotMediator("UnboundDerived", HierarchySource, out Assembly assembly);

        object note = Activator.CreateInstance(assembly.GetType("Shapes.UnboundNote", true)!)!;
        await mediator.Publish(note);

        Seen(note).ShouldBe(new[] { "base", "audit" });
    }

    [Theory]
    [InlineData("Host")]
    [InlineData("Aot")]
    public void KeywordNamedMessages_Compile(string profile)
    {
        HostCompilation host = HostCompiler.Compile("KeywordNames" + profile, KeywordSource, profile);

        Errors(host).ShouldBeEmpty();
    }

    [Theory]
    [InlineData("Host")]
    [InlineData("Aot")]
    public void CollidingSanitizedNames_Compile(string profile)
    {
        HostCompilation host = HostCompiler.Compile("Colliding" + profile, CollidingSource, profile);

        Errors(host).ShouldBeEmpty();
        INamedTypeSymbol mediator = host.GetType("TimeWarp.Mediator.Generated.Mediator");
        string[] dispatch = mediator.GetMembers()
            .Select(m => m.Name)
            .Where(n => n.StartsWith("Dispatch_global__A_B_C", StringComparison.Ordinal))
            .ToArray();
        dispatch.Length.ShouldBe(2);
        dispatch.Distinct().Count().ShouldBe(2);
    }

    [Fact]
    public async Task CollidingSanitizedNames_DispatchToTheirOwnHandlers()
    {
        IMediator mediator = LoadAotMediator("CollidingRuntime", CollidingSource, out Assembly assembly);

        object? first = await mediator.Send(Activator.CreateInstance(assembly.GetType("A.B_C", true)!)!);
        object? second = await mediator.Send(Activator.CreateInstance(assembly.GetType("A_B.C", true)!)!);

        first.ShouldBe("A.B_C");
        second.ShouldBe("A_B.C");
    }

    private static Diagnostic[] Errors(HostCompilation host)
    {
        return host.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
    }

    private static string[] Seen(object notification)
    {
        object list = notification.GetType().GetField("Seen")!.GetValue(notification)!;
        return ((List<string>)list).ToArray();
    }

    private static IMediator LoadAotMediator(string assemblyName, string source, out Assembly assembly)
    {
        HostCompilation host = HostCompiler.Compile(assemblyName, source, "Aot");
        Errors(host).ShouldBeEmpty();

        using MemoryStream stream = new();
        EmitResult result = host.Compilation.Emit(stream);
        result.Success.ShouldBeTrue(string.Join(Environment.NewLine, result.Diagnostics));
        stream.Position = 0;

        AssemblyLoadContext context = new(assemblyName, isCollectible: true);
        assembly = context.LoadFromStream(stream);
        Type mediatorType = assembly.GetType("TimeWarp.Mediator.Generated.Mediator", true)!;
        return (IMediator)Activator.CreateInstance(mediatorType)!;
    }

    // ABase* sorts before ZDerived*: alphabetical emission produced CS8120 and lost base delivery.
    private const string HierarchySource = @"
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;

[assembly: MediatorAssembly]

namespace Shapes
{
    public class ABaseRequest : IRequest<string>
    {
    }

    public sealed class ZDerivedRequest : ABaseRequest
    {
    }

    public sealed class ABaseRequestHandler : IRequestHandler<ABaseRequest, string>
    {
        public Task<string> Handle(ABaseRequest request, CancellationToken cancellationToken) => Task.FromResult(""base"");
    }

    public sealed class ZDerivedRequestHandler : IRequestHandler<ZDerivedRequest, string>
    {
        public Task<string> Handle(ZDerivedRequest request, CancellationToken cancellationToken) => Task.FromResult(""derived"");
    }

    public interface IAuditNote : INotification
    {
        List<string> Log { get; }
    }

    public class ABaseNote : INotification
    {
        public List<string> Seen = new List<string>();
    }

    public sealed class ZDerivedNote : ABaseNote, IAuditNote
    {
        public List<string> Log => Seen;
    }

    public sealed class UnboundNote : ABaseNote, IAuditNote
    {
        public List<string> Log => Seen;
    }

    public sealed class BaseNoteHandler : INotificationHandler<ABaseNote>
    {
        public Task Handle(ABaseNote notification, CancellationToken cancellationToken)
        {
            notification.Seen.Add(""base"");
            return Task.CompletedTask;
        }
    }

    public sealed class DerivedNoteHandler : INotificationHandler<ZDerivedNote>
    {
        public Task Handle(ZDerivedNote notification, CancellationToken cancellationToken)
        {
            notification.Seen.Add(""derived"");
            return Task.CompletedTask;
        }
    }

    public sealed class AuditNoteHandler : INotificationHandler<IAuditNote>
    {
        public Task Handle(IAuditNote notification, CancellationToken cancellationToken)
        {
            notification.Log.Add(""audit"");
            return Task.CompletedTask;
        }
    }
}
";

    // Lower-casing these names produced `request`, `notification`, `object`, `lock`, `params`, `base`.
    private const string KeywordSource = @"
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;

[assembly: MediatorAssembly]

namespace Keywords
{
    public sealed class Request : IRequest<string> { }
    public sealed class Object : IRequest<string> { }
    public sealed class Lock : IRequest<string> { }
    public sealed class Params : IRequest<string> { }
    public sealed class Base : IRequest<string> { }
    public sealed class Notification : INotification { }
    public sealed class Event : INotification { }

    public sealed class RequestHandler : IRequestHandler<Request, string>
    {
        public Task<string> Handle(Request request, CancellationToken cancellationToken) => Task.FromResult(""r"");
    }

    public sealed class ObjectHandler : IRequestHandler<Object, string>
    {
        public Task<string> Handle(Object request, CancellationToken cancellationToken) => Task.FromResult(""o"");
    }

    public sealed class LockHandler : IRequestHandler<Lock, string>
    {
        public Task<string> Handle(Lock request, CancellationToken cancellationToken) => Task.FromResult(""l"");
    }

    public sealed class ParamsHandler : IRequestHandler<Params, string>
    {
        public Task<string> Handle(Params request, CancellationToken cancellationToken) => Task.FromResult(""p"");
    }

    public sealed class BaseHandler : IRequestHandler<Base, string>
    {
        public Task<string> Handle(Base request, CancellationToken cancellationToken) => Task.FromResult(""b"");
    }

    public sealed class NotificationHandler : INotificationHandler<Notification>
    {
        public Task Handle(Notification notification, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    public sealed class EventHandler : INotificationHandler<Event>
    {
        public Task Handle(Event notification, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
";

    // A.B_C and A_B.C both sanitize to global__A_B_C (requests, notifications, and handlers).
    private const string CollidingSource = @"
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;

[assembly: MediatorAssembly]

namespace A
{
    public sealed class B_C : IRequest<string> { }
    public sealed class B_CNote : INotification { }

    public sealed class B_CHandler : IRequestHandler<B_C, string>
    {
        public Task<string> Handle(B_C request, CancellationToken cancellationToken) => Task.FromResult(""A.B_C"");
    }

    public sealed class B_CNoteHandler : INotificationHandler<B_CNote>
    {
        public Task Handle(B_CNote notification, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}

namespace A_B
{
    public sealed class C : IRequest<string> { }
    public sealed class CNote : INotification { }

    public sealed class CHandler : IRequestHandler<C, string>
    {
        public Task<string> Handle(C request, CancellationToken cancellationToken) => Task.FromResult(""A_B.C"");
    }

    public sealed class CNoteHandler : INotificationHandler<CNote>
    {
        public Task Handle(CNote notification, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
";
}
