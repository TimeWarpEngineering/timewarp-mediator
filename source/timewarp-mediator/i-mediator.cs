// Modified by Steven T. Cramer

#region Purpose
// Mediator contract: sending requests and publishing notifications.
#endregion

namespace TimeWarp.Mediator;

/// <summary>
/// Defines a mediator to encapsulate request/response and publishing interaction patterns
/// </summary>
public interface IMediator : ISender, IPublisher
{
}