// Modified by Steven T. Cramer
using Microsoft.Extensions.DependencyInjection;
using TimeWarp.Mediator.Pipeline;
using System.Threading;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace TimeWarp.Mediator.Extensions.Microsoft.DependencyInjection.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void GetPipelineInfo_Should_Return_Empty_Lists_When_No_Components_Registered()
    {
        var services = new ServiceCollection();
        
        var pipelineInfo = services.GetPipelineInfo();
        
        pipelineInfo.ShouldContain("TimeWarp Mediator Pipeline Registrations:");
        pipelineInfo.ShouldContain("Preprocessors:");
        pipelineInfo.ShouldContain("(none)");
        pipelineInfo.ShouldContain("Behaviors:");
        pipelineInfo.ShouldContain("Postprocessors:");
    }

    [Fact]
    public void GetPipelineInfo_Should_List_Registered_Behaviors()
    {
        var services = new ServiceCollection();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TestBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AnotherTestBehavior<,>));
        
        var pipelineInfo = services.GetPipelineInfo();
        
        pipelineInfo.ShouldContain("Behaviors:");
        pipelineInfo.ShouldContain("1. TestBehavior");
        pipelineInfo.ShouldContain("2. AnotherTestBehavior");
    }

    [Fact]
    public void GetPipelineInfo_Should_List_Registered_PreProcessors()
    {
        var services = new ServiceCollection();
        services.AddTransient(typeof(IRequestPreProcessor<>), typeof(TestPreProcessor<>));
        
        var pipelineInfo = services.GetPipelineInfo();
        
        pipelineInfo.ShouldContain("Preprocessors:");
        pipelineInfo.ShouldContain("1. TestPreProcessor");
    }

    [Fact]
    public void GetPipelineInfo_Should_List_Registered_PostProcessors()
    {
        var services = new ServiceCollection();
        services.AddTransient(typeof(IRequestPostProcessor<,>), typeof(TestPostProcessor<,>));
        
        var pipelineInfo = services.GetPipelineInfo();
        
        pipelineInfo.ShouldContain("Postprocessors:");
        pipelineInfo.ShouldContain("1. TestPostProcessor");
    }

    [Fact]
    public void GetPipelineInfo_Should_List_All_Component_Types()
    {
        var services = new ServiceCollection();
        services.AddTransient(typeof(IRequestPreProcessor<>), typeof(TestPreProcessor<>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TestBehavior<,>));
        services.AddTransient(typeof(IRequestPostProcessor<,>), typeof(TestPostProcessor<,>));
        
        var pipelineInfo = services.GetPipelineInfo();
        
        pipelineInfo.ShouldContain("Preprocessors:");
        pipelineInfo.ShouldContain("1. TestPreProcessor");
        pipelineInfo.ShouldContain("Behaviors:");
        pipelineInfo.ShouldContain("1. TestBehavior");
        pipelineInfo.ShouldContain("Postprocessors:");
        pipelineInfo.ShouldContain("1. TestPostProcessor");
    }

    private class TestBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            return next();
        }
    }

    private class AnotherTestBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            return next();
        }
    }

    private class TestPreProcessor<TRequest> : IRequestPreProcessor<TRequest>
        where TRequest : notnull
    {
        public Task Process(TRequest request, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private class TestPostProcessor<TRequest, TResponse> : IRequestPostProcessor<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public Task Process(TRequest request, TResponse response, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}