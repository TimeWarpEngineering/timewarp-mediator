#region Purpose
// Declares the handler type another generator emits, so this generator can dispatch to it.
#endregion

#region Design
// Roslyn generators cannot see each other's output. A handler emitted by TimeWarp.State is
// invisible while this generator runs, so an interface search never binds it. The request
// carries the handler's simple name instead. HandlerTypeName is relative to the request's
// containing type (or to its namespace when the request is top-level). Derived attributes are
// found by walking the attribute's base types. Generated code may name that handler because
// every generator's output compiles together.
// An attribute declared in another assembly passes the name as a constructor parameter named
// handlerTypeName. A default value is enough. A reference assembly does not keep the getter body,
// so a getter-only attribute in a ProjectReference is TWM012.
#endregion

namespace TimeWarp.Mediator;

/// <summary>
/// Names the handler a request dispatches to when that handler is emitted by another generator.
/// </summary>
/// <remarks>
/// Apply a derived attribute to the request. <see cref="HandlerTypeName"/> is a constant type
/// name relative to the request's containing type. <c>Handler</c> on a request nested in
/// <c>CounterState</c> names <c>CounterState.Handler</c>. A top-level request resolves the name
/// in its own namespace. In this compilation a getter that returns a string literal is enough.
/// An attribute declared in another assembly takes a constructor parameter named
/// <c>handlerTypeName</c>. A default value is enough:
/// <c>public sealed class ReducerAttribute(string handlerTypeName = "Handler")</c>.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public abstract class GeneratedHandlerAttribute : Attribute
{
  /// <summary>
  /// Handler type name relative to the request's containing type.
  /// </summary>
  public abstract string HandlerTypeName { get; }
}
