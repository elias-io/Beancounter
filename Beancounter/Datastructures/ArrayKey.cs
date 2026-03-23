namespace Beancounter.Datastructures;

public sealed class ArrayKey<T> : IEquatable<ArrayKey<T>>
{
    private readonly T[] sorted;
    private readonly int hashCode;
    private readonly IEqualityComparer<T> comparer;

    public ArrayKey(T[] values, IEqualityComparer<T>? comparer = null, IComparer<T>? sortComparer = null)
    {
        ArgumentNullException.ThrowIfNull(values);
        this.comparer = comparer ?? EqualityComparer<T>.Default;

        // Sort a copy so element order doesn't matter
        sorted = new T[values.Length];
        Array.Copy(values, sorted, values.Length);
        Array.Sort(sorted, sortComparer ?? Comparer<T>.Default);

        // XOR-based hash: order-independent by nature,
        // but we've already sorted so a sequential hash also works.
        // Using sequential hash on the sorted array for better distribution.
        var hash = new HashCode();
        for (int i = 0; i < sorted.Length; i++)
            hash.Add(sorted[i], this.comparer);
        hashCode = hash.ToHashCode();
    }

    public ReadOnlySpan<T> Values => sorted;

    public bool Equals(ArrayKey<T>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (hashCode != other.hashCode) return false;
        if (sorted.Length != other.sorted.Length) return false;

        for (int i = 0; i < sorted.Length; i++)
        {
            if (!comparer.Equals(sorted[i], other.sorted[i]))
                return false;
        }

        return true;
    }

    public override bool Equals(object? obj) => Equals(obj as ArrayKey<T>);

    public override int GetHashCode() => hashCode;

    public override string ToString() => $"[{string.Join(", ", sorted)}]";

    public static bool operator ==(ArrayKey<T>? a, ArrayKey<T>? b) => Equals(a, b);
    public static bool operator !=(ArrayKey<T>? a, ArrayKey<T>? b) => !Equals(a, b);
}
