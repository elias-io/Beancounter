# Beancounter Documentation

Welcome to the Beancounter library documentation. This library provides a collection of reusable utilities, data structures, and extensions for .NET applications.

## Table of Contents

- [Overview](#overview)
- [Installation](#installation)
- [Core Components](#core-components)
  - [Data Structures](#data-structures)
  - [Extensions](#extensions)
  - [Configuration](#configuration)
  - [Helper Classes](#helper-classes)
  - [Try Wrapper](#try-wrapper)
- [API Reference](#api-reference)
- [Examples](#examples)
- [Contributing](#contributing)

## Overview

Beancounter is a utility library that provides:

- **Functional Programming Patterns**: Optional and Result types for safer code
- **Async Utilities**: AsyncBarrier and LazyAsync for concurrent programming
- **Extension Methods**: Useful extensions for common .NET types
- **Configuration Management**: Environment variable-based configuration
- **Error Handling**: Try wrapper for exception-safe operations
- **Reflection Utilities**: Helper methods for type discovery

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

## Core Components

### Data Structures

#### Optional<T>
A functional programming pattern for handling nullable values safely.

```csharp
var optional = Optional<string>.Some("Hello");
var result = optional.UnwrapOr("Default");
```

#### Result<TSuccess, TError>
Represents the result of an operation that can either succeed or fail.

```csharp
var result = Result<int, string>.Ok(42);
var value = result.Unwrap(); // 42
```

#### AsyncBarrier
Synchronization primitive for coordinating multiple async operations.

```csharp
var barrier = new AsyncBarrier(3);
await barrier.SignalAndWaitAsync(); // Waits for all 3 participants
```

#### LazyAsync<T>
Lazy initialization for async operations.

```csharp
var lazyValue = new LazyAsync<string>(() => LoadDataAsync());
var result = await lazyValue.Value;
```

### Extensions

#### String Extensions
- `ConvertToAscii()`: Removes diacritics from strings
- `ToStringId()`: Converts strings to URL-safe identifiers

#### IEnumerable Extensions
- `ForEach()`: Functional foreach operation
- `ForEachAsync()`: Async foreach operation
- `OfType<T>()`: Type-safe filtering

#### Object Extensions
- `Validate()`: Validates objects using data annotations

#### Other Extensions
- `HashSet.AddRange()`: Adds multiple items to HashSet
- `Guid.ToBase64String()`: Converts GUID to Base64
- `DirectoryInfo` utilities: File operations and directory management

### Configuration

#### EnvironmentVariableConfiguration
Base class for configuration objects that automatically load from environment variables.

```csharp
public class MyConfig : EnvironmentVariableConfiguration
{
    [EnvName("DATABASE_URL")]
    public string DatabaseUrl { get; set; }
    
    [EnvName("PORT", "8080")]
    public int Port { get; set; }
}
```

### Helper Classes

#### ReflectionHelper
Utilities for type discovery and reflection operations.

```csharp
var derivedTypes = ReflectionHelper.FindDerivedClasses<MyBaseClass>();
var implementingTypes = ReflectionHelper.FindImplementingClasses<IMyInterface>();
```

### Try Wrapper

#### Try Class
Provides exception-safe operations that return Result or Optional types.

```csharp
var result = Try.Result(() => RiskyOperation(), ex => ex.Message);
var optional = Try.Optional(() => VoidOperation(), ex => ex.Message);
```

## API Reference

For detailed API documentation, see:
- [Data Structures API](datastructures.md)
- [Extensions API](extensions.md)
- [Configuration API](configuration.md)
- [Helper Classes API](helpers.md)
- [Try Wrapper API](try-wrapper.md)

## Examples

### Using Optional for Safe Null Handling

```csharp
var user = GetUser(id);
var name = user.Match(
    some: u => u.Name,
    none: () => "Anonymous"
);
```

### Using Result for Error Handling

```csharp
var result = Try.Result(() => ParseJson(json), ex => $"Parse error: {ex.Message}");
await result.Match(
    success: data => ProcessData(data),
    error: error => LogError(error)
);
```

### Environment Configuration

```csharp
public class AppConfig : EnvironmentVariableConfiguration
{
    [EnvName("API_KEY")]
    public string ApiKey { get; set; }
    
    [EnvName("DEBUG", "false")]
    public bool Debug { get; set; }
    
    [EnvName("MAX_RETRIES", "3")]
    public int MaxRetries { get; set; }
}

var config = new AppConfig();
```

### Async Coordination

```csharp
var barrier = new AsyncBarrier(3);

// In three different tasks
await barrier.SignalAndWaitAsync(); // All tasks will wait here until all 3 arrive
```

## Contributing

This is a personal utility library. For suggestions or improvements, please contact the maintainer.

## License

See [LICENSE](../LICENSE) file for details.
