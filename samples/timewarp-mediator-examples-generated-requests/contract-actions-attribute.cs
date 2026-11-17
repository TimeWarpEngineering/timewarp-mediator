#region Purpose
// The one attribute both generators read: the sibling emits ActionSets from it, Mediator dispatches them.
#endregion

#region Design
// Derives from GeneratedRequestPatternAttribute. The templates are const fields so the Mediator
// generator reads them even when this attribute lives in a referenced assembly. Contracts are the
// params constructor parameter named contracts.
#endregion

namespace TimeWarp.Mediator.Examples.GeneratedRequests;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ContractActionsAttribute : GeneratedRequestPatternAttribute
{
  public const string RequestTypeName = "{Host}.{Contract}ActionSet.Action";
  public const string HandlerTypeName = "{Host}.{Contract}ActionSet.Handler";
  public const string RequestInterfaceName = "TimeWarp.Mediator.IRequest<{ContractType}.Response>";

  public ContractActionsAttribute(params Type[] contracts)
      : base(contracts)
  {
  }
}
