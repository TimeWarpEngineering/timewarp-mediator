#region Purpose
// Assembly marker so a consumer can detect the generated mediator without reading build properties.
#endregion

#region Design
// The generator emits [assembly: GeneratedMediator(typeof(Mediator))] (task 021-003). This type is
// the contract Nuru reads. The constructor stores the mediator type; it does not dispatch.
#endregion

namespace TimeWarp.Mediator;

/// <summary>
/// Marks an assembly that contains a generated mediator.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
public sealed class GeneratedMediatorAttribute : Attribute
{
  /// <summary>
  /// Initializes the attribute with the generated mediator type.
  /// </summary>
  /// <param name="mediatorType">Generated mediator type, for example <c>typeof(Mediator)</c>.</param>
  public GeneratedMediatorAttribute(Type mediatorType)
  {
    MediatorType = mediatorType ?? throw new ArgumentNullException(nameof(mediatorType));
  }

  /// <summary>
  /// Generated mediator type.
  /// </summary>
  public Type MediatorType { get; }
}
