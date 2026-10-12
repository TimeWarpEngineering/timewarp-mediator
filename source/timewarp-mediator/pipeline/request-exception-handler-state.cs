// Modified by Steven T. Cramer
namespace TimeWarp.Mediator.Pipeline;

/// <summary>
/// Represents the result of handling an exception thrown by a request handler
/// </summary>
/// <typeparam name="TResponse">Response type</typeparam>
public class RequestExceptionHandlerState<TResponse>
{
    /// <summary>
    /// Indicates whether the current exception has been handled and the response should be returned.
    /// </summary>
    public bool Handled { get; private set; }

    /// <summary>
    /// The response that is returned if <see cref="Handled"/> is <c>true</c>.
    /// A <c>null</c> response is returned as-is; the exception is not rethrown.
    /// </summary>
    public TResponse? Response { get; private set; }

    /// <summary>
    /// Call to indicate whether the current exception should be considered handled and the specified response should be returned.
    /// </summary>
    /// <param name="response">Set the response that will be returned.</param>
    public void SetHandled(TResponse response)
    {
        Handled = true;
        Response = response;
    }
}