#region Purpose
// Response shapes from #68: explicit ICommand<Unit> + IRequestHandler<T, Unit>, void ICommand, and
// non-named (array) plus generic collection query responses, with a closed behavior over string[].
#endregion

namespace TimeWarp.Mediator.Generators.Tests.Responses;

public sealed class CreateWidget : ICommand<Unit>
{
    public string Name { get; set; } = string.Empty;
}

public sealed class CreateWidgetHandler : IRequestHandler<CreateWidget, Unit>
{
    public static List<string> Created { get; } = new();

    public Task<Unit> Handle(CreateWidget request, CancellationToken cancellationToken)
    {
        Created.Add(request.Name);
        PipelineLog.Events.Add("handler");
        return Task.FromResult(Unit.Value);
    }
}

public sealed class ArchiveWidget : ICommand
{
    public string Name { get; set; } = string.Empty;
}

public sealed class ArchiveWidgetHandler : IRequestHandler<ArchiveWidget>
{
    public static List<string> Archived { get; } = new();

    public Task Handle(ArchiveWidget request, CancellationToken cancellationToken)
    {
        Archived.Add(request.Name);
        return Task.CompletedTask;
    }
}

public sealed class SearchWidgets : IQuery<string[]>
{
    public string Prefix { get; set; } = string.Empty;
}

public sealed class SearchWidgetsHandler : IRequestHandler<SearchWidgets, string[]>
{
    public Task<string[]> Handle(SearchWidgets request, CancellationToken cancellationToken)
    {
        PipelineLog.Events.Add("handler");
        return Task.FromResult(new[] { request.Prefix + "-1", request.Prefix + "-2" });
    }
}

public sealed class ListWidgets : IQuery<List<string>>
{
    public int Count { get; set; }
}

public sealed class ListWidgetsHandler : IQueryHandler<ListWidgets, List<string>>
{
    public Task<List<string>> Handle(ListWidgets request, CancellationToken cancellationToken)
    {
        List<string> widgets = Enumerable.Range(1, request.Count).Select(i => "widget-" + i).ToList();
        return Task.FromResult(widgets);
    }
}

public sealed class SearchWidgetsAuditBehavior : IPipelineBehavior<SearchWidgets, string[]>
{
    public async Task<string[]> Handle(
        SearchWidgets request,
        RequestHandlerDelegate<string[]> next,
        CancellationToken cancellationToken)
    {
        PipelineLog.Events.Add("array-before");
        string[] response = await next(cancellationToken).ConfigureAwait(false);
        PipelineLog.Events.Add("array-after");
        return response;
    }
}
