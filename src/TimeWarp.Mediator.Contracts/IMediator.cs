// Modified by Steven T. Cramer
namespace TimeWarp.Mediator;

/// <summary>
/// Defines a med to encapsulate request/response and publishing interaction patterns
/// </summary>
public interface IMediator : ISender, IPublisher
{
}