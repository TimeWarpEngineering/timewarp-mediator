#region Purpose
// TWM001/TWM002 descriptors shared by the analyzer-only package and the generator.
#endregion

using Microsoft.CodeAnalysis;

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
}
