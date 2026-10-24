#region Purpose
// Equatable host and referenced-assembly facts the model step combines. No symbols or syntax nodes.
#endregion

#region Design
// A TypeFact is one discoverable declaration: the handler and notification matches it declares,
// whether it is a concrete request, and the scope ResolveScope would return.
// HandlerMatch.MessageScope is that same ResolveScope for the message, so a notification or a
// non-member request still carries a scope when the message itself has no fact.
// A BehaviorFact is one [assembly: MediatorBehavior]. A pipeline slot is a type parameter, a
// concrete key, or a generic or array pattern, so Result<TRequest> closes without a symbol.
// Path is empty. SpanStart is the Assembly.GetAttributes index: source order for a host
// attribute, metadata order for a referenced assembly.
// HostIdentity is the compilation assembly name and public key. It is not an MSBuild property.
#endregion

namespace TimeWarp.Mediator.Generators;

internal sealed record HandlerMatch(
    TypeModel Message,
    TypeModel? Response,
    bool IsVoid,
    bool IsNotification,
    TypeModel? MessageScope) : IEquatable<HandlerMatch>;

internal sealed record TypeFact(
    TypeModel Type,
    EquatableArray<HandlerMatch> Matches,
    bool IsConcreteRequest,
    TypeModel? Scope) : IEquatable<TypeFact>;

// ParameterIndex >= 0 is a type parameter of the behavior. Otherwise the slot is a concrete key,
// a generic pattern (GenericDefinitionKey plus Arguments), or an array (ArrayRank plus one argument).
internal sealed record TypeArgSlot(
    int ParameterIndex,
    string? ConcreteKey,
    string? GenericDefinitionKey,
    int ArrayRank,
    EquatableArray<TypeArgSlot> Arguments) : IEquatable<TypeArgSlot>;

internal sealed record PipelineSlot(TypeArgSlot Request, TypeArgSlot Response) : IEquatable<PipelineSlot>;

internal sealed record ConstraintSlot(string ConcreteKey, string DefinitionKey) : IEquatable<ConstraintSlot>;

internal sealed record ParameterSlot(
    bool ReferenceType,
    bool ValueType,
    bool Unmanaged,
    bool Constructor,
    bool HasTypeParameterConstraint,
    EquatableArray<ConstraintSlot> Constraints) : IEquatable<ParameterSlot>;

internal sealed record BehaviorFact(
    string AssemblyName,
    string Path,
    int SpanStart,
    int Order,
    TypeModel Definition,
    string FullyQualifiedPrefix,
    string DisplayPrefix,
    string ErrorDisplayPrefix,
    EquatableArray<ParameterSlot> Parameters,
    EquatableArray<PipelineSlot> Pipelines,
    TypeModel? Scope,
    LocationInfo? Location) : IEquatable<BehaviorFact>;

internal sealed record HostInput(
    EquatableArray<TypeFact> Types,
    EquatableArray<BehaviorFact> Behaviors,
    EquatableArray<string> DeclaredRuntime) : IEquatable<HostInput>;

internal sealed record ReferencedInput(
    EquatableArray<TypeFact> Types,
    EquatableArray<BehaviorFact> Behaviors,
    EquatableArray<string> MemberAssemblyNames,
    EquatableArray<string> MissingRuntime,
    string HostAssemblyName) : IEquatable<ReferencedInput>;

internal sealed record HostIdentity(string AssemblyName, EquatableArray<byte> PublicKey) : IEquatable<HostIdentity>
{
  internal static HostIdentity From(Compilation compilation)
  {
    string name = compilation.AssemblyName ?? string.Empty;
    ImmutableArray<byte> key = compilation.Assembly.Identity.PublicKey;
    if (key.IsDefault)
    {
      key = ImmutableArray<byte>.Empty;
    }

    return new HostIdentity(name, new EquatableArray<byte>(key));
  }
}
