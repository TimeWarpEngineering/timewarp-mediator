// Modified by Steven T. Cramer
using System.Threading;
using System.Threading.Tasks;

namespace TimeWarp.Mediator;

/// <summary>
/// Publish a notification or event through the med pipeline to be handled by multiple handlers.
/// </summary>
public interface IPublisher
{
    /// <summary>
    /// Asynchronously send a notification to multiple handlers
    /// </summary>
    /// <param name="notification">Notification object</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A task that represents the publish operation.</returns>
    Task Publish(object notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously send a notification to multiple handlers
    /// </summary>
    /// <param name="notification">Notification object</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A task that represents the publish operation.</returns>
    Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification;
}

/// <summary>
/// Publish through the named pipeline identified by <typeparamref name="TScope"/>.
/// Unscoped <see cref="IPublisher"/> is the default pipeline (handlers with no
/// <see cref="MediatorScopeAttribute"/>).
/// </summary>
/// <typeparam name="TScope">Marker type that names the pipeline (for example <c>ClientPipeline</c>).</typeparam>
public interface IPublisher<TScope> : IPublisher
{
}