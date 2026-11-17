#region Purpose
// Host, contract, and sibling-generator sources for GeneratedRequestPatternTests.
#endregion

namespace TimeWarp.Mediator.Generators.Compilation.Tests;

public sealed partial class GeneratedRequestPatternTests
{
  private const string ContractsSource = """
      using System.Collections.Generic;

      namespace App
      {
          public static class Log
          {
              public static readonly List<string> Entries = new();
          }

          public sealed class CreateRole
          {
              public sealed class Response
              {
                  public string Value { get; init; } = "";

                  public override string ToString() => Value;
              }
          }

          public sealed class UpdateRole
          {
              public sealed class Response
              {
                  public string Value { get; init; } = "";

                  public override string ToString() => Value;
              }
          }
      }

      """;

  private const string BehaviorSource = """
      using System.Threading;
      using System.Threading.Tasks;
      using TimeWarp.Mediator;

      [assembly: MediatorBehavior(typeof(App.Tag<,>))]

      namespace App
      {
          public sealed class Tag<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
              where TRequest : notnull, IRequest<TResponse>
          {
              public ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
              {
                  Log.Entries.Add(typeof(TRequest).Name + ":" + typeof(TResponse).FullName);
                  return next(request, cancellationToken);
              }
          }
      }

      """;

  private const string PatternHost = """
      using TimeWarp.Mediator;

      namespace App
      {
          [GeneratedRequestPattern(
              "{Host}.{Contract}ActionSet.Action",
              "{Host}.{Contract}ActionSet.Handler",
              "TimeWarp.Mediator.IRequest<{ContractType}.Response>",
              typeof(CreateRole),
              typeof(UpdateRole))]
          public partial class RoleState { }
      }
      """;

  private const string RoleActionSets = """
      using System.Threading;
      using System.Threading.Tasks;
      using TimeWarp.Mediator;

      namespace App
      {
          public partial class RoleState
          {
              public static class CreateRoleActionSet
              {
                  public sealed class Action : IRequest<CreateRole.Response> { }

                  public sealed class Handler : IRequestHandler<Action, CreateRole.Response>
                  {
                      public ValueTask<CreateRole.Response> Handle(Action request, CancellationToken cancellationToken) =>
                          ValueTask.FromResult(new CreateRole.Response { Value = "created" });
                  }
              }

              public static class UpdateRoleActionSet
              {
                  public sealed class Action : IRequest<UpdateRole.Response> { }

                  public sealed class Handler : IRequestHandler<Action, UpdateRole.Response>
                  {
                      public ValueTask<UpdateRole.Response> Handle(Action request, CancellationToken cancellationToken) =>
                          ValueTask.FromResult(new UpdateRole.Response { Value = "updated" });
                  }
              }
          }
      }
      """;

  private const string StringActionSets = """
      using System.Threading;
      using System.Threading.Tasks;
      using TimeWarp.Mediator;

      namespace App
      {
          public partial class RoleState
          {
              public static class CreateRoleActionSet
              {
                  public sealed class Action : IRequest<string> { }

                  public sealed class Handler : IRequestHandler<Action, string>
                  {
                      public ValueTask<string> Handle(Action request, CancellationToken cancellationToken) =>
                          ValueTask.FromResult("created");
                  }
              }
          }
      }
      """;

  private const string NotAHandlerActionSets = """
      using System.Threading;
      using System.Threading.Tasks;
      using TimeWarp.Mediator;

      namespace App
      {
          public partial class RoleState
          {
              public static class CreateRoleActionSet
              {
                  public sealed class Action : IRequest<CreateRole.Response> { }

                  public sealed class Handler
                  {
                      public ValueTask<CreateRole.Response> Handle(Action request, CancellationToken cancellationToken) =>
                          ValueTask.FromResult(new CreateRole.Response());
                  }
              }
          }
      }
      """;

  private const string ScopedHost = """
      using System.Threading;
      using System.Threading.Tasks;
      using TimeWarp.Mediator;

      [assembly: MediatorBehavior(typeof(App.Before<,>), Scope = typeof(App.ClientPipeline))]

      namespace App
      {
          public sealed class ClientPipeline { }

          public sealed class Before<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
              where TRequest : notnull, IAction
          {
              public ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
              {
                  Log.Entries.Add("before:" + typeof(TRequest).Name);
                  return next(request, cancellationToken);
              }
          }

          [MediatorScope(typeof(ClientPipeline))]
          [GeneratedRequestPattern(
              "{Host}.{Contract}ActionSet.Action",
              "{Host}.{Contract}ActionSet.Handler",
              "TimeWarp.Mediator.IAction",
              typeof(CreateRole))]
          public partial class CounterState { }
      }
      """;

  private const string CounterActionSets = """
      using System.Threading;
      using System.Threading.Tasks;
      using TimeWarp.Mediator;

      namespace App
      {
          public partial class CounterState
          {
              public static class CreateRoleActionSet
              {
                  public sealed class Action : IAction { }

                  public sealed class Handler : IRequestHandler<Action>
                  {
                      public ValueTask Handle(Action request, CancellationToken cancellationToken)
                      {
                          Log.Entries.Add("handled:CreateRole");
                          return default;
                      }
                  }
              }
          }
      }
      """;

  private const string DerivedLibrary = """
      using System;
      using TimeWarp.Mediator;

      namespace State
      {
          public sealed class ContractStateAttribute : GeneratedRequestPatternAttribute
          {
              public const string RequestTypeName = "{Host}.{Contract}ActionSet.Action";
              public const string HandlerTypeName = "{Host}.{Contract}ActionSet.Handler";
              public const string RequestInterfaceName = "TimeWarp.Mediator.IRequest<{ContractType}.Response>";

              public ContractStateAttribute(params Type[] contracts)
                  : base(contracts)
              {
              }
          }
      }
      """;

  private const string DerivedHost = """
      using State;

      namespace App
      {
          [ContractState(typeof(CreateRole), typeof(UpdateRole))]
          public partial class RoleState { }
      }
      """;

  private const string DerivedSourceHost = """
      using System;
      using TimeWarp.Mediator;

      namespace App
      {
          public sealed class MyPatternAttribute : GeneratedRequestPatternAttribute
          {
              public MyPatternAttribute(
                  Type[] contracts,
                  string requestTypeName = "{Host}.{Contract}ActionSet.Action",
                  string handlerTypeName = "{Host}.{Contract}ActionSet.Handler",
                  string requestInterfaceName = "TimeWarp.Mediator.IRequest<{ContractType}.Response>")
                  : base(requestTypeName, handlerTypeName, requestInterfaceName, contracts)
              {
              }
          }

          [MyPattern(new[] { typeof(CreateRole) })]
          public partial class RoleState { }
      }
      """;

  private const string UnreadableHost = """
      using System;
      using TimeWarp.Mediator;

      namespace App
      {
          public sealed class BareAttribute : GeneratedRequestPatternAttribute
          {
              public BareAttribute(params Type[] contracts)
                  : base(contracts)
              {
              }
          }

          [Bare(typeof(CreateRole))]
          public partial class RoleState { }
      }
      """;
}
