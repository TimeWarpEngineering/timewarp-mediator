// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Examples.Windsor;

public class ContravariantFilter : IHandlersFilter
{
    public bool HasOpinionAbout(Type service)
    {
        if (!service.IsGenericType)
            return false;

        Type genericType = service.GetGenericTypeDefinition();
        Type[] genericArguments = genericType.GetGenericArguments();
        return genericArguments.Count() == 1
               && genericArguments.Single().GenericParameterAttributes.HasFlag(GenericParameterAttributes.Contravariant);
    }

    public IHandler[] SelectHandlers(Type service, IHandler[] handlers)
    {
        return handlers;
    }
}
