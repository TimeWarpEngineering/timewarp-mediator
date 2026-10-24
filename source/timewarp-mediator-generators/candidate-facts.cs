#region Purpose
// Equatable host and referenced-assembly facts the model step combines. No symbols or syntax nodes.
#endregion

#region Design
// A TypeFact is one discoverable declaration: the handler and notification matches it declares,
// whether it is a concrete request, and the scope ResolveScope would return.
// A BehaviorFact is one [assembly: MediatorBehavior]. Pipeline slots describe IPipelineBehavior<,>
// type arguments as either a type-parameter index or a concrete key, so the model step can close
// the behavior without constructing a symbol. Path and SpanStart preserve attribute order.
#endregion

namespace TimeWarp.Mediator.Generators;

internal sealed record HandlerMatch(
    TypeModel Message,
    TypeModel? Response,
    bool IsVoid,
    bool IsNotification) : IEquatable<HandlerMatch>;

internal sealed record TypeFact(
    TypeModel Type,
    EquatableArray<HandlerMatch> Matches,
    bool IsConcreteRequest,
    TypeModel? Scope) : IEquatable<TypeFact>;

internal sealed record TypeArgSlot(int ParameterIndex, string? ConcreteKey) : IEquatable<TypeArgSlot>;

internal sealed record PipelineSlot(TypeArgSlot Request, TypeArgSlot Response) : IEquatable<PipelineSlot>;

internal sealed record ConstraintSlot(string ConcreteKey, string DefinitionKey) : IEquatable<ConstraintSlot>;

internal sealed record ParameterSlot(
    bool ReferenceType,
    bool ValueType,
    bool Unmanaged,
    bool Constructor,
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
    EquatableArray<BehaviorFact> Behaviors) : IEquatable<HostInput>;

internal sealed record ReferencedInput(
    EquatableArray<TypeFact> Types,
    EquatableArray<BehaviorFact> Behaviors,
    EquatableArray<string> MemberAssemblyNames,
    EquatableArray<string> MissingRuntime) : IEquatable<ReferencedInput>;
