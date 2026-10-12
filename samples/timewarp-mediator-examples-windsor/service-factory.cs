// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Examples.Windsor;

/// <summary>
/// Windsor-side resolver delegate used by this sample. TimeWarp.Mediator itself resolves through
/// <see cref="IServiceProvider"/>; the library no longer ships a <c>ServiceFactory</c> delegate.
/// </summary>
internal delegate object ServiceFactory(Type serviceType);

/// <summary>
/// Adapts <see cref="ServiceFactory"/> to the <see cref="IServiceProvider"/> that
/// <see cref="Mediator"/> takes.
/// </summary>
internal sealed class ServiceFactoryServiceProvider(ServiceFactory serviceFactory) : IServiceProvider
{
  public object GetService(Type serviceType) => serviceFactory(serviceType);
}