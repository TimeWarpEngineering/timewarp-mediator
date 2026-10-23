#region Purpose
// Table-driven host shapes: each compiles under the generator with no warnings or errors and binds
// the handlers the manifest lists, in every profile (013-012, G-23).
#endregion

#region Design
// One row per source shape; the row names the request/notification bindings it must produce, so a
// shape that compiles but silently drops a handler still fails. Profiles Host and Aot both run
// because they emit different dispatch (service provider vs. ServiceGen fields).
#endregion

namespace TimeWarp.Mediator.Generators.Compilation.Tests;

public sealed class ShapeMatrixTests
{
  public static IEnumerable<object[]> Shapes()
  {
    foreach (string profile in new[] { "Host", "Aot" })
    {
      foreach (string shape in ShapeSources.Keys)
      {
        yield return new object[] { shape, profile };
      }
    }
  }

  [Theory]
  [MemberData(nameof(Shapes))]
  public void Shape_CompilesCleanAndBindsEveryHandler(string shape, string profile)
  {
    ShapeSource row = ShapeSources[shape];

    HostCompilation host = HostCompiler.Compile("Shape" + shape + profile, row.Source, profile);

    Diagnostic[] problems = host.Diagnostics.Where(d => d.Severity >= DiagnosticSeverity.Warning).ToArray();
    problems.ShouldBeEmpty(string.Join(Environment.NewLine, problems.Select(d => d.ToString())));
    ManifestDocument manifest = host.Manifest();
    manifest.Profile.ShouldBe(profile);
    manifest.Requests.Select(r => r.Request + " => " + r.Handler).OrderBy(b => b, StringComparer.Ordinal)
        .ShouldBe(row.Requests.OrderBy(b => b, StringComparer.Ordinal));
    manifest.Notifications.Select(n => n.Notification + " => " + string.Join(", ", n.Handlers))
        .OrderBy(b => b, StringComparer.Ordinal)
        .ShouldBe(row.Notifications.OrderBy(b => b, StringComparer.Ordinal));
  }

  private static readonly Dictionary<string, ShapeSource> ShapeSources = new()
  {
    ["Nested"] = new ShapeSource(
          @"
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;

namespace Shapes
{
    public static class Outer
    {
        public sealed class Ping : IRequest<string>
        {
        }

        public static class Inner
        {
            public sealed class PingHandler : IRequestHandler<Outer.Ping, string>
            {
                public ValueTask<string> Handle(Outer.Ping request, CancellationToken cancellationToken) => ValueTask.FromResult(""pong"");
            }
        }
    }
}
",
          new[] { "Shapes.Outer.Ping => Shapes.Outer.Inner.PingHandler" }),

    ["Internal"] = new ShapeSource(
          @"
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;

namespace Shapes
{
    internal sealed class Ping : IRequest<string>
    {
    }

    internal sealed class PingHandler : IRequestHandler<Ping, string>
    {
        public ValueTask<string> Handle(Ping request, CancellationToken cancellationToken) => ValueTask.FromResult(""pong"");
    }

    internal sealed class Note : INotification
    {
    }

    internal sealed class NoteHandler : INotificationHandler<Note>
    {
        public ValueTask Handle(Note notification, CancellationToken cancellationToken) => default;
    }
}
",
          new[] { "Shapes.Ping => Shapes.PingHandler" },
          new[] { "Shapes.Note => Shapes.NoteHandler" }),

    ["KeywordNamed"] = new ShapeSource(
          @"
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;

namespace @event.@class
{
    public sealed class @object : IRequest<string>
    {
    }

    public sealed class @string : IRequestHandler<@object, string>
    {
        public ValueTask<string> Handle(@object request, CancellationToken cancellationToken) => ValueTask.FromResult(""keyword"");
    }

    public sealed class @int : INotification
    {
    }

    public sealed class @namespace : INotificationHandler<@int>
    {
        public ValueTask Handle(@int notification, CancellationToken cancellationToken) => default;
    }
}
",
          // The manifest keeps the @ escapes, so a name read back from it is valid C#.
          new[] { "@event.@class.@object => @event.@class.@string" },
          new[] { "@event.@class.@int => @event.@class.@namespace" }),

    ["ClosedGeneric"] = new ShapeSource(
          @"
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;

namespace Shapes
{
    public sealed class Box<T> : IRequest<List<T>>
    {
    }

    public sealed class IntBoxHandler : IRequestHandler<Box<int>, List<int>>
    {
        public ValueTask<List<int>> Handle(Box<int> request, CancellationToken cancellationToken) => ValueTask.FromResult(new List<int>());
    }

    public sealed class StringBoxHandler : IRequestHandler<Box<string>, List<string>>
    {
        public ValueTask<List<string>> Handle(Box<string> request, CancellationToken cancellationToken) => ValueTask.FromResult(new List<string>());
    }
}
",
          new[] { "Shapes.Box<int> => Shapes.IntBoxHandler", "Shapes.Box<string> => Shapes.StringBoxHandler" }),

    ["MultiInterfaceHandler"] = new ShapeSource(
          @"
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;

namespace Shapes
{
    public sealed class GetName : IRequest<string>
    {
    }

    public sealed class GetCount : IRequest<int>
    {
    }

    public sealed class Archive : IRequest
    {
    }

    public sealed class Archived : INotification
    {
    }

    public sealed class EverythingHandler :
        IRequestHandler<GetName, string>,
        IRequestHandler<GetCount, int>,
        IRequestHandler<Archive>,
        INotificationHandler<Archived>
    {
        public ValueTask<string> Handle(GetName request, CancellationToken cancellationToken) => ValueTask.FromResult(""name"");

        public ValueTask<int> Handle(GetCount request, CancellationToken cancellationToken) => ValueTask.FromResult(1);

        public ValueTask Handle(Archive request, CancellationToken cancellationToken) => default;

        public ValueTask Handle(Archived notification, CancellationToken cancellationToken) => default;
    }
}
",
          new[]
          {
                "Shapes.Archive => Shapes.EverythingHandler",
                "Shapes.GetCount => Shapes.EverythingHandler",
                "Shapes.GetName => Shapes.EverythingHandler",
          },
          new[] { "Shapes.Archived => Shapes.EverythingHandler" }),

    ["DerivedHandler"] = new ShapeSource(
          @"
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;

namespace Shapes
{
    public sealed class Ping : IRequest<string>
    {
    }

    public abstract class PingHandlerBase : IRequestHandler<Ping, string>
    {
        public abstract ValueTask<string> Handle(Ping request, CancellationToken cancellationToken);
    }

    public sealed class PingHandler : PingHandlerBase
    {
        public override ValueTask<string> Handle(Ping request, CancellationToken cancellationToken) => ValueTask.FromResult(""derived"");
    }
}
",
          new[] { "Shapes.Ping => Shapes.PingHandler" }),

    ["NotificationFanOut"] = new ShapeSource(
          @"
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;

namespace Shapes
{
    public class Changed : INotification
    {
    }

    public sealed class Renamed : Changed
    {
    }

    public sealed class AuditHandler : INotificationHandler<Changed>
    {
        public ValueTask Handle(Changed notification, CancellationToken cancellationToken) => default;
    }

    public sealed class CacheHandler : INotificationHandler<Changed>
    {
        public ValueTask Handle(Changed notification, CancellationToken cancellationToken) => default;
    }

    public sealed class RenamedHandler : INotificationHandler<Renamed>
    {
        public ValueTask Handle(Renamed notification, CancellationToken cancellationToken) => default;
    }
}
",
          Array.Empty<string>(),
          new[] { "Shapes.Changed => Shapes.AuditHandler, Shapes.CacheHandler", "Shapes.Renamed => Shapes.RenamedHandler" }),

    ["ScopedPipeline"] = new ShapeSource(
          @"
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;

namespace Shapes
{
    public sealed class ClientPipeline
    {
    }

    [MediatorScope(typeof(ClientPipeline))]
    public sealed class Ping : IRequest<string>
    {
    }

    [MediatorScope(typeof(ClientPipeline))]
    public sealed class PingHandler : IRequestHandler<Ping, string>
    {
        public ValueTask<string> Handle(Ping request, CancellationToken cancellationToken) => ValueTask.FromResult(""client"");
    }

    public sealed class Pong : IRequest<string>
    {
    }

    public sealed class PongHandler : IRequestHandler<Pong, string>
    {
        public ValueTask<string> Handle(Pong request, CancellationToken cancellationToken) => ValueTask.FromResult(""default"");
    }
}
",
          new[] { "Shapes.Ping => Shapes.PingHandler", "Shapes.Pong => Shapes.PongHandler" }),

    ["StructRequest"] = new ShapeSource(
          @"
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;

namespace Shapes
{
    public readonly struct Ping : IRequest<int>
    {
    }

    public sealed class PingHandler : IRequestHandler<Ping, int>
    {
        public ValueTask<int> Handle(Ping request, CancellationToken cancellationToken) => ValueTask.FromResult(7);
    }
}
",
          new[] { "Shapes.Ping => Shapes.PingHandler" }),

    ["Empty"] = new ShapeSource(
          "namespace Shapes { public sealed class Program { } }",
          Array.Empty<string>()),
  };

  private sealed class ShapeSource
  {
    public ShapeSource(string source, string[] requests, string[]? notifications = null)
    {
      Source = source;
      Requests = requests;
      Notifications = notifications ?? Array.Empty<string>();
    }

    public string Source { get; }

    public string[] Requests { get; }

    public string[] Notifications { get; }
  }
}