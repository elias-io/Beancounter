# Configuration API Reference

This document provides detailed API reference for the configuration system in the Beancounter library.

## Table of Contents

- [EnvironmentVariableConfiguration](#environmentvariableconfiguration)
- [EnvName Attribute](#envname-attribute)
- [VariableLoader](#variableloader)

## EnvironmentVariableConfiguration

The `EnvironmentVariableConfiguration` abstract base class provides automatic loading of configuration values from environment variables using reflection and attributes.

### Usage

Create a configuration class that inherits from `EnvironmentVariableConfiguration` and use the `[EnvName]` attribute to specify which environment variables to load.

```csharp
public class DatabaseConfig : EnvironmentVariableConfiguration
{
    [EnvName("DATABASE_URL")]
    public string DatabaseUrl { get; set; }
    
    [EnvName("DATABASE_PORT", "5432")]
    public int Port { get; set; }
    
    [EnvName("DEBUG", "false")]
    public bool Debug { get; set; }
    
    [EnvName("MAX_CONNECTIONS")]
    public int? MaxConnections { get; set; }
}
```

### Constructor

#### `EnvironmentVariableConfiguration()`

The constructor automatically loads all properties marked with `[EnvName]` attributes from environment variables.

**Behavior:**
- Iterates through all public properties of the derived class
- Looks for `[EnvName]` attributes on each property
- Loads the corresponding environment variable value
- Converts the string value to the property's type
- Sets default values if specified in the attribute
- Throws exceptions for missing required variables or conversion failures

**Exceptions:**
- `ArgumentException`: Thrown when a required environment variable is not set
- `NotSupportedException`: Thrown when an environment variable cannot be converted to the property type

### Type Conversion

The configuration system supports automatic type conversion for the following types:

- **Primitive Types**: `int`, `long`, `double`, `float`, `decimal`, `bool`, `char`
- **String Types**: `string`
- **Nullable Types**: `int?`, `bool?`, etc.
- **Enum Types**: Any enum type
- **Custom Types**: Any type that has a `TypeConverter` registered

### Example

```csharp
public class AppConfig : EnvironmentVariableConfiguration
{
    [EnvName("API_KEY")]
    public string ApiKey { get; set; }
    
    [EnvName("PORT", "8080")]
    public int Port { get; set; }
    
    [EnvName("ENABLE_LOGGING", "true")]
    public bool EnableLogging { get; set; }
    
    [EnvName("MAX_RETRIES", "3")]
    public int MaxRetries { get; set; }
    
    [EnvName("TIMEOUT_SECONDS", "30")]
    public int TimeoutSeconds { get; set; }
}

// Usage
var config = new AppConfig();
Console.WriteLine($"API Key: {config.ApiKey}");
Console.WriteLine($"Port: {config.Port}");
```

## EnvName Attribute

The `EnvName` attribute is used to mark properties that should be loaded from environment variables.

### Constructors

#### `EnvName(string name)`

Creates an attribute for a required environment variable.

**Parameters:**
- `name` (`string`): The name of the environment variable

**Example:**
```csharp
[EnvName("DATABASE_URL")]
public string DatabaseUrl { get; set; }
```

#### `EnvName(string name, string? defaultValue)`

Creates an attribute for an optional environment variable with a default value.

**Parameters:**
- `name` (`string`): The name of the environment variable
- `defaultValue` (`string?`): The default value if the environment variable is not set

**Example:**
```csharp
[EnvName("PORT", "8080")]
public int Port { get; set; }
```

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Name` | `string` | The name of the environment variable |
| `DefaultValue` | `string?` | The default value if the environment variable is not set |
| `HasDefaultValue` | `bool` | Indicates whether a default value is specified |

## VariableLoader

The `VariableLoader` class is responsible for loading environment variables from both the system environment and `.env` files.

### Properties

#### `Instance`

Gets the singleton instance of the VariableLoader.

**Returns:** `VariableLoader`

**Example:**
```csharp
var loader = VariableLoader.Instance;
var value = loader.GetEnvironmentVariable("MY_VAR");
```

### Methods

#### `GetEnvironmentVariable(string key)`

Gets the value of an environment variable.

**Parameters:**
- `key` (`string`): The name of the environment variable

**Returns:** `string?` - The value of the environment variable, or null if not found

**Example:**
```csharp
var loader = VariableLoader.Instance;
var apiKey = loader.GetEnvironmentVariable("API_KEY");
```

### Behavior

The `VariableLoader` loads environment variables from multiple sources in the following order:

1. **System Environment Variables**: Variables set in the system environment
2. **`.env` Files**: Variables from `.env` files in the current directory and parent directories

**Environment Variable Loading:**
- Loads all system environment variables on initialization
- Uses `DotNetEnv` library to load `.env` files
- Variables from `.env` files override system environment variables
- Only loads non-null and non-whitespace values

**Thread Safety:**
- The `VariableLoader` is thread-safe and can be used concurrently
- Uses lazy initialization for the singleton instance

### Example

```csharp
// .env file content:
// API_KEY=my-secret-key
// DEBUG=true
// PORT=3000

var loader = VariableLoader.Instance;
var apiKey = loader.GetEnvironmentVariable("API_KEY"); // "my-secret-key"
var debug = loader.GetEnvironmentVariable("DEBUG"); // "true"
var port = loader.GetEnvironmentVariable("PORT"); // "3000"
```

## Advanced Usage

### Custom Type Conversion

For custom types, you can register a `TypeConverter`:

```csharp
public class CustomTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
        return sourceType == typeof(string);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        if (value is string stringValue)
        {
            // Custom conversion logic
            return new CustomType(stringValue);
        }
        return base.ConvertFrom(context, culture, value);
    }
}

// Register the converter
TypeDescriptor.AddAttributes(typeof(CustomType), new TypeConverterAttribute(typeof(CustomTypeConverter)));

public class Config : EnvironmentVariableConfiguration
{
    [EnvName("CUSTOM_VALUE")]
    public CustomType CustomValue { get; set; }
}
```

### Nested Configuration

You can create nested configuration objects:

```csharp
public class DatabaseConfig : EnvironmentVariableConfiguration
{
    [EnvName("DB_HOST")]
    public string Host { get; set; }
    
    [EnvName("DB_PORT", "5432")]
    public int Port { get; set; }
}

public class AppConfig : EnvironmentVariableConfiguration
{
    [EnvName("API_KEY")]
    public string ApiKey { get; set; }
    
    public DatabaseConfig Database { get; set; }
    
    public AppConfig()
    {
        Database = new DatabaseConfig();
    }
}
```

### Error Handling

```csharp
try
{
    var config = new AppConfig();
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Missing required environment variable: {ex.Message}");
}
catch (NotSupportedException ex)
{
    Console.WriteLine($"Type conversion error: {ex.Message}");
}
```
