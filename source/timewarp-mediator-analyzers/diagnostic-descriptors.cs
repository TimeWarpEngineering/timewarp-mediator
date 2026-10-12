#region Purpose
// TWM001–TWM006 descriptors shared by the analyzer-only package and the generator.
// TWM005/TWM006 are generator-only: they depend on the host's Aot/Link profile.
#endregion

namespace TimeWarp.Mediator.Analyzers;

internal static class DiagnosticDescriptors
{
    internal const string Category = "TimeWarp.Mediator";

    internal static readonly DiagnosticDescriptor RequestHasNoHandler = new(
        id: "TWM001",
        title: "Request has no handler",
        messageFormat: "Request '{0}' has no handler",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Every concrete IRequest/IAction/ICommand/IQuery in a member assembly must have exactly one handler.");

    internal static readonly DiagnosticDescriptor DuplicateHandler = new(
        id: "TWM002",
        title: "Duplicate handler for request",
        messageFormat: "Request '{0}' has multiple handlers: {1}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A request may have only one handler. Duplicate handlers are a link error.");

    internal static readonly DiagnosticDescriptor ScopeMismatch = new(
        id: "TWM003",
        title: "Handler and request belong to different pipelines",
        messageFormat: "Handler '{0}' is assigned to pipeline '{1}' but request '{2}' is assigned to pipeline '{3}'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A handler and its request must resolve to the same MediatorScope marker type (or both be unscoped).");

    internal static readonly DiagnosticDescriptor WrongScopeSend = new(
        id: "TWM004",
        title: "Request is not a member of this sender pipeline",
        messageFormat: "Request '{0}' belongs to pipeline '{1}' and cannot be sent through ISender<{2}>",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "ISender<TScope>.Send only dispatches requests assigned to that TScope marker. Use the matching scoped sender or Send(object) which throws NoHandlerException.");

    internal static readonly DiagnosticDescriptor AotBehaviorNotWoven = new(
        id: "TWM005",
        title: "Pipeline behavior is not woven under the Aot profile",
        messageFormat: "Behavior '{0}' is not woven under the '{1}' profile; Aot dispatch calls handlers directly and the manifest omits it",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The Aot/Link profile has no IServiceProvider and resolves handlers from static ServiceGen fields. [assembly: MediatorBehavior] registrations do not run in that profile. Use the Host profile for validation, transaction, or other cross-cutting behaviors.");

    internal static readonly DiagnosticDescriptor AotHandlerNotConstructible = new(
        id: "TWM006",
        title: "Aot handler is not constructible with new()",
        messageFormat: "Handler '{0}' cannot be created with 'new()' under the '{1}' profile",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The Aot/Link profile creates each handler once with 'new Handler()' into a static readonly ServiceGen field (a process-wide singleton). That call needs an accessible constructor with no required arguments. Required members must be set by that constructor (SetsRequiredMembers).");
}
