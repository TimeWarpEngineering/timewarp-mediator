#region Purpose
// State golden-file: IncrementActionSet + StateTransactionBehavior vs Reverse().Aggregate semantics.
#endregion

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using TimeWarp.Mediator;
using TimeWarp.Mediator.Generated;
using TimeWarp.Mediator.Generators.Tests.State;
using Xunit;
using GeneratedMediator = TimeWarp.Mediator.Generated.Mediator;

public class IncrementActionSetTests
{
    [Fact]
    public void GeneratedMediator_IsSealedAndImplementsIMediator()
    {
        typeof(GeneratedMediator).IsSealed.ShouldBeTrue();
        typeof(IMediator).IsAssignableFrom(typeof(GeneratedMediator)).ShouldBeTrue();
    }

    [Fact]
    public void Manifest_IsVersion1()
    {
        MediatorManifest.Version.ShouldBe(1);
        MediatorManifest.Json.ShouldContain("\"version\":1");
        MediatorManifest.Json.ShouldContain("IncrementActionSet");
        MediatorManifest.Json.ShouldContain("StateTransactionBehavior");
        string manifestPath = System.IO.Path.Combine(AppContext.BaseDirectory, "mediator.manifest.json");
        System.IO.File.WriteAllText(manifestPath, MediatorManifest.Json);
    }

    [Fact]
    public async Task Increment_ClonesStateAndAddsAmount()
    {
        Fixture fixture = Fixture.Create();
        Guid originalGuid = fixture.CounterState.Guid;

        await fixture.Generated.Send(new CounterState.IncrementActionSet.Action(2));

        fixture.Store.GetState<CounterState>().Count.ShouldBe(10);
        fixture.Store.GetState<CounterState>().Guid.ShouldNotBe(originalGuid);
    }

    [Fact]
    public async Task Increment_SendObject_UsesGeneratedSwitch()
    {
        Fixture fixture = Fixture.Create();

        object? boxed = await fixture.Generated.Send((object)new CounterState.IncrementActionSet.Action(1));

        boxed.ShouldBe(Unit.Value);
        fixture.Store.GetState<CounterState>().Count.ShouldBe(9);
    }

    [Fact]
    public async Task Increment_OnHandlerException_RestoresStateAndPublishes()
    {
        Fixture fixture = Fixture.Create();
        Guid originalGuid = fixture.CounterState.Guid;

        await fixture.Generated.Send(new CounterState.IncrementActionSet.Action(-1));

        CounterState state = fixture.Store.GetState<CounterState>();
        state.Count.ShouldBe(8);
        state.Guid.ShouldBe(originalGuid);
        fixture.Recorder.Notifications.Count.ShouldBe(1);
        fixture.Recorder.Notifications[0].Exception.ShouldBeOfType<InvalidOperationException>();
    }

    [Fact]
    public async Task Increment_ShortCircuit_SkipsHandler()
    {
        Fixture fixture = Fixture.Create();
        ShortCircuitBehavior<CounterState.IncrementActionSet.Action, Unit>.ShortCircuit = true;
        try
        {
            await fixture.Generated.Send(new CounterState.IncrementActionSet.Action(5));
            fixture.Store.GetState<CounterState>().Count.ShouldBe(8);
            PipelineLog.Events.ShouldContain("short-circuit");
            PipelineLog.Events.ShouldNotContain("handler");
        }
        finally
        {
            ShortCircuitBehavior<CounterState.IncrementActionSet.Action, Unit>.ShortCircuit = false;
        }
    }

    [Fact]
    public async Task PipelineOrder_MatchesReverseAggregate()
    {
        Fixture fixture = Fixture.Create();
        PipelineLog.Clear();
        string generated = await fixture.Generated.Send(new Ping { Message = "hi" });
        string[] generatedOrder = PipelineLog.Events.ToArray();

        PipelineLog.Clear();
        TimeWarp.Mediator.Mediator legacy = new(fixture.Scope.ServiceProvider);
        string legacyResult = await legacy.Send(new Ping { Message = "hi" });
        string[] legacyOrder = PipelineLog.Events.ToArray();

        generated.ShouldBe("hi-pong");
        legacyResult.ShouldBe("hi-pong");
        generatedOrder.ShouldBe(new[] { "outer-before", "inner-before", "handler", "inner-after", "outer-after" });
        legacyOrder.ShouldBe(generatedOrder);
    }

    [Fact]
    public async Task ReentrantSend_UsesInjectableISender()
    {
        Fixture fixture = Fixture.Create();
        int result = await fixture.Generated.Send(new ReentrantAction { Value = 0 });
        result.ShouldBe(2);
    }

    [Fact]
    public async Task UnknownObject_ThrowsNoHandlerException()
    {
        Fixture fixture = Fixture.Create();
        await Should.ThrowAsync<NoHandlerException>(() => fixture.Generated.Send(new object()));
    }

    private sealed class Fixture
    {
        private Fixture(
            IServiceScope scope,
            GeneratedMediator generated,
            InMemoryStore store,
            CounterState counterState,
            RecordingExceptionHandler recorder)
        {
            Scope = scope;
            Generated = generated;
            Store = store;
            CounterState = counterState;
            Recorder = recorder;
        }

        public IServiceScope Scope { get; }

        public GeneratedMediator Generated { get; }

        public InMemoryStore Store { get; }

        public CounterState CounterState { get; }

        public RecordingExceptionHandler Recorder { get; }

        public static Fixture Create()
        {
            PipelineLog.Clear();
            InMemoryStore store = new();
            CounterState counterState = new() { Count = 8 };
            store.SetState(counterState);

            ServiceCollection services = new();
            services.AddGeneratedMediator();
            services.AddSingleton<IStore>(store);
            ServiceProvider provider = services.BuildServiceProvider();
            IServiceScope scope = provider.CreateScope();
            IServiceProvider scoped = scope.ServiceProvider;
            GeneratedMediator generated = scoped.GetRequiredService<GeneratedMediator>();
            counterState.Sender = generated;
            RecordingExceptionHandler recorder = scoped.GetRequiredService<RecordingExceptionHandler>();
            return new Fixture(scope, generated, store, counterState, recorder);
        }
    }
}
