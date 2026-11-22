#region Purpose
// Library, host, and sibling-generator sources for the referenced-assembly request pattern tests.
#endregion

#region Design
// The libraries mirror timewarp-architecture: contracts with a nested Response in one assembly
// (Web.Contracts), a OneOf stand-in in its own assembly, the derived [ContractState] attribute in
// another, and a TimeWarp.State stand-in that owns IAction and a behavior constrained on it.
// ShadowLibrary repeats contract and problem-details names as internal types in another assembly,
// which is what makes Compilation.GetTypeByMetadataName return null.
#endregion

namespace TimeWarp.Mediator.Generators.Compilation.Tests;

public sealed partial class GeneratedRequestPatternTests
{
  private const string OneOfLibrary = """
      namespace OneOf
      {
          public sealed class OneOf<T0, T1>
          {
              private OneOf(int index, object? value)
              {
                  Index = index;
                  Value = value;
              }

              public int Index { get; }

              public object? Value { get; }

              public static implicit operator OneOf<T0, T1>(T0 value) => new(0, value);

              public static implicit operator OneOf<T0, T1>(T1 value) => new(1, value);

              public override string ToString() => Index + ":" + Value;
          }
      }
      """;

  private const string WebContractsLibrary = """
      namespace Ns
      {
          public sealed class SharedProblemDetails
          {
              public string Title { get; init; } = "";

              public override string ToString() => Title;
          }

          public static class CreateThing
          {
              public sealed record Response(string Name);
          }

          public static class UpdateThing
          {
              public sealed record Response(string Name);
          }

          public static class NoResponse
          {
          }
      }
      """;

  private const string ShadowLibrary = """
      namespace Ns
      {
          internal sealed class SharedProblemDetails
          {
          }

          internal static class CreateThing
          {
              internal sealed class Response
              {
              }
          }
      }
      """;

  private const string ContractStateLibrary = """
      using System;
      using TimeWarp.Mediator;

      namespace State
      {
          public sealed class ContractStateAttribute : GeneratedRequestPatternAttribute
          {
              public const string RequestTypeName = "{Host}.{Contract}ActionSet.Action";
              public const string HandlerTypeName = "{Host}.{Contract}ActionSet.Handler";
              public const string RequestInterfaceName = "TimeWarp.Mediator.IRequest<OneOf.OneOf<{ContractType}.Response, Ns.SharedProblemDetails>>";

              public ContractStateAttribute(params Type[] contracts)
                  : base(contracts)
              {
              }
          }
      }
      """;

  private const string StateRuntimeLibrary = """
      using System.Collections.Generic;
      using System.Threading;
      using System.Threading.Tasks;
      using TimeWarp.Mediator;

      namespace TimeWarp.State
      {
          public interface IAction : IRequest
          {
          }

          public static class StateLog
          {
              public static readonly List<string> Entries = new();
          }

          public sealed class StateTransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
              where TRequest : notnull, IAction
          {
              public ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
              {
                  StateLog.Entries.Add("state:" + typeof(TRequest).Name);
                  return next(request, cancellationToken);
              }
          }
      }
      """;

  private const string ReferencedOneOfHost = """
      using System.Collections.Generic;
      using System.Linq;
      using System.Threading;
      using System.Threading.Tasks;
      using State;
      using TimeWarp.Mediator;

      [assembly: MediatorBehavior(typeof(App.Tag<,>))]

      namespace App
      {
          public static class Log
          {
              public static readonly List<string> Entries = new();
          }

          public sealed class Tag<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
              where TRequest : notnull, IRequest<TResponse>
          {
              public ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
              {
                  Log.Entries.Add(
                      typeof(TRequest).DeclaringType!.Name + ":" + typeof(TResponse).Name
                      + "<" + string.Join(",", typeof(TResponse).GenericTypeArguments.Select(argument => argument.FullName)) + ">");
                  return next(request, cancellationToken);
              }
          }

          [ContractState(typeof(Ns.CreateThing), typeof(Ns.UpdateThing))]
          public partial class ThingState { }
      }
      """;

  private const string ReferencedNoResponseHost = """
      using State;

      namespace App
      {
          [ContractState(typeof(Ns.NoResponse))]
          public partial class ThingState { }
      }
      """;

  private const string ThingActionSets = """
      using System.Threading;
      using System.Threading.Tasks;
      using OneOf;
      using TimeWarp.Mediator;

      namespace App
      {
          public partial class ThingState
          {
              public static class CreateThingActionSet
              {
                  public sealed class Action : IRequest<OneOf<Ns.CreateThing.Response, Ns.SharedProblemDetails>> { }

                  public sealed class Handler : IRequestHandler<Action, OneOf<Ns.CreateThing.Response, Ns.SharedProblemDetails>>
                  {
                      public ValueTask<OneOf<Ns.CreateThing.Response, Ns.SharedProblemDetails>> Handle(Action request, CancellationToken cancellationToken) =>
                          ValueTask.FromResult<OneOf<Ns.CreateThing.Response, Ns.SharedProblemDetails>>(new Ns.CreateThing.Response("widget"));
                  }
              }

              public static class UpdateThingActionSet
              {
                  public sealed class Action : IRequest<OneOf<Ns.UpdateThing.Response, Ns.SharedProblemDetails>> { }

                  public sealed class Handler : IRequestHandler<Action, OneOf<Ns.UpdateThing.Response, Ns.SharedProblemDetails>>
                  {
                      public ValueTask<OneOf<Ns.UpdateThing.Response, Ns.SharedProblemDetails>> Handle(Action request, CancellationToken cancellationToken) =>
                          ValueTask.FromResult<OneOf<Ns.UpdateThing.Response, Ns.SharedProblemDetails>>(new Ns.SharedProblemDetails { Title = "conflict" });
                  }
              }
          }
      }
      """;

  private const string ReferencedActionHost = """
      using System.Threading;
      using System.Threading.Tasks;
      using TimeWarp.Mediator;

      [assembly: MediatorBehavior(typeof(TimeWarp.State.StateTransactionBehavior<,>))]
      [assembly: MediatorBehavior(typeof(App.MediatorActionOnly<,>))]

      namespace App
      {
          // Constrained on TimeWarp.Mediator.IAction, which the declaration does not name.
          public sealed class MediatorActionOnly<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
              where TRequest : notnull, TimeWarp.Mediator.IAction
          {
              public ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
              {
                  TimeWarp.State.StateLog.Entries.Add("mediator-action:" + typeof(TRequest).Name);
                  return next(request, cancellationToken);
              }
          }

          [GeneratedRequestPattern(
              "{Host}.{Contract}ActionSet.Action",
              "{Host}.{Contract}ActionSet.Handler",
              "TimeWarp.State.IAction",
              typeof(Ns.CreateThing))]
          public partial class CounterState { }
      }
      """;

  private const string StateActionSets = """
      using System.Threading;
      using System.Threading.Tasks;
      using TimeWarp.Mediator;

      namespace App
      {
          public partial class CounterState
          {
              public static class CreateThingActionSet
              {
                  public sealed class Action : TimeWarp.State.IAction { }

                  public sealed class Handler : IRequestHandler<Action>
                  {
                      public ValueTask Handle(Action request, CancellationToken cancellationToken)
                      {
                          TimeWarp.State.StateLog.Entries.Add("handled:CreateThing");
                          return default;
                      }
                  }
              }
          }
      }
      """;
}
