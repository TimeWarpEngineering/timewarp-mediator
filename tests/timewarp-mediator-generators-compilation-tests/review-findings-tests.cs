#region Purpose
// Output parity with the symbol walk for the review round 1 findings the goldens do not reach.
#endregion

#region Design
// Each test is one finding: empty host-name property, notification scope, strong name,
// array interface constraints, derived-handler dedupe, aliased behaviors, type-parameter
// and nested pipeline slots, duplicate references, notification drops, assembly order,
// and a host-declared runtime type.
#endregion

using System.Security.Cryptography;

namespace TimeWarp.Mediator.Generators.Compilation.Tests;

public sealed class ReviewFindingsTests
{
  [Fact]
  public void EmptyAssemblyNameProperty_StillTreatsTheCompilationAsTheHost()
  {
    HostCompilation host = HostCompiler.Compile("EmptyPropertyHost", BehaviorHost, "");

    host.Diagnostics.ShouldNotContain(d => d.Id == "TWM001" || d.Id == "TWM010" || d.Id == "TWM011");
    host.Manifest().Membership.ShouldContain("EmptyPropertyHost");
    host.Manifest().Request("App.Ping").Behaviors.ShouldBe(new[] { "App.OpenBehavior<App.Ping, string>" });
  }

  [Fact]
  public void NotificationScopeOnTheMessage_AssignsThePublisher()
  {
    HostCompilation host = HostCompiler.Compile("ScopedNote", NotificationScopeSource);

    host.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
    host.Manifest().Notification("App.InvoicePaid").Scope.ShouldBe("App.Billing");
  }

  [Fact]
  public void NotificationScopeMismatch_IsTwm003()
  {
    HostCompilation host = HostCompiler.Compile(
        "MismatchedNote",
        NotificationScopeSource.Replace(
            "public sealed class InvoicePaidHandler",
            "[MediatorScope(typeof(Shipping))]\n    public sealed class InvoicePaidHandler"));

    Diagnostic diagnostic = host.Diagnostics.Single(d => d.Id == "TWM003");
    diagnostic.GetMessage().ShouldContain("App.Billing");
    diagnostic.GetMessage().ShouldContain("App.Shipping");
  }

  [Fact]
  public void StrongNamedHost_BindsInternalsVisibleToWithPublicKey()
  {
    ImmutableArray<byte> publicKey = CreatePublicKey();
    string hex = ToHex(publicKey);
    MetadataReference member = HostCompiler.CompileLibrary(
        "SignedVisible",
        InternalHandlerSource.Replace(
            "[assembly: MediatorAssembly]",
            "[assembly: MediatorAssembly]\n[assembly: System.Runtime.CompilerServices.InternalsVisibleTo(\"SignedHost, PublicKey=" + hex + "\")]"));

    HostCompilation host = HostCompiler.CompileSigned("SignedHost", "namespace App { public sealed class Program { } }", publicKey, member);

    host.Diagnostics.ShouldNotContain(d => d.Id == "TWM007" || d.Id == "TWM001" || d.Id == "CS0122");
    host.Manifest().Request("Members.Hidden").Handler.ShouldBe("Members.HiddenHandler");
  }

  [Fact]
  public void ArrayResponse_ClosesInterfaceConstraint()
  {
    HostCompilation host = HostCompiler.Compile("ArrayResponse", ArrayBehaviorSource);

    host.Diagnostics.ShouldNotContain(d => d.Id == "TWM011");
    host.Manifest().Request("App.GetIds").Behaviors.ShouldBe(new[] { "App.Enumerate<App.GetIds, int[]>" });
  }

  [Fact]
  public void DerivedHandlerWithADifferentResponse_IsTwm002()
  {
    HostCompilation host = HostCompiler.Compile("DerivedHandler", DerivedHandlerSource);

    Diagnostic diagnostic = host.Diagnostics.Single(d => d.Id == "TWM002");
    diagnostic.GetMessage().ShouldContain("App.BaseHandler");
    diagnostic.GetMessage().ShouldContain("App.DerivedHandler");
  }

  [Fact]
  public void AliasedMediatorBehavior_IsWoven()
  {
    HostCompilation host = HostCompiler.Compile("AliasedBehavior", AliasedBehaviorSource);

    host.Diagnostics.ShouldNotContain(d => d.Id == "TWM010" || d.Id == "TWM011");
    host.Manifest().Request("App.Ping").Behaviors.ShouldBe(new[] { "App.OpenBehavior<App.Ping, string>" });
  }

  [Fact]
  public void TypeParameterConstraint_DoesNotClose()
  {
    HostCompilation host = HostCompiler.Compile("TypeParameterConstraint", TypeParameterConstraintSource);

    host.Manifest().Request("App.Ping").Behaviors.ShouldBeEmpty();
    host.Diagnostics.ShouldContain(d => d.Id == "TWM011");
  }

  [Fact]
  public void NestedPipelineArgument_Closes()
  {
    HostCompilation host = HostCompiler.Compile("NestedPipeline", NestedPipelineSource);

    host.Diagnostics.ShouldNotContain(d => d.Id == "TWM011");
    host.Manifest().Request("App.Ping").Behaviors.ShouldBe(new[] { "App.Wrap<App.Ping>" });
  }

  [Fact]
  public void DuplicateReference_RegistersTheBehaviorOnce()
  {
    MetadataReference member = HostCompiler.CompileLibrary("BehaviorLib", LibraryBehaviorSource);

    HostCompilation host = HostCompiler.Compile("DuplicateRef", "namespace App { public sealed class Program { } }", member, member);

    host.Manifest().Request("Lib.Ping").Behaviors.ShouldBe(new[] { "Lib.OpenBehavior<Lib.Ping, string>" });
  }

  [Fact]
  public void InaccessibleNotificationHandler_DoesNotSuppressTwm001()
  {
    MetadataReference member = HostCompiler.CompileLibrary("BothMember", BothSource);

    HostCompilation host = HostCompiler.Compile("BothHost", "namespace App { public sealed class Program { } }", member);

    host.Diagnostics.ShouldContain(d => d.Id == "TWM007");
    host.Diagnostics.ShouldContain(d => d.Id == "TWM001" && d.GetMessage().Contains("Members.Both"));
  }

  [Fact]
  public void MissingRequestDiagnostics_FollowAssemblyName()
  {
    MetadataReference zeta = HostCompiler.CompileLibrary("Zeta", OrphanSource("Zeta"));
    MetadataReference alpha = HostCompiler.CompileLibrary("Alpha", OrphanSource("Alpha"));

    string forward = Twm001Order(HostCompiler.Compile("OrderHost", "namespace App { public sealed class Program { } }", zeta, alpha));
    string reverse = Twm001Order(HostCompiler.Compile("OrderHost", "namespace App { public sealed class Program { } }", alpha, zeta));

    forward.ShouldBe("Request 'Alpha.Orphan' has no handler" + Environment.NewLine + "Request 'Zeta.Orphan' has no handler");
    reverse.ShouldBe(forward);
  }

  [Fact]
  public void HostDeclaredRuntimeType_IsNotAFalseTwm008()
  {
    string source = """
        namespace Microsoft.Extensions.DependencyInjection
        {
            public interface IServiceCollection { }
        }

        namespace App
        {
            public sealed class Program { }
        }
        """;
    HostCompilation host = HostCompiler.CompileWithReferences(
        "DeclaresCollection",
        source,
        HostCompiler.GetSystemReferences().Append(MetadataReference.CreateFromFile(typeof(IMediator).Assembly.Location)));

    Diagnostic diagnostic = host.Diagnostics.Single(d => d.Id == "TWM008");
    diagnostic.GetMessage().ShouldContain("ServiceCollectionServiceExtensions");
    diagnostic.GetMessage().ShouldNotContain("which this compilation cannot resolve; nothing is generated. Reference the TimeWarp.Mediator package");
    diagnostic.GetMessage().ShouldNotContain("'Microsoft.Extensions.DependencyInjection.IServiceCollection'");
  }

  private static string Twm001Order(HostCompilation host)
  {
    return string.Join(
        Environment.NewLine,
        host.Diagnostics.Where(d => d.Id == "TWM001").Select(d => d.GetMessage()));
  }

  private static ImmutableArray<byte> CreatePublicKey()
  {
    using RSA rsa = RSA.Create(2048);
    RSAParameters parameters = rsa.ExportParameters(false);
    byte[] modulus = (byte[])parameters.Modulus!.Clone();
    Array.Reverse(modulus);
    byte[] exponent = parameters.Exponent!;
    byte[] exp = new byte[4];
    for (int i = 0; i < exponent.Length && i < 4; i++)
    {
      exp[i] = exponent[exponent.Length - 1 - i];
    }

    using MemoryStream stream = new();
    using BinaryWriter writer = new(stream);
    writer.Write(0x00002400);
    writer.Write(0x00008004);
    writer.Write(20 + modulus.Length);
    writer.Write((byte)0x06);
    writer.Write((byte)0x02);
    writer.Write((ushort)0);
    writer.Write(0x00002400);
    writer.Write(0x31415352);
    writer.Write(modulus.Length * 8);
    writer.Write(exp);
    writer.Write(modulus);
    return ImmutableArray.Create(stream.ToArray());
  }

  private static string ToHex(ImmutableArray<byte> publicKey)
  {
    char[] hex = new char[publicKey.Length * 2];
    const string digits = "0123456789ABCDEF";
    for (int i = 0; i < publicKey.Length; i++)
    {
      hex[i * 2] = digits[publicKey[i] >> 4];
      hex[(i * 2) + 1] = digits[publicKey[i] & 0xF];
    }

    return new string(hex);
  }

  private static string OrphanSource(string ns) => @"
using TimeWarp.Mediator;

[assembly: MediatorAssembly]

namespace " + ns + @"
{
    public sealed class Orphan : IRequest<int> { }
}
";

  private const string BehaviorHost = @"
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;

[assembly: MediatorBehavior(typeof(App.OpenBehavior<,>))]

namespace App
{
    public sealed class Ping : IRequest<string> { }

    public sealed class PingHandler : IRequestHandler<Ping, string>
    {
        public ValueTask<string> Handle(Ping request, CancellationToken cancellationToken) => ValueTask.FromResult(""pong"");
    }

    public sealed class OpenBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        public ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken) => next(request, cancellationToken);
    }
}
";

  private const string NotificationScopeSource = @"
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;

namespace App
{
    public sealed class Billing { }

    public sealed class Shipping { }

    [MediatorScope(typeof(Billing))]
    public sealed class InvoicePaid : INotification { }

    public sealed class InvoicePaidHandler : INotificationHandler<InvoicePaid>
    {
        public ValueTask Handle(InvoicePaid notification, CancellationToken cancellationToken) => default;
    }
}
";

  private const string InternalHandlerSource = @"
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;

[assembly: MediatorAssembly]

namespace Members
{
    public sealed class Hidden : IRequest<int> { }

    internal sealed class HiddenHandler : IRequestHandler<Hidden, int>
    {
        public ValueTask<int> Handle(Hidden request, CancellationToken cancellationToken) => ValueTask.FromResult(1);
    }
}
";

  private const string ArrayBehaviorSource = @"
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;

[assembly: MediatorBehavior(typeof(App.Enumerate<,>))]

namespace App
{
    public sealed class GetIds : IRequest<int[]> { }

    public sealed class GetIdsHandler : IRequestHandler<GetIds, int[]>
    {
        public ValueTask<int[]> Handle(GetIds request, CancellationToken cancellationToken) => ValueTask.FromResult(new[] { 1 });
    }

    public sealed class Enumerate<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
        where TResponse : IEnumerable
    {
        public ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken) => next(request, cancellationToken);
    }
}
";

  private const string DerivedHandlerSource = @"
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;

namespace App
{
    public sealed class Ping : IRequest<int> { }

    public class BaseHandler : IRequestHandler<Ping, int>
    {
        public ValueTask<int> Handle(Ping request, CancellationToken cancellationToken) => ValueTask.FromResult(1);
    }

    public sealed class DerivedHandler : BaseHandler, IRequestHandler<Ping, string>
    {
        public ValueTask<string> Handle(Ping request, CancellationToken cancellationToken) => ValueTask.FromResult(""two"");
    }
}
";

  private const string AliasedBehaviorSource = @"
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;
using Weave = TimeWarp.Mediator.MediatorBehaviorAttribute;

[assembly: Weave(typeof(App.OpenBehavior<,>))]

namespace App
{
    public sealed class Ping : IRequest<string> { }

    public sealed class PingHandler : IRequestHandler<Ping, string>
    {
        public ValueTask<string> Handle(Ping request, CancellationToken cancellationToken) => ValueTask.FromResult(""pong"");
    }

    public sealed class OpenBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        public ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken) => next(request, cancellationToken);
    }
}
";

  private const string TypeParameterConstraintSource = @"
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;

[assembly: MediatorBehavior(typeof(App.Linked<,>))]

namespace App
{
    public sealed class Ping : IRequest<string> { }

    public sealed class PingHandler : IRequestHandler<Ping, string>
    {
        public ValueTask<string> Handle(Ping request, CancellationToken cancellationToken) => ValueTask.FromResult(""pong"");
    }

    public sealed class Linked<TReq, TRes> : IPipelineBehavior<TReq, TRes>
        where TReq : notnull
        where TRes : TReq
    {
        public ValueTask<TRes> Handle(TReq request, MessageHandlerDelegate<TReq, TRes> next, CancellationToken cancellationToken) => next(request, cancellationToken);
    }
}
";

  private const string NestedPipelineSource = @"
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;

[assembly: MediatorBehavior(typeof(App.Wrap<>))]

namespace App
{
    public sealed class Result<T>
    {
        public T? Value { get; set; }
    }

    public sealed class Ping : IRequest<Result<Ping>> { }

    public sealed class PingHandler : IRequestHandler<Ping, Result<Ping>>
    {
        public ValueTask<Result<Ping>> Handle(Ping request, CancellationToken cancellationToken) => ValueTask.FromResult(new Result<Ping>());
    }

    public sealed class Wrap<TRequest> : IPipelineBehavior<TRequest, Result<TRequest>>
        where TRequest : notnull
    {
        public ValueTask<Result<TRequest>> Handle(TRequest request, MessageHandlerDelegate<TRequest, Result<TRequest>> next, CancellationToken cancellationToken) => next(request, cancellationToken);
    }
}
";

  private const string LibraryBehaviorSource = @"
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;

[assembly: MediatorAssembly]
[assembly: MediatorBehavior(typeof(Lib.OpenBehavior<,>))]

namespace Lib
{
    public sealed class Ping : IRequest<string> { }

    public sealed class PingHandler : IRequestHandler<Ping, string>
    {
        public ValueTask<string> Handle(Ping request, CancellationToken cancellationToken) => ValueTask.FromResult(""pong"");
    }

    public sealed class OpenBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        public ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken) => next(request, cancellationToken);
    }
}
";

  private const string BothSource = @"
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;

[assembly: MediatorAssembly]

namespace Members
{
    public sealed class Both : IRequest<int>, INotification { }

    internal sealed class BothNoteHandler : INotificationHandler<Both>
    {
        public ValueTask Handle(Both notification, CancellationToken cancellationToken) => default;
    }
}
";
}
