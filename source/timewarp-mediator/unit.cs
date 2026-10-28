// Modified by Steven T. Cramer

#region Purpose
// Void stand-in for a handler that returns no value.
#endregion

#region Design
// ValueTask is an expression-bodied getter with no backing field. A static
// auto-property would emit a field of type ValueTask<Unit> on Unit. ValueTask<T>
// stores a T, so that field is a recursive type. CoreCLR loads it. The Mono
// WebAssembly loader throws TypeLoadException. A completed ValueTask<T> is the
// value itself, so the getter does not allocate.
// Unit.ValueTask shadows System.Threading.Tasks.ValueTask under using static Unit.
// Call sites write Unit.ValueTask. A signature still binds to the framework type.
#endregion

namespace TimeWarp.Mediator;

/// <summary>
/// Represents a void type, since <see cref="System.Void"/> is not a valid return type in C#.
/// </summary>
public readonly struct Unit : IEquatable<Unit>
{
  private static readonly Unit Instance = new();

  /// <summary>
  /// Default and only value of the <see cref="Unit"/> type.
  /// </summary>
  public static ref readonly Unit Value => ref Instance;

  /// <summary>
  /// Completed <see cref="System.Threading.Tasks.ValueTask{T}"/> of <see cref="Value"/>.
  /// The getter has no backing field. A static field of type <see cref="System.Threading.Tasks.ValueTask{T}"/>
  /// of <see cref="Unit"/> on <see cref="Unit"/> is a recursive type, and the Mono
  /// WebAssembly loader rejects it.
  /// Under <c>using static TimeWarp.Mediator.Unit</c> this name does not hide
  /// <see cref="System.Threading.Tasks.Task"/> (the removed <c>Unit.Task</c> property did).
  /// It does shadow <see cref="System.Threading.Tasks.ValueTask"/>.
  /// Prefer the qualified names <c>Unit.Value</c> and <c>Unit.ValueTask</c>.
  /// </summary>
  public static System.Threading.Tasks.ValueTask<Unit> ValueTask => new(Instance);

  /// <summary>
  /// Returns a hash code for this instance.
  /// </summary>
  /// <returns>
  /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
  /// </returns>
  public override int GetHashCode() => 0;

  /// <summary>
  /// Determines whether the current object is equal to another object of the same type.
  /// </summary>
  /// <param name="other">An object to compare with this object.</param>
  /// <returns>
  /// <c>true</c> if the current object is equal to the <paramref name="other"/> parameter; otherwise, <c>false</c>.
  /// </returns>
  public bool Equals(Unit other) => true;

  /// <summary>
  /// Determines whether the specified <see cref="object"/> is equal to this instance.
  /// </summary>
  /// <param name="obj">The object to compare with the current instance.</param>
  /// <returns>
  /// <c>true</c> if the specified <see cref="object"/> is equal to this instance; otherwise, <c>false</c>.
  /// </returns>
  public override bool Equals(object? obj) => obj is Unit;

  /// <summary>
  /// Determines whether the <paramref name="first"/> object is equal to the <paramref name="second"/> object.
  /// </summary>
  /// <param name="first">The first object.</param>
  /// <param name="second">The second object.</param>
  /// <returns><c>true</c> if the <paramref name="first"/> object is equal to the <paramref name="second"/> object; otherwise, <c>false</c>.</returns>
  public static bool operator ==(Unit first, Unit second) => true;

  /// <summary>
  /// Determines whether the <paramref name="first"/> object is not equal to the <paramref name="second"/> object.
  /// </summary>
  /// <param name="first">The first object.</param>
  /// <param name="second">The second object.</param>
  /// <returns><c>true</c> if the <paramref name="first"/> object is not equal to the <paramref name="second"/> object; otherwise, <c>false</c>.</returns>
  public static bool operator !=(Unit first, Unit second) => false;

  /// <summary>
  /// Returns a <see cref="string"/> that represents this instance.
  /// </summary>
  /// <returns>A <see cref="string"/> that represents this instance.</returns>
  public override string ToString() => "()";
}
