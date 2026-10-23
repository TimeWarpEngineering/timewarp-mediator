#region Purpose
// #68: IRequestHandler<T, Unit> dispatches as response-returning, IRequestHandler<T> stays void, and
// array / List<T> query handlers are discovered, dispatched, and wrapped by behaviors.
#endregion

using GeneratedMediator = TimeWarp.Mediator.Generated.Mediator;

public class ResponseShapeTests
{
  [Fact]
  public void AddGeneratedMediator_RegistersConcreteHandlersOnly()
  {
    using ServiceProvider provider = CreateProvider();
    using IServiceScope scope = provider.CreateScope();

    scope.ServiceProvider.GetRequiredService<CreateWidgetHandler>().ShouldBeOfType<CreateWidgetHandler>();
    scope.ServiceProvider.GetRequiredService<ArchiveWidgetHandler>().ShouldBeOfType<ArchiveWidgetHandler>();
    scope.ServiceProvider.GetService<IRequestHandler<CreateWidget, Unit>>().ShouldBeNull();
    scope.ServiceProvider.GetService<IRequestHandler<ArchiveWidget>>().ShouldBeNull();
  }

  [Fact]
  public async Task Send_ExplicitUnitCommand_ReturnsHandlerResponse()
  {
    using ServiceProvider provider = CreateProvider();
    using IServiceScope scope = provider.CreateScope();
    GeneratedMediator generated = scope.ServiceProvider.GetRequiredService<GeneratedMediator>();
    ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();
    CreateWidgetHandler.Created.Clear();

    ValueTask<Unit> monomorphic = generated.Send(new CreateWidget { Name = "a" });
    Unit direct = await monomorphic;
    Unit viaSender = await sender.Send(new CreateWidget { Name = "b" });
    object? boxed = await sender.Send((object) new CreateWidget { Name = "c" });

    direct.ShouldBe(Unit.Value);
    viaSender.ShouldBe(Unit.Value);
    boxed.ShouldBe(Unit.Value);
    CreateWidgetHandler.Created.ShouldBe(new[] { "a", "b", "c" });
  }

  [Fact]
  public async Task Send_VoidCommand_StaysVoid()
  {
    using ServiceProvider provider = CreateProvider();
    using IServiceScope scope = provider.CreateScope();
    ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();
    ArchiveWidgetHandler.Archived.Clear();

    await sender.Send(new ArchiveWidget { Name = "old" });
    object? boxed = await sender.Send((object) new ArchiveWidget { Name = "older" });

    boxed.ShouldBe(Unit.Value);
    ArchiveWidgetHandler.Archived.ShouldBe(new[] { "old", "older" });
  }

  [Fact]
  public async Task Send_ArrayQuery_IsDiscoveredAndDispatches()
  {
    using ServiceProvider provider = CreateProvider();
    using IServiceScope scope = provider.CreateScope();
    GeneratedMediator generated = scope.ServiceProvider.GetRequiredService<GeneratedMediator>();
    ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();

    scope.ServiceProvider.GetRequiredService<SearchWidgetsHandler>().ShouldBeOfType<SearchWidgetsHandler>();
    scope.ServiceProvider.GetService<IRequestHandler<SearchWidgets, string[]>>().ShouldBeNull();

    string[] direct = await generated.Send(new SearchWidgets { Prefix = "w" });
    string[] viaSender = await sender.Send(new SearchWidgets { Prefix = "x" });
    object? boxed = await sender.Send((object) new SearchWidgets { Prefix = "y" });

    direct.ShouldBe(new[] { "w-1", "w-2" });
    viaSender.ShouldBe(new[] { "x-1", "x-2" });
    boxed.ShouldBe(new[] { "y-1", "y-2" });
  }

  [Fact]
  public async Task Send_ListQuery_IsDiscoveredAndDispatches()
  {
    using ServiceProvider provider = CreateProvider();
    using IServiceScope scope = provider.CreateScope();
    ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();

    List<string> widgets = await sender.Send(new ListWidgets { Count = 2 });

    widgets.ShouldBe(new[] { "widget-1", "widget-2" });
  }

  [Fact]
  public async Task ArrayQuery_BehaviorsCloseOverArrayResponse()
  {
    using ServiceProvider provider = CreateProvider();
    using IServiceScope scope = provider.CreateScope();
    ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();
    PipelineLog.Clear();

    await sender.Send(new SearchWidgets { Prefix = "p" });

    PipelineLog.Events.ShouldBe(new[]
    {
            "outer-before",
            "inner-before",
            "array-before",
            "handler",
            "array-after",
            "inner-after",
            "outer-after",
        });
    scope.ServiceProvider.GetRequiredService<SearchWidgetsAuditBehavior>().ShouldBeOfType<SearchWidgetsAuditBehavior>();
    scope.ServiceProvider.GetService<IPipelineBehavior<SearchWidgets, string[]>>().ShouldBeNull();
  }

  [Fact]
  public async Task ExplicitUnitCommand_UnitBehaviorsStillWrapHandler()
  {
    using ServiceProvider provider = CreateProvider();
    using IServiceScope scope = provider.CreateScope();
    ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();
    PipelineLog.Clear();

    await sender.Send(new CreateWidget { Name = "wrapped" });

    PipelineLog.Events.ShouldBe(new[] { "outer-before", "inner-before", "handler", "inner-after", "outer-after" });
    ManifestDocument.Parse(MediatorManifest.Json)
        .Request("TimeWarp.Mediator.Generators.Tests.Responses.CreateWidget")
        .Behaviors.ShouldContain("TimeWarp.Mediator.Generators.Tests.State.UnitOnlyBehavior<TimeWarp.Mediator.Generators.Tests.Responses.CreateWidget>");
  }

  [Fact]
  public void Manifest_RecordsResponseShapes()
  {
    ManifestDocument manifest = ManifestDocument.Parse(MediatorManifest.Json);
    ManifestRequest search = manifest.Request("TimeWarp.Mediator.Generators.Tests.Responses.SearchWidgets");
    search.Response.ShouldBe("string[]");
    search.Kind.ShouldBe("Query");
    search.Behaviors.ShouldContain("TimeWarp.Mediator.Generators.Tests.Responses.SearchWidgetsAuditBehavior");
    ManifestRequest list = manifest.Request("TimeWarp.Mediator.Generators.Tests.Responses.ListWidgets");
    list.Response.ShouldBe("System.Collections.Generic.List<string>");
    list.Kind.ShouldBe("Query");
    ManifestRequest create = manifest.Request("TimeWarp.Mediator.Generators.Tests.Responses.CreateWidget");
    create.Response.ShouldBe("TimeWarp.Mediator.Unit");
    create.Kind.ShouldBe("Command");
    manifest.Request("TimeWarp.Mediator.Generators.Tests.Responses.ArchiveWidget").Kind.ShouldBe("Command");
  }

  private static ServiceProvider CreateProvider()
  {
    ServiceCollection services = new();
    services.AddGeneratedMediator();
    return services.BuildServiceProvider();
  }
}