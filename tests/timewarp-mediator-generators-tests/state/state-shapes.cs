#region Purpose
// Minimal TimeWarp.State-shaped IStore/IState used by the IncrementActionSet golden file.
#endregion

namespace TimeWarp.Mediator.Generators.Tests.State;

public interface IState
{
    Guid Guid { get; }

    ISender Sender { get; set; }

    IState Clone();
}

public interface IStore
{
    TState GetState<TState>() where TState : class, IState;

    IState GetState(Type stateType);

    void SetState(IState state);
}

public abstract class State<TState> : IState, ICloneable
    where TState : State<TState>
{
    public Guid Guid { get; protected set; } = Guid.NewGuid();

    public ISender Sender { get; set; } = null!;

    public object Clone()
    {
        TState clone = (TState)MemberwiseClone();
        clone.Guid = Guid.NewGuid();
        return clone;
    }

    IState IState.Clone()
    {
        return (IState)Clone();
    }
}

public sealed class InMemoryStore : IStore
{
    private readonly System.Collections.Generic.Dictionary<Type, IState> States = new();

    public TState GetState<TState>() where TState : class, IState
    {
        return (TState)GetState(typeof(TState));
    }

    public IState GetState(Type stateType)
    {
        return States[stateType];
    }

    public void SetState(IState state)
    {
        States[state.GetType()] = state;
    }
}

public static class RequestTypeExtensions
{
    public static Type GetEnclosingStateType(this Type requestType)
    {
        Type? current = requestType;
        while (current is not null)
        {
            if (typeof(IState).IsAssignableFrom(current) && current != typeof(IState))
            {
                return current;
            }

            current = current.DeclaringType;
        }

        throw new InvalidOperationException($"Request type '{requestType}' is not nested in an IState.");
    }
}
