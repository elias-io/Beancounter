namespace Beancounter.Extension;

/// <summary>
/// Provides extension methods for IEnumerable operations.
/// </summary>
public static class IEnumerable_Extensions {
    /// <summary>
    /// Executes an action for each element in the collection.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to iterate over.</param>
    /// <param name="predicate">The action to execute for each element.</param>
    public static void ForEach<T>(this IEnumerable<T> collection, Action<T> predicate) {
        foreach (var item in collection) {
            predicate(item);
        }
    }

    /// <summary>
    /// Executes an async action for each element in the collection sequentially.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to iterate over.</param>
    /// <param name="predicate">The async action to execute for each element.</param>
    /// <returns>A Task that completes when all elements have been processed.</returns>
    public static async Task ForEachAsync<T>(this IEnumerable<T> collection, Func<T, Task> predicate) {
        foreach (var item in collection) {
            await predicate(item);
        }
    }

    /// <summary>
    /// Filters the collection to only include elements of the specified type.
    /// </summary>
    /// <typeparam name="T">The type to filter for.</typeparam>
    /// <param name="collection">The collection to filter.</param>
    /// <returns>Filtered collection containing only elements of type T.</returns>
    public static IEnumerable<T> OfType<T>(this IEnumerable<object> collection) where T : class {
        return collection
            .Where(item => item.GetType() == typeof(T))
            .Select(item => item as T)!;
    }
}