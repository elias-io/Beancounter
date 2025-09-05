namespace Beancounter.Extension;

/// <summary>
/// Provides extension methods for HashSet operations.
/// </summary>
public static class HashSet_Extensions {
    /// <summary>
    /// Adds multiple items to the HashSet.
    /// </summary>
    /// <typeparam name="T">The type of elements in the HashSet.</typeparam>
    /// <param name="hashSet">The HashSet to add items to.</param>
    /// <param name="range">The items to add.</param>
    /// <returns>The same HashSet instance (for method chaining).</returns>
    public static HashSet<T> AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> range) {
        range.ForEach(obj => hashSet.Add(obj));
        return hashSet;
    }
}