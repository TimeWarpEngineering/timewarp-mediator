#region Purpose
// IIdempotentCommand (void + generic) and IQuery messages that must dispatch exactly like ICommand.
#endregion

namespace TimeWarp.Mediator.Generators.Tests.Idempotency;

public sealed class UpsertSetting : IIdempotentCommand<string>
{
    public string Key { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;
}

public sealed class UpsertSettingHandler : IIdempotentCommandHandler<UpsertSetting, string>
{
    public Task<string> Handle(UpsertSetting request, CancellationToken cancellationToken)
    {
        return Task.FromResult(request.Key + "=" + request.Value);
    }
}

public sealed class ResetSetting : IIdempotentCommand
{
    public string Key { get; set; } = string.Empty;
}

public sealed class ResetSettingHandler : IIdempotentCommandHandler<ResetSetting>
{
    public static List<string> Resets { get; } = new();

    public Task Handle(ResetSetting request, CancellationToken cancellationToken)
    {
        Resets.Add(request.Key);
        return Task.CompletedTask;
    }
}

public sealed class GetSetting : IQuery<string>
{
    public string Key { get; set; } = string.Empty;
}

public sealed class GetSettingHandler : IQueryHandler<GetSetting, string>
{
    public Task<string> Handle(GetSetting request, CancellationToken cancellationToken)
    {
        return Task.FromResult("value-of-" + request.Key);
    }
}
