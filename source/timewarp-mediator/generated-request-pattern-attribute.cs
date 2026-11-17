#region Purpose
// Declares requests another generator emits, so this generator can put them in the Send switch.
#endregion

#region Design
// Roslyn generators cannot see each other's output. A request another generator emits into the
// host is invisible while the Mediator generator runs, so it gets no Send arm. A source type
// carries this attribute instead. The names are constant templates expanded once per contract,
// because Mediator cannot run the other generator's naming rule and a hand-written list would
// repeat the consumer's own attribute.
// Placeholders: {Host} is the attributed type's dotted full name (namespace and containing types),
// {Contract} is the contract's simple name, {ContractType} is the contract's dotted full name.
// Any other {...} is TWM013. Contracts are non-generic named types.
// requestInterfaceName names the Mediator request interface the emitted request implements, for
// example "TimeWarp.Mediator.IAction" or "TimeWarp.Mediator.IRequest<{ContractType}.Response>".
// The response is that interface's IRequest<T> argument. An interface that is only IRequest is a
// void request. The interface is the declaration, not just the response, so a behavior
// constrained "where TRequest : IAction" still closes over the declared request. The interface and
// its response must be visible to the Mediator generator: in source or in a reference.
// A derived attribute is read the same way. Each template comes from a constructor parameter of the
// same name (a default value is enough) or from a const string field on the attribute class named
// RequestTypeName, HandlerTypeName, or RequestInterfaceName. Contracts come from a constructor
// parameter named contracts. A const survives in a reference assembly; a getter body does not, so
// properties are not read.
// The analyzer checks every expansion after all generators have run (TWM013): the request and
// handler exist, the request implements the declared interface, and the handler implements that
// request's IRequestHandler.
#endregion

namespace TimeWarp.Mediator;

/// <summary>
/// Declares request types that another source generator emits into this compilation, so the
/// generated mediator dispatches them.
/// </summary>
/// <remarks>
/// <para>
/// Each template is expanded once per contract type. <c>{Host}</c> is the attributed type's full
/// name, <c>{Contract}</c> the contract's simple name, and <c>{ContractType}</c> the contract's full
/// name. Names are dotted type names; a nested type is <c>Outer.Inner</c>.
/// </para>
/// <code>
/// [GeneratedRequestPattern(
///   "{Host}.{Contract}ActionSet.Action",
///   "{Host}.{Contract}ActionSet.Handler",
///   "TimeWarp.Mediator.IRequest&lt;{ContractType}.Response&gt;",
///   typeof(CreateRole), typeof(UpdateRole))]
/// public partial class RoleState { }
/// </code>
/// <para>
/// A derived attribute passes the templates as constructor parameters with the same names or
/// declares them as <c>const string</c> fields named <c>RequestTypeName</c>,
/// <c>HandlerTypeName</c>, and <c>RequestInterfaceName</c>. The contract list is a constructor
/// parameter named <c>contracts</c>.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
public class GeneratedRequestPatternAttribute : Attribute
{
  /// <summary>
  /// Declares one request and one handler per contract type.
  /// </summary>
  /// <param name="requestTypeName">Request type name template, for example <c>{Host}.{Contract}ActionSet.Action</c>.</param>
  /// <param name="handlerTypeName">Handler type name template, for example <c>{Host}.{Contract}ActionSet.Handler</c>.</param>
  /// <param name="requestInterfaceName">
  /// Request interface the emitted request implements, for example <c>TimeWarp.Mediator.IAction</c>
  /// or <c>TimeWarp.Mediator.IRequest&lt;{ContractType}.Response&gt;</c>.
  /// </param>
  /// <param name="contracts">Contract types the templates are expanded for.</param>
  public GeneratedRequestPatternAttribute(
      string requestTypeName,
      string handlerTypeName,
      string requestInterfaceName,
      params Type[] contracts)
  {
    ArgumentNullException.ThrowIfNull(contracts);
    Contracts = contracts;
  }

  /// <summary>
  /// For a derived attribute that declares its templates as <c>const string</c> fields.
  /// </summary>
  /// <param name="contracts">Contract types the templates are expanded for.</param>
  protected GeneratedRequestPatternAttribute(params Type[] contracts)
  {
    ArgumentNullException.ThrowIfNull(contracts);
    Contracts = contracts;
  }

  /// <summary>
  /// Contract types the templates are expanded for.
  /// </summary>
  public IReadOnlyList<Type> Contracts { get; }
}
