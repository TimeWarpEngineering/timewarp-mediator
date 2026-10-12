#region Purpose
// ImmutableArray wrapper with element-wise equality, so incremental pipeline models compare by value.
#endregion

namespace TimeWarp.Mediator.Generators;

internal readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IEnumerable<T>
    where T : IEquatable<T>
{
  private readonly ImmutableArray<T> Items;

  public EquatableArray(ImmutableArray<T> items)
  {
    Items = items;
  }

  public static EquatableArray<T> Empty => new(ImmutableArray<T>.Empty);

  public int Length => Items.IsDefault ? 0 : Items.Length;

  public bool Equals(EquatableArray<T> other)
  {
    ImmutableArray<T> left = Items.IsDefault ? ImmutableArray<T>.Empty : Items;
    ImmutableArray<T> right = other.Items.IsDefault ? ImmutableArray<T>.Empty : other.Items;
    if (left.Length != right.Length)
    {
      return false;
    }

    for (int i = 0; i < left.Length; i++)
    {
      if (!EqualityComparer<T>.Default.Equals(left[i], right[i]))
      {
        return false;
      }
    }

    return true;
  }

  public override bool Equals(object? obj) => obj is EquatableArray<T> other && Equals(other);

  public override int GetHashCode()
  {
    unchecked
    {
      int hash = 17;
      foreach (T item in this)
      {
        hash = (hash * 31) + EqualityComparer<T>.Default.GetHashCode(item!);
      }

      return hash;
    }
  }

  public IEnumerator<T> GetEnumerator()
  {
    IEnumerable<T> items = Items.IsDefault ? ImmutableArray<T>.Empty : Items;
    return items.GetEnumerator();
  }

  System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}