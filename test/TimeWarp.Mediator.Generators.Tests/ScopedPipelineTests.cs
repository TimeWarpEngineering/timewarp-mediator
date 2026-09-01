#region Purpose
// Named pipelines: disjoint dispatch tables, behaviors, re-entrancy, and MS.DI resolution.
#endregion

using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using TimeWarp.Mediator;
using TimeWarp.Mediator.Generated;
using TimeWarp.Mediator.Generators.Tests.Pipelines;
using Xunit;

public class ScopedPipelineTests
{
    [Fact]
    public async Task ClientSender_DispatchesClientPingAndClientBehavior()
    {
        Fixture fixture = Fixture.Create();

        string result = await fixture.Client.Send(new ClientPing { Message = "hi" });

        result.ShouldBe("client:hi");
        ClientStampBehavior<ClientPing, string>.Events.ShouldContain("client-stamp");
        ServerStampBehavior<ServerPing, string>.Events.ShouldBeEmpty();
    }

    [Fact]
    public async Task ServerSender_DispatchesServerPingAndServerBehavior()
    {
        Fixture fixture = Fixture.Create();

        string result = await fixture.Server.Send(new ServerPing { Message = "hi" });

        result.ShouldBe("server:hi");
        ServerStampBehavior<ServerPing, string>.Events.ShouldContain("server-stamp");
        ClientStampBehavior<ClientPing, string>.Events.ShouldBeEmpty();
    }

    [Fact]
    public async Task ClientSender_ObjectSendOfServerPing_ThrowsNoHandlerException()
    {
        Fixture fixture = Fixture.Create();

        await Should.ThrowAsync<NoHandlerException>(
            () => fixture.Client.Send((object)new ServerPing { Message = "nope" }));
    }

    [Fact]
    public async Task UnscopedSender_ObjectSendOfClientPing_ThrowsNoHandlerException()
    {
        Fixture fixture = Fixture.Create();

        await Should.ThrowAsync<NoHandlerException>(
            () => fixture.Unscoped.Send((object)new ClientPing { Message = "nope" }));
    }

    [Fact]
    public void ClientSender_HasMonomorphicClientPing_NotServerPing()
    {
        MethodInfo[] methods = typeof(Sender_ClientPipeline).GetMethods(BindingFlags.Public | BindingFlags.Instance);
        methods.Any(method => IsMonomorphicSend(method, typeof(ClientPing))).ShouldBeTrue();
        methods.Any(method => IsMonomorphicSend(method, typeof(ServerPing))).ShouldBeFalse();
    }

    [Fact]
    public async Task ReentrantScopedSend_StaysInClientPipeline()
    {
        Fixture fixture = Fixture.Create();

        int result = await fixture.Client.Send(new ClientReentrant { Value = 0 });

        result.ShouldBe(2);
        ClientStampBehavior<ClientReentrant, int>.Events.Count.ShouldBeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task ClientPublisher_DoesNotInvokeServerNotificationHandlers()
    {
        Fixture fixture = Fixture.Create();
        ClientNoteHandler.Count = 0;
        ServerNoteHandler.Count = 0;

        await fixture.ClientPublisher.Publish(new ClientNote());
        await fixture.ServerPublisher.Publish(new ServerNote());

        ClientNoteHandler.Count.ShouldBe(1);
        ServerNoteHandler.Count.ShouldBe(1);

        await fixture.ClientPublisher.Publish((object)new ServerNote());
        ServerNoteHandler.Count.ShouldBe(1);
    }

    [Fact]
    public void Di_ResolvesScopedSendersIndependentlyOfUnscoped()
    {
        Fixture fixture = Fixture.Create();

        fixture.Client.ShouldNotBeSameAs(fixture.Unscoped);
        fixture.Server.ShouldNotBeSameAs(fixture.Unscoped);
        fixture.Client.ShouldNotBeSameAs(fixture.Server);
        fixture.Client.ShouldBeOfType<Sender_ClientPipeline>();
        fixture.Server.ShouldBeOfType<Sender_ServerPipeline>();
    }

    [Fact]
    public void Manifest_RecordsScopedRequests()
    {
        MediatorManifest.Json.ShouldContain("ClientPipeline");
        MediatorManifest.Json.ShouldContain("ServerPipeline");
        MediatorManifest.Json.ShouldContain("ClientPing");
        MediatorManifest.Json.ShouldContain("ServerPing");
    }

    private static bool IsMonomorphicSend(MethodInfo method, System.Type requestType)
    {
        if (method.Name != "Send" || method.IsGenericMethod)
        {
            return false;
        }

        ParameterInfo[] parameters = method.GetParameters();
        return parameters.Length >= 1 && parameters[0].ParameterType == requestType;
    }

    private sealed class Fixture
    {
        private Fixture(
            ISender unscoped,
            ISender<ClientPipeline> client,
            ISender<ServerPipeline> server,
            IPublisher<ClientPipeline> clientPublisher,
            IPublisher<ServerPipeline> serverPublisher)
        {
            Unscoped = unscoped;
            Client = client;
            Server = server;
            ClientPublisher = clientPublisher;
            ServerPublisher = serverPublisher;
        }

        public ISender Unscoped { get; }

        public ISender<ClientPipeline> Client { get; }

        public ISender<ServerPipeline> Server { get; }

        public IPublisher<ClientPipeline> ClientPublisher { get; }

        public IPublisher<ServerPipeline> ServerPublisher { get; }

        public static Fixture Create()
        {
            ClientStampBehavior<ClientPing, string>.Events.Clear();
            ClientStampBehavior<ClientReentrant, int>.Events.Clear();
            ServerStampBehavior<ServerPing, string>.Events.Clear();
            ClientNoteHandler.Count = 0;
            ServerNoteHandler.Count = 0;

            ServiceCollection services = new();
            services.AddGeneratedMediator();
            services.AddGeneratedMediator<ClientPipeline>();
            services.AddGeneratedMediator<ServerPipeline>();
            ServiceProvider provider = services.BuildServiceProvider();
            IServiceScope scope = provider.CreateScope();
            IServiceProvider scoped = scope.ServiceProvider;
            return new Fixture(
                scoped.GetRequiredService<ISender>(),
                scoped.GetRequiredService<ISender<ClientPipeline>>(),
                scoped.GetRequiredService<ISender<ServerPipeline>>(),
                scoped.GetRequiredService<IPublisher<ClientPipeline>>(),
                scoped.GetRequiredService<IPublisher<ServerPipeline>>());
        }
    }
}
