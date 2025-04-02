namespace Beancounter.Extension;

public static class IEnumerable_Extensions {
    public static void ForEach<T>(this IEnumerable<T> collection, Action<T> predicate) {
        foreach (var item in collection) {
            predicate(item);
        }
    }

    public static async Task ForEachAsync<T>(this IEnumerable<T> collection, Func<T, Task> predicate) {
        foreach (var item in collection) {
            await predicate(item);
        }
    }

    public static IEnumerable<T> OfType<T>(this IEnumerable<object> collection) where T : class {
        return collection
            .Where(item => item.GetType() == typeof(T))
            .Select(item => item as T)!;
    }
}