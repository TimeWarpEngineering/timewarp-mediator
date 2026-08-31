#region Purpose
// State-shaped IncrementActionSet: nested Action + Handler mutating Count through IStore.
#endregion

using System;
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;

namespace TimeWarp.Mediator.Generators.Tests.State;

public sealed partial class CounterState : State<CounterState>
{
    public int Count { get; set; }

    public static class IncrementActionSet
    {
        public sealed class Action : IAction
        {
            public Action(int amount)
            {
                Amount = amount;
            }

            public int Amount { get; }
        }

        public sealed class Handler : ActionHandler<Action>
        {
            private readonly IStore Store;

            public Handler(IStore store)
            {
                Store = store;
            }

            public override ValueTask Handle(Action request, CancellationToken cancellationToken)
            {
                CounterState counterState = Store.GetState<CounterState>();
                if (request.Amount < 0)
                {
                    throw new InvalidOperationException("Amount must be non-negative.");
                }

                counterState.Count += request.Amount;
                return default;
            }
        }
    }
}
