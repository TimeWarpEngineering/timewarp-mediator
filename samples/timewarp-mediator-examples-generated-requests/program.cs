#region Purpose
// Sends Actions a sibling generator emits; Mediator dispatches them through the declared pattern.
#endregion

using TimeWarp.Mediator.Examples.GeneratedRequests;

internal static class Program
{
  private static async Task<int> Main()
  {
    ServiceCollection services = new();
    services.AddGeneratedMediator();
    await using ServiceProvider provider = services.BuildServiceProvider();
    using IServiceScope scope = provider.CreateScope();
    ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();

    Greet.Response greeting = await sender.Send(new CalculatorState.GreetActionSet.Action(new Greet("Ada")));
    Add.Response sum = await sender.Send(new CalculatorState.AddActionSet.Action(new Add(2, 3)));

    string traced = string.Join(",", TraceBehavior<CalculatorState.GreetActionSet.Action, Greet.Response>.Seen)
        + "," + string.Join(",", TraceBehavior<CalculatorState.AddActionSet.Action, Add.Response>.Seen);
    if (greeting.Text != "Hello, Ada" || sum.Sum != 5 || traced != "GreetActionSet,AddActionSet")
    {
      Console.Error.WriteLine("unexpected: " + greeting.Text + " " + sum.Sum + " " + traced);
      return 1;
    }

    Console.WriteLine("greet:" + greeting.Text);
    Console.WriteLine("add:" + sum.Sum);
    return 0;
  }
}
