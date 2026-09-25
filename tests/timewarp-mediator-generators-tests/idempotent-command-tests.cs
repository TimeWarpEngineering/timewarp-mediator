#region Purpose
// IIdempotentCommand (void + generic) is registered by AddGeneratedMediator and dispatched like ICommand.
#endregion

[Collection(PipelineLogCollection.Name)]
public class IdempotentCommandTests
{
    [Fact]
    public void Contracts_IdempotentCommandIsCommandAndIdempotent()
    {
        typeof(ICommand<string>).IsAssignableFrom(typeof(UpsertSetting)).ShouldBeTrue();
        typeof(IIdempotent).IsAssignableFrom(typeof(UpsertSetting)).ShouldBeTrue();
        typeof(ICommand).IsAssignableFrom(typeof(ResetSetting)).ShouldBeTrue();
        typeof(IIdempotent).IsAssignableFrom(typeof(ResetSetting)).ShouldBeTrue();
        typeof(IIdempotent).IsAssignableFrom(typeof(GetSetting)).ShouldBeTrue();
    }

    [Fact]
    public void AddGeneratedMediator_RegistersIdempotentCommandHandlers()
    {
        using ServiceProvider provider = CreateProvider();
        using IServiceScope scope = provider.CreateScope();

        scope.ServiceProvider.GetService<IRequestHandler<UpsertSetting, string>>()
            .ShouldBeOfType<UpsertSettingHandler>();
        scope.ServiceProvider.GetService<IRequestHandler<ResetSetting>>()
            .ShouldBeOfType<ResetSettingHandler>();
    }

    [Fact]
    public async Task Send_GenericIdempotentCommand_ReachesHandler()
    {
        using ServiceProvider provider = CreateProvider();
        using IServiceScope scope = provider.CreateScope();
        ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();

        string result = await sender.Send(new UpsertSetting { Key = "theme", Value = "dark" });

        result.ShouldBe("theme=dark");
    }

    [Fact]
    public async Task Send_VoidIdempotentCommand_ReachesHandler()
    {
        using ServiceProvider provider = CreateProvider();
        using IServiceScope scope = provider.CreateScope();
        ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();
        ResetSettingHandler.Resets.Clear();

        await sender.Send(new ResetSetting { Key = "theme" });

        ResetSettingHandler.Resets.ShouldBe(new[] { "theme" });
    }

    [Fact]
    public async Task SendObject_IdempotentCommands_UseGeneratedSwitch()
    {
        using ServiceProvider provider = CreateProvider();
        using IServiceScope scope = provider.CreateScope();
        ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();
        ResetSettingHandler.Resets.Clear();

        object? generic = await sender.Send((object)new UpsertSetting { Key = "a", Value = "b" });
        object? unit = await sender.Send((object)new ResetSetting { Key = "a" });

        generic.ShouldBe("a=b");
        unit.ShouldBe(Unit.Value);
        ResetSettingHandler.Resets.ShouldBe(new[] { "a" });
    }

    [Fact]
    public async Task Send_Query_StillReachesHandler()
    {
        using ServiceProvider provider = CreateProvider();
        using IServiceScope scope = provider.CreateScope();
        ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();

        string result = await sender.Send(new GetSetting { Key = "theme" });

        result.ShouldBe("value-of-theme");
    }

    [Fact]
    public void Manifest_RecordsIdempotentCommands()
    {
        MediatorManifest.Json.ShouldContain("UpsertSetting");
        MediatorManifest.Json.ShouldContain("ResetSetting");
    }

    private static ServiceProvider CreateProvider()
    {
        ServiceCollection services = new();
        services.AddGeneratedMediator();
        return services.BuildServiceProvider();
    }
}
