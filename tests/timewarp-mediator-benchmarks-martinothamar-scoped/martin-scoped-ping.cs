#region Purpose
// Scoped martinothamar send that pairs with TimeWarpGeneratedScopedSend.
#endregion

#region Design
// martinothamar emits one ServiceLifetime per compilation, so this assembly is Scoped
// and the comparison project stays Transient for MartinothamarGenerated.
// The handler body matches ScopedPingHandler: read ScopeToken, return a pong of the message.
#endregion

using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace MartinothamarScopedBenchmarks;

public sealed class MartinScopedPing : IRequest<MartinScopedPong>
{
  public string Message { get; set; } = "hi";
}

public sealed class MartinScopedPong
{
  public string Message { get; set; } = string.Empty;
}

public sealed class MartinScopedPingHandler : IRequestHandler<MartinScopedPing, MartinScopedPong>
{
  private readonly BenchmarkPeers.ScopeToken ScopeToken;

  public MartinScopedPingHandler(BenchmarkPeers.ScopeToken scopeToken)
  {
    ScopeToken = scopeToken;
  }

  public ValueTask<MartinScopedPong> Handle(MartinScopedPing request, CancellationToken cancellationToken)
  {
    _ = ScopeToken;
    return ValueTask.FromResult(new MartinScopedPong { Message = request.Message });
  }
}

public static class MartinScopedRegistration
{
  public static void Add(IServiceCollection services)
  {
    services.AddScoped<BenchmarkPeers.ScopeToken>();
    services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);
  }
}
