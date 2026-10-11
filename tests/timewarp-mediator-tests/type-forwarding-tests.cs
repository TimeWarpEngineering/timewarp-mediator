// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Tests;

public class TypeForwardingTests
{
    [Fact]
    public void Should_forward_every_public_contract_type()
    {
        Assembly contractsAssembly = typeof(IRequest).Assembly;
        Assembly mediatorAssembly = typeof(Mediator).Assembly;

        HashSet<Type> forwardedTypes = new(mediatorAssembly.GetForwardedTypes());

        List<string> missingForwards = contractsAssembly.GetExportedTypes()
            .Where(static type => !type.IsNested)
            .Where(type => !forwardedTypes.Contains(type))
            .Select(static type => type.FullName!)
            .OrderBy(static name => name, StringComparer.Ordinal)
            .ToList();

        missingForwards.ShouldBeEmpty();
    }
}
