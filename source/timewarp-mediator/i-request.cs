// Modified by Steven T. Cramer

#region Purpose
// Marker interfaces for void and response-returning requests.
#endregion

namespace TimeWarp.Mediator;

/// <summary>
/// Marker interface to represent a request with a void response.
/// </summary>
public interface IRequest { }

/// <summary>
/// Marker interface to represent a request with a response.
/// </summary>
/// <typeparam name="TResponse">Response type.</typeparam>
public interface IRequest<out TResponse> { }
