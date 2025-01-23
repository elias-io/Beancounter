namespace Beancounter.Extension;

public static class HashSet_Extensions {
    public static HashSet<T> AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> range) {
        range.ForEach(obj => hashSet.Add(obj));
        return hashSet;
    }
}