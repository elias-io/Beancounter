# Helper Classes API Reference

This document provides detailed API reference for the helper classes in the Beancounter library.

## Table of Contents

- [ReflectionHelper](#reflectionhelper)

## ReflectionHelper

The `ReflectionHelper` class provides utility methods for type discovery and reflection operations across all loaded assemblies.

### Methods

#### `FindDerivedClasses<T>()`

Finds all classes that derive from the specified base type across all loaded assemblies.

**Returns:** `IEnumerable<Type>` - Collection of types that derive from T

**Example:**
```csharp
// Find all classes that derive from MyBaseClass
var derivedTypes = ReflectionHelper.FindDerivedClasses<MyBaseClass>();

foreach (var type in derivedTypes)
{
    Console.WriteLine($"Found derived class: {type.Name}");
}
```

**Behavior:**
- Searches through all assemblies in the current application domain
- Returns only classes (not interfaces or value types)
- Uses `IsSubclassOf()` for inheritance checking
- Returns types that directly or indirectly inherit from the specified base type

**Use Cases:**
- Plugin discovery
- Factory pattern implementations
- Dependency injection container setup
- Command pattern implementations

#### `FindImplementingClasses<T>(Assembly? assembly = null)`

Finds all classes that implement the specified interface across all loaded assemblies.

**Parameters:**
- `assembly` (`Assembly?`): Optional specific assembly to search. If null, searches all assemblies.

**Returns:** `IEnumerable<Type>` - Collection of types that implement the interface T

**Example:**
```csharp
// Find all classes that implement IMyInterface
var implementingTypes = ReflectionHelper.FindImplementingClasses<IMyInterface>();

foreach (var type in implementingTypes)
{
    Console.WriteLine($"Found implementing class: {type.Name}");
}

// Search in a specific assembly
var specificAssembly = Assembly.GetExecutingAssembly();
var typesInAssembly = ReflectionHelper.FindImplementingClasses<IMyInterface>(specificAssembly);
```

**Behavior:**
- Searches through all assemblies in the current application domain (or specified assembly)
- Returns only classes (not interfaces or value types)
- Uses `GetInterfaces().Contains()` for interface checking
- Returns types that implement the specified interface

**Use Cases:**
- Service discovery
- Strategy pattern implementations
- Event handler discovery
- Repository pattern implementations

### Private Methods

#### `GetTypes()`

Gets all types from all loaded assemblies in the current application domain.

**Returns:** `Type[]` - Array of all types from all assemblies

**Behavior:**
- Iterates through all assemblies in `AppDomain.CurrentDomain.GetAssemblies()`
- Collects all types from each assembly
- Returns a flattened array of all types
- Uses `HashSet<Type>` to avoid duplicates

### Examples

#### Plugin Discovery

```csharp
public interface IPlugin
{
    string Name { get; }
    void Execute();
}

public class DatabasePlugin : IPlugin
{
    public string Name => "Database Plugin";
    public void Execute() => Console.WriteLine("Database operations");
}

public class FilePlugin : IPlugin
{
    public string Name => "File Plugin";
    public void Execute() => Console.WriteLine("File operations");
}

// Discover and instantiate all plugins
var pluginTypes = ReflectionHelper.FindImplementingClasses<IPlugin>();
var plugins = pluginTypes.Select(type => (IPlugin)Activator.CreateInstance(type));

foreach (var plugin in plugins)
{
    Console.WriteLine($"Loading plugin: {plugin.Name}");
    plugin.Execute();
}
```

#### Command Pattern Implementation

```csharp
public abstract class Command
{
    public abstract void Execute();
}

public class SaveCommand : Command
{
    public override void Execute() => Console.WriteLine("Saving...");
}

public class LoadCommand : Command
{
    public override void Execute() => Console.WriteLine("Loading...");
}

// Build command registry
var commandTypes = ReflectionHelper.FindDerivedClasses<Command>();
var commandRegistry = new Dictionary<string, Type>();

foreach (var type in commandTypes)
{
    var commandName = type.Name.Replace("Command", "").ToLower();
    commandRegistry[commandName] = type;
}

// Usage
var commandType = commandRegistry["save"];
var command = (Command)Activator.CreateInstance(commandType);
command.Execute();
```

#### Service Discovery for Dependency Injection

```csharp
public interface IService
{
    void DoWork();
}

public class DatabaseService : IService
{
    public void DoWork() => Console.WriteLine("Database work");
}

public class EmailService : IService
{
    public void DoWork() => Console.WriteLine("Email work");
}

// Register all services
var serviceTypes = ReflectionHelper.FindImplementingClasses<IService>();
var services = new List<IService>();

foreach (var type in serviceTypes)
{
    var service = (IService)Activator.CreateInstance(type);
    services.Add(service);
}

// Use services
foreach (var service in services)
{
    service.DoWork();
}
```

#### Assembly-Specific Search

```csharp
// Search only in the current assembly
var currentAssembly = Assembly.GetExecutingAssembly();
var localTypes = ReflectionHelper.FindImplementingClasses<IMyInterface>(currentAssembly);

// Search in a specific plugin assembly
var pluginAssembly = Assembly.LoadFrom("MyPlugin.dll");
var pluginTypes = ReflectionHelper.FindImplementingClasses<IPlugin>(pluginAssembly);
```

### Performance Considerations

- **Assembly Loading**: The helper methods load all assemblies in the current application domain, which can be expensive
- **Type Caching**: Consider caching results if you need to call these methods frequently
- **Filtering**: Use the optional `assembly` parameter to limit the search scope when possible

### Error Handling

The reflection operations can throw various exceptions:

```csharp
try
{
    var types = ReflectionHelper.FindImplementingClasses<IMyInterface>();
}
catch (ReflectionTypeLoadException ex)
{
    Console.WriteLine($"Failed to load some types: {ex.Message}");
}
catch (FileLoadException ex)
{
    Console.WriteLine($"Failed to load assembly: {ex.Message}");
}
catch (BadImageFormatException ex)
{
    Console.WriteLine($"Invalid assembly format: {ex.Message}");
}
```

### Thread Safety

The `ReflectionHelper` methods are thread-safe and can be called concurrently. However, the underlying reflection operations may not be thread-safe for all scenarios, so use appropriate synchronization if needed.
