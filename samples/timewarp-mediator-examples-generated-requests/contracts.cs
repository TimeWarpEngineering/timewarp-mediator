#region Purpose
// Contracts the sibling generator turns into ActionSets.
#endregion

namespace TimeWarp.Mediator.Examples.GeneratedRequests;

public sealed record Greet(string Name)
{
  public sealed record Response(string Text);

  public static Response Execute(Greet contract) => new("Hello, " + contract.Name);
}

public sealed record Add(int Left, int Right)
{
  public sealed record Response(int Sum);

  public static Response Execute(Add contract) => new(contract.Left + contract.Right);
}

[ContractActions(typeof(Greet), typeof(Add))]
public sealed partial class CalculatorState
{
}
