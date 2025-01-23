using System.Reflection;
using Beancounter.Extension;

namespace Beancounter.Helper;

public static class ReflectionHelper
{
    
    public static IEnumerable<Type> FindDerivedClasses<T>()
    {
        var types = GetTypes();
        return types
            .Where(t => t is { IsClass: true }
                        && t.IsSubclassOf(typeof(T)));
    }

    public static IEnumerable<Type> FindImplementingClasses<T>(Assembly? assembly = null)
    {
        var types = GetTypes();
        return types
            .Where(t => t is { IsClass: true }
                        && t.GetInterfaces().Contains(typeof(T)));
    }

    private static Type[] GetTypes() {
        HashSet<Type> types = [];
        AppDomain.CurrentDomain.GetAssemblies().ForEach(a => {
            types.AddRange(a.GetTypes());
        });
        return types.ToArray();
    }

}
