using System.Reflection;

namespace Beancounter.Helper;

public static class ReflectionHelper
{
    
    public static IEnumerable<Type> FindDerivedClasses<T>()
    {
        var baseType = typeof(T);
        var assembly = Assembly.GetAssembly(baseType);
        if (assembly == null) return [];
        var types = assembly.GetTypes();
        return types
            .Where(t => t is { IsClass: true } && t.IsSubclassOf(baseType));
    }

    public static IEnumerable<Type> FindImplementingClasses<T>()
    {
        var baseType = typeof(T);
        var assembly = Assembly.GetAssembly(baseType);
        if (assembly == null) return [];
        var types = assembly.GetTypes();
        return types
            .Where(t => t is { IsClass: true } && t.GetInterfaces().Contains(baseType));
    }

}
