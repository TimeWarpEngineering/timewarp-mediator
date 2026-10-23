#region Purpose
// IIdempotentCommand (void + generic) and IQuery messages that must dispatch exactly like ICommand.
#endregion

namespace TimeWarp.Mediator.Generators.Tests.Idempotency;

public sealed class UpsertSetting : IIdempotentCommand<string>
{
  public string Key { get; set; } = string.Empty;

  public string Value { get; set; } = string.Empty;
}

public sealed class UpsertSettingHandler : IRequestHandler<UpsertSetting, string>
{
  public ValueTask<string> Handle(UpsertSetting request, CancellationToken cancellationToken)
  {
    return ValueTask.FromResult(request.Key + "=" + request.Value);
  }
}

public sealed class ResetSetting : IIdempotentCommand
{
  public string Key { get; set; } = string.Empty;
}

public sealed class ResetSettingHandler : IRequestHandler<ResetSetting>
{
  public static List<string> Resets { get; } = new();

  public ValueTask Handle(ResetSetting request, CancellationToken cancellationToken)
  {
    Resets.Add(request.Key);
    return default;
  }
}

public sealed class GetSetting : IQuery<string>
{
  public string Key { get; set; } = string.Empty;
}

public sealed class GetSettingHandler : IRequestHandler<GetSetting, string>
{
  public ValueTask<string> Handle(GetSetting request, CancellationToken cancellationToken)
  {
    return ValueTask.FromResult("value-of-" + request.Key);
  }
}