# Beancounter

A comprehensive .NET utility library providing functional programming patterns, async utilities, extension methods, and configuration management for modern C# applications.

## Features

- **Functional Programming Patterns**: Optional and Result types for safer, more expressive code
- **Async Utilities**: AsyncBarrier and LazyAsync for concurrent programming scenarios
- **Extension Methods**: Useful extensions for common .NET types (String, IEnumerable, Object, etc.)
- **Configuration Management**: Environment variable-based configuration with automatic type conversion
- **Error Handling**: Try wrapper for exception-safe operations
- **Reflection Utilities**: Helper methods for type discovery and plugin systems

## Installation

### NuGet Package

```bash
dotnet add package dev.elias.beancounter
```

### Package Manager

```powershell
Install-Package dev.elias.beancounter
```

### PackageReference

```xml
<PackageReference Include="dev.elias.beancounter" Version="1.0.1" />
```

## Quick Start

### Optional Values

```csharp
using Beancounter.Datastructures;

var user = Optional<string>.Some("John Doe");
var displayName = user.UnwrapOr("Anonymous");

await user.Match(
    some: async name => await SendWelcomeEmail(name),
    none: async () => await LogUserNotFound()
);
```

### Result Types

```csharp
using Beancounter.Datastructures;
using Beancounter.TryWrap;

var result = Try.Result(
    func: () => ParseJson(jsonString),
    errorFunc: ex => $"Parse Error: {ex.Message}"
);

await result.Match(
    success: async data => await ProcessData(data),
    error: async error => await LogError(error)
);
```

### Environment Configuration

```csharp
using Beancounter.Configuration;

public class AppConfig : EnvironmentVariableConfiguration
{
    [EnvName("API_KEY")]
    public string ApiKey { get; set; }
    
    [EnvName("PORT", "8080")]
    public int Port { get; set; }
    
    [EnvName("DEBUG", "false")]
    public bool Debug { get; set; }
}

var config = new AppConfig();
```

### Async Coordination

```csharp
using Beancounter.Datastructures;

var barrier = new AsyncBarrier(3);

// In three different async methods:
await barrier.SignalAndWaitAsync(); // All three will wait here until all arrive
```

## Documentation

Comprehensive documentation is available in the [docs](docs/) folder:

- [API Reference](docs/README.md) - Complete API documentation
- [Data Structures](docs/datastructures.md) - Optional, Result, AsyncBarrier, LazyAsync
- [Extensions](docs/extensions.md) - String, IEnumerable, Object, and other extensions
- [Configuration](docs/configuration.md) - Environment variable configuration system
- [Helper Classes](docs/helpers.md) - Reflection utilities
- [Try Wrapper](docs/try-wrapper.md) - Exception-safe operations
- [Examples](docs/examples.md) - Real-world usage patterns and examples

## Key Components

### Data Structures

- **Optional<T>**: Safe handling of nullable values with functional patterns
- **Result<TSuccess, TError>**: Explicit error handling without exceptions
- **AsyncBarrier**: Synchronization primitive for coordinating async operations
- **LazyAsync<T>**: Lazy initialization for expensive async operations

### Extensions

- **String Extensions**: ASCII conversion, URL-safe identifiers
- **IEnumerable Extensions**: Functional foreach operations, type filtering
- **Object Extensions**: Validation with detailed error reporting
- **HashSet Extensions**: Bulk operations
- **Guid Extensions**: Base64 and plain string representations
- **DirectoryInfo Extensions**: File operations and directory management

### Configuration

- **EnvironmentVariableConfiguration**: Base class for automatic environment variable loading
- **EnvName Attribute**: Declarative configuration mapping
- **VariableLoader**: Singleton for loading from system environment and .env files

### Error Handling

- **Try Class**: Exception-safe operations returning Result or Optional types
- **Run Methods**: Fire-and-forget operations with optional error handling

## Requirements

- .NET 8.0 or later
- Newtonsoft.Json 13.0.3
- DotNetEnv 3.1.1

## License

This project is licensed under the terms specified in the [LICENSE](LICENSE) file.

## Contributing

This is a personal utility library. For suggestions or improvements, please contact the maintainer.

## Version History

- **1.0.1** - Current version with comprehensive documentation and XML comments
- **1.0.0** - Initial release with core functionality
