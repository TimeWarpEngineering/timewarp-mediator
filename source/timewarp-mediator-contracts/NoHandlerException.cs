#region Purpose
// Runtime fallback when Send(object) receives a type the generated switch does not cover.
#endregion

using System;

namespace TimeWarp.Mediator;

/// <summary>
/// Thrown when the generated <c>Send(object)</c> switch does not contain a handler for the request type.
/// Missing handlers for statically typed requests are compile-time TWM001 errors.
/// </summary>
public sealed class NoHandlerException : InvalidOperationException
{
    /// <summary>
    /// Initializes the exception for a request type that has no generated dispatch arm.
    /// </summary>
    /// <param name="requestType">Runtime request type.</param>
    public NoHandlerException(Type requestType)
        : base($"No handler registered for request type '{requestType}'.")
    {
        RequestType = requestType ?? throw new ArgumentNullException(nameof(requestType));
    }

    /// <summary>
    /// Request type that was not in the generated switch.
    /// </summary>
    public Type RequestType { get; }
}
