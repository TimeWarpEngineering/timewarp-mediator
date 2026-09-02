using System;

namespace TimeWarp.Mediator;

/// <summary>
/// Factory method used to resolve services from the container.
/// </summary>
/// <param name="serviceType">Type of service to resolve</param>
/// <returns>An instance of the service</returns>
public delegate object ServiceFactory(Type serviceType);