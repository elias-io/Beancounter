using System.Reflection;
using Beancounter.Extension;

namespace Beancounter.Helper;

/// <summary>
/// Provides utility methods for type discovery and reflection operations across all loaded assemblies.
/// </summary>
public static class ReflectionHelper
{
    /// <summary>
    /// Finds all classes that derive from the specified base type across all loaded assemblies.
    /// </summary>
    /// <typeparam name="T">The base type to search for derived classes.</typeparam>
    /// <returns>Collection of types that derive from T.</returns>
    public static IEnumerable<Type> FindDerivedClasses<T>()
    {
        var types = GetTypes();
        return types
            .Where(t => t is { IsClass: true }
                        && t.IsSubclassOf(typeof(T)));
    }

    /// <summary>
    /// Finds all classes that implement the specified interface across all loaded assemblies.
    /// </summary>
    /// <typeparam name="T">The interface type to search for implementing classes.</typeparam>
    /// <param name="assembly">Optional specific assembly to search. If null, searches all assemblies.</param>
    /// <returns>Collection of types that implement the interface T.</returns>
    public static IEnumerable<Type> FindImplementingClasses<T>(Assembly? assembly = null)
    {
        var types = GetTypes();
        return types
            .Where(t => t is { IsClass: true }
                        && t.GetInterfaces().Contains(typeof(T)));
    }

    /// <summary>
    /// Gets all types from all loaded assemblies in the current application domain.
    /// </summary>
    /// <returns>Array of all types from all assemblies.</returns>
    private static Type[] GetTypes() {
        HashSet<Type> types = [];
        AppDomain.CurrentDomain.GetAssemblies().ForEach(a => {
            types.AddRange(a.GetTypes());
        });
        return types.ToArray();
    }

}
