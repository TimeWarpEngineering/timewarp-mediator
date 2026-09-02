// Modified by Steven T. Cramer
namespace TimeWarp.Mediator;

/// <summary>
/// Marker interface to represent a request with a streaming response
/// </summary>
/// <typeparam name="TResponse">Response type</typeparam>
public interface IStreamRequest<out TResponse> { }
