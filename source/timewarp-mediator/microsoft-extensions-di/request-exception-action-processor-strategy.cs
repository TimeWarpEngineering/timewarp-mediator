// Modified by Steven T. Cramer
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Controls when <see cref="TimeWarp.Mediator.Pipeline.IRequestExceptionAction{TRequest,TException}"/> instances run
/// relative to <see cref="TimeWarp.Mediator.Pipeline.IRequestExceptionHandler{TRequest,TResponse,TException}"/> instances.
/// </summary>
public enum RequestExceptionActionProcessorStrategy
{
    /// <summary>
    /// Run exception actions only for exceptions that no exception handler marked as handled.
    /// </summary>
    ApplyForUnhandledExceptions,

    /// <summary>
    /// Run exception actions for every exception, including ones an exception handler later marks as handled.
    /// </summary>
    ApplyForAllExceptions
}