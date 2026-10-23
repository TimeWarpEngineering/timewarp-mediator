#region Purpose
// TWM001–TWM011 descriptors shared by the analyzer-only package and the generator.
// TWM005–TWM011 are generator-only: they describe what the host's generated mediator emits.
#endregion

#region Design
// TWM001–TWM003 are reported from a compilation-end action in RequestHandlerAnalyzer, so they
// carry WellKnownDiagnosticTags.CompilationEnd. Every descriptor links to its section in
// documentation/diagnostics.md.
#endregion

namespace TimeWarp.Mediator.Analyzers;

internal static class DiagnosticDescriptors
{
  internal const string Category = "TimeWarp.Mediator";

  private const string HelpLinkBase =
      "https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/diagnostics.md#";

  internal static readonly DiagnosticDescriptor RequestHasNoHandler = new(
      id: "TWM001",
      title: "Request has no handler",
      messageFormat: "Request '{0}' has no handler",
      category: Category,
      defaultSeverity: DiagnosticSeverity.Error,
      isEnabledByDefault: true,
      description: "Every concrete IRequest/IAction/ICommand/IQuery in a member assembly must have exactly one handler.",
      helpLinkUri: HelpLink("TWM001"),
      customTags: WellKnownDiagnosticTags.CompilationEnd);

  internal static readonly DiagnosticDescriptor DuplicateHandler = new(
      id: "TWM002",
      title: "Duplicate handler for request",
      messageFormat: "Request '{0}' has multiple handlers: {1}",
      category: Category,
      defaultSeverity: DiagnosticSeverity.Error,
      isEnabledByDefault: true,
      description: "A request may have only one handler. Duplicate handlers are a link error.",
      helpLinkUri: HelpLink("TWM002"),
      customTags: WellKnownDiagnosticTags.CompilationEnd);

  internal static readonly DiagnosticDescriptor ScopeMismatch = new(
      id: "TWM003",
      title: "Handler and message belong to different pipelines",
      messageFormat: "Handler '{0}' is assigned to pipeline '{1}' but message '{2}' is assigned to pipeline '{3}'",
      category: Category,
      defaultSeverity: DiagnosticSeverity.Error,
      isEnabledByDefault: true,
      description: "When a handler and its request or notification both resolve a MediatorScope marker type, the markers must match. If only one side is scoped, the pair joins that pipeline; if neither is, it joins the unscoped default pipeline.",
      helpLinkUri: HelpLink("TWM003"),
      customTags: WellKnownDiagnosticTags.CompilationEnd);

  internal static readonly DiagnosticDescriptor WrongScopeSend = new(
      id: "TWM004",
      title: "Request is not a member of this sender pipeline",
      messageFormat: "Request '{0}' belongs to pipeline '{1}' and cannot be sent through '{2}'",
      category: Category,
      defaultSeverity: DiagnosticSeverity.Error,
      isEnabledByDefault: true,
      description: "ISender<TScope>.Send only dispatches requests assigned to that TScope marker, and in a generator host the unscoped ISender/IMediator only dispatches unscoped requests. Use the matching sender, or Send(object), which throws NoHandlerException.",
      helpLinkUri: HelpLink("TWM004"));

  internal static readonly DiagnosticDescriptor AotBehaviorNotWoven = new(
      id: "TWM005",
      title: "Pipeline behavior is not woven under the Aot profile",
      messageFormat: "Behavior '{0}' is not woven under the '{1}' profile; Aot dispatch calls handlers directly and the manifest omits it",
      category: Category,
      defaultSeverity: DiagnosticSeverity.Warning,
      isEnabledByDefault: true,
      description: "The Aot/Link profile has no IServiceProvider and resolves handlers from static ServiceGen fields. [assembly: MediatorBehavior] registrations do not run in that profile. Use the Host profile for validation, transaction, or other cross-cutting behaviors.",
      helpLinkUri: HelpLink("TWM005"));

  internal static readonly DiagnosticDescriptor AotHandlerNotConstructible = new(
      id: "TWM006",
      title: "Aot handler is not constructible with new()",
      messageFormat: "Handler '{0}' cannot be created with 'new()' under the '{1}' profile",
      category: Category,
      defaultSeverity: DiagnosticSeverity.Error,
      isEnabledByDefault: true,
      description: "The Aot/Link profile creates each handler once with 'new Handler()' into a static readonly ServiceGen field (a process-wide singleton). That call needs an accessible constructor with no required arguments. Required members must be set by that constructor (SetsRequiredMembers).",
      helpLinkUri: HelpLink("TWM006"));

  internal static readonly DiagnosticDescriptor InaccessibleMemberType = new(
      id: "TWM007",
      title: "Member assembly type is not accessible from the host",
      messageFormat: "'{0}' in member assembly '{1}' is not accessible from '{2}' and is left out of the generated mediator; make it public or add [assembly: InternalsVisibleTo(\"{2}\")]",
      category: Category,
      defaultSeverity: DiagnosticSeverity.Error,
      isEnabledByDefault: true,
      description: "The generated mediator names every handler, request, response, notification, behavior, and pipeline scope marker directly. A type the host cannot see (internal without InternalsVisibleTo) would be CS0122 in generated code, so it is not bound.",
      helpLinkUri: HelpLink("TWM007"));

  internal static readonly DiagnosticDescriptor MediatorRuntimeMissing = new(
      id: "TWM008",
      title: "Generated mediator dependencies are not referenced",
      messageFormat: "The generated mediator needs '{0}', which this compilation cannot resolve; nothing is generated. {1}.",
      category: Category,
      defaultSeverity: DiagnosticSeverity.Error,
      isEnabledByDefault: true,
      description: "The generator emits code against TimeWarp.Mediator.Contracts and, under the Host profile, Microsoft.Extensions.DependencyInjection.Abstractions. When either is missing it emits nothing and reports this once, instead of compile errors inside generated files.",
      helpLinkUri: HelpLink("TWM008"));

  internal static readonly DiagnosticDescriptor UnsupportedHandlerKind = new(
      id: "TWM009",
      title: "Handler kind is not dispatched by the generated mediator",
      messageFormat: "'{0}' implements '{1}', which the generated mediator does not dispatch; it is ignored",
      category: Category,
      defaultSeverity: DiagnosticSeverity.Warning,
      isEnabledByDefault: true,
      description: "The generated mediator dispatches IRequestHandler, INotificationHandler, and [assembly: MediatorBehavior] pipeline behaviors. Pre/post processors and exception handlers/actions in member assemblies never run.",
      helpLinkUri: HelpLink("TWM009"));

  internal static readonly DiagnosticDescriptor InvalidBehaviorRegistration = new(
      id: "TWM010",
      title: "Registered behavior is not a pipeline behavior",
      messageFormat: "'{0}' is registered with [MediatorBehavior] but does not implement IPipelineBehavior<TRequest, TResponse>; it is ignored",
      category: Category,
      defaultSeverity: DiagnosticSeverity.Warning,
      isEnabledByDefault: true,
      description: "[assembly: MediatorBehavior(typeof(T))] must name a type that implements IPipelineBehavior<TRequest, TResponse> (open generic or closed). Other types are dropped from every pipeline.",
      helpLinkUri: HelpLink("TWM010"));

  internal static readonly DiagnosticDescriptor BehaviorAppliesToNoRequest = new(
      id: "TWM011",
      title: "Registered behavior applies to no request",
      messageFormat: "Behavior '{0}' does not close over any request in pipeline '{1}' and never runs",
      category: Category,
      defaultSeverity: DiagnosticSeverity.Warning,
      isEnabledByDefault: true,
      description: "A [MediatorBehavior] registration weaves only into requests in its pipeline (Scope, or the unscoped default) whose request and response satisfy the behavior's generic constraints. When no request qualifies, the registration has no effect.",
      helpLinkUri: HelpLink("TWM011"));

  private static string HelpLink(string id) => HelpLinkBase + id.ToLowerInvariant();
}