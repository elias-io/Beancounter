# Try Wrapper API Reference

This document provides detailed API reference for the Try wrapper utilities in the Beancounter library.

## Table of Contents

- [Try Class](#try-class)
  - [ResultAsync](#resultasync)
  - [Result](#result)
  - [OptionalAsync](#optionalasync)
  - [Optional](#optional)
  - [Run](#run)
  - [RunAsync](#runasync)

## Try Class

The `Try` class provides static methods for exception-safe operations that return `Result` or `Optional` types instead of throwing exceptions.

### Methods

#### `ResultAsync<TSuccess, TError>(Func<Task<TSuccess>> func, Func<Exception, TError> errorFunc)`

Executes an async function and returns a `Result<TSuccess, TError>` instead of throwing exceptions.

**Parameters:**
- `func` (`Func<Task<TSuccess>>`): The async function to execute
- `errorFunc` (`Func<Exception, TError>`): Function to convert exceptions to error values

**Returns:** `Task<Result<TSuccess, TError>>` - A task that completes with a Result containing either the success value or error

**Example:**
```csharp
var result = await Try.ResultAsync(
    func: async () => await LoadDataFromApiAsync(),
    errorFunc: ex => $"API Error: {ex.Message}"
);

await result.Match(
    success: async data => await ProcessData(data),
    error: async error => await LogError(error)
);
```

**Behavior:**
- Executes the provided async function
- If successful, returns `Result<TSuccess, TError>.Ok(successValue)`
- If an exception occurs, catches it and returns `Result<TSuccess, TError>.Err(errorFunc(exception))`
- Never throws exceptions (except for argument validation)

#### `Result<TSuccess, TError>(Func<TSuccess> func, Func<Exception, TError> errorFunc)`

Executes a synchronous function and returns a `Result<TSuccess, TError>` instead of throwing exceptions.

**Parameters:**
- `func` (`Func<TSuccess>`): The function to execute
- `errorFunc` (`Func<Exception, TError>`): Function to convert exceptions to error values

**Returns:** `Result<TSuccess, TError>` - A Result containing either the success value or error

**Example:**
```csharp
var result = Try.Result(
    func: () => ParseJson(jsonString),
    errorFunc: ex => $"Parse Error: {ex.Message}"
);

result.Match(
    success: data => Console.WriteLine($"Parsed: {data}"),
    error: error => Console.WriteLine($"Error: {error}")
);
```

**Behavior:**
- Executes the provided function
- If successful, returns `Result<TSuccess, TError>.Ok(successValue)`
- If an exception occurs, catches it and returns `Result<TSuccess, TError>.Err(errorFunc(exception))`
- Never throws exceptions (except for argument validation)

#### `OptionalAsync<TError>(Func<Task> func, Func<Exception, TError> errorFunc)`

Executes an async void function and returns an `Optional<TError>` instead of throwing exceptions.

**Parameters:**
- `func` (`Func<Task>`): The async void function to execute
- `errorFunc` (`Func<Exception, TError>`): Function to convert exceptions to error values

**Returns:** `Task<Optional<TError>>` - A task that completes with an Optional containing either no value (success) or an error

**Example:**
```csharp
var optional = await Try.OptionalAsync(
    func: async () => await SaveDataAsync(data),
    errorFunc: ex => $"Save Error: {ex.Message}"
);

await optional.Match(
    some: async error => await LogError(error),
    none: async () => await NotifySuccess()
);
```

**Behavior:**
- Executes the provided async void function
- If successful, returns `Optional<TError>.None` (empty optional)
- If an exception occurs, catches it and returns `Optional<TError>.Some(errorFunc(exception))`
- Never throws exceptions (except for argument validation)

#### `Optional<TError>(Action func, Func<Exception, TError> errorFunc)`

Executes a synchronous void function and returns an `Optional<TError>` instead of throwing exceptions.

**Parameters:**
- `func` (`Action`): The void function to execute
- `errorFunc` (`Func<Exception, TError>`): Function to convert exceptions to error values

**Returns:** `Optional<TError>` - An Optional containing either no value (success) or an error

**Example:**
```csharp
var optional = Try.Optional(
    func: () => DeleteFile(filePath),
    errorFunc: ex => $"Delete Error: {ex.Message}"
);

optional.Match(
    some: error => Console.WriteLine($"Error: {error}"),
    none: () => Console.WriteLine("File deleted successfully")
);
```

**Behavior:**
- Executes the provided void function
- If successful, returns `Optional<TError>.None` (empty optional)
- If an exception occurs, catches it and returns `Optional<TError>.Some(errorFunc(exception))`
- Never throws exceptions (except for argument validation)

#### `Run(Action func, Action<Exception>? errorFunc = null)`

Executes a synchronous void function with optional error handling.

**Parameters:**
- `func` (`Action`): The void function to execute
- `errorFunc` (`Action<Exception>?`): Optional function to handle exceptions

**Example:**
```csharp
Try.Run(
    func: () => RiskyOperation(),
    errorFunc: ex => Console.WriteLine($"Operation failed: {ex.Message}")
);
```

**Behavior:**
- Executes the provided void function
- If an exception occurs and `errorFunc` is provided, calls `errorFunc(exception)`
- If an exception occurs and `errorFunc` is null, silently ignores the exception
- Never throws exceptions (except for argument validation)

#### `RunAsync(Func<Task> func, Func<Exception, Task>? errorFunc = null)`

Executes an async void function with optional error handling.

**Parameters:**
- `func` (`Func<Task>`): The async void function to execute
- `errorFunc` (`Func<Exception, Task>?`): Optional async function to handle exceptions

**Returns:** `Task`

**Example:**
```csharp
await Try.RunAsync(
    func: async () => await RiskyAsyncOperation(),
    errorFunc: async ex => await LogErrorAsync(ex)
);
```

**Behavior:**
- Executes the provided async void function
- If an exception occurs and `errorFunc` is provided, calls `await errorFunc(exception)`
- If an exception occurs and `errorFunc` is null, silently ignores the exception
- Never throws exceptions (except for argument validation)

## Usage Patterns

### Error Handling with Result

```csharp
// API call with error handling
var result = await Try.ResultAsync(
    func: async () => await httpClient.GetStringAsync("https://api.example.com/data"),
    errorFunc: ex => new ApiError { Message = ex.Message, StatusCode = 500 }
);

await result.Match(
    success: async json => {
        var data = JsonSerializer.Deserialize<MyData>(json);
        await ProcessData(data);
    },
    error: async error => {
        await LogError(error);
        await NotifyUser("Failed to load data");
    }
);
```

### Safe File Operations

```csharp
// Safe file reading
var result = Try.Result(
    func: () => File.ReadAllText("config.json"),
    errorFunc: ex => $"File read error: {ex.Message}"
);

var config = result.UnwrapOr("{}");
```

### Database Operations

```csharp
// Safe database operation
var result = await Try.ResultAsync(
    func: async () => await database.SaveAsync(entity),
    errorFunc: ex => new DatabaseError { 
        Message = ex.Message, 
        EntityId = entity.Id 
    }
);

if (result.IsSuccess)
{
    Console.WriteLine("Entity saved successfully");
}
else
{
    var error = result.UnwrapError();
    Console.WriteLine($"Save failed: {error.Message}");
}
```

### Void Operations with Optional

```csharp
// Safe cleanup operation
var optional = Try.Optional(
    func: () => CleanupTempFiles(),
    errorFunc: ex => $"Cleanup failed: {ex.Message}"
);

if (optional.HasValue)
{
    var error = optional.Unwrap();
    Console.WriteLine($"Warning: {error}");
}
```

### Fire-and-Forget Operations

```csharp
// Fire-and-forget with error logging
Try.Run(
    func: () => SendAnalyticsEvent(),
    errorFunc: ex => Logger.LogError("Analytics failed", ex)
);

// Async fire-and-forget
_ = Try.RunAsync(
    func: async () => await SendNotificationAsync(),
    errorFunc: async ex => await Logger.LogErrorAsync("Notification failed", ex)
);
```

## Best Practices

### 1. Use Descriptive Error Messages

```csharp
// Good
var result = Try.Result(
    func: () => ParseJson(json),
    errorFunc: ex => $"Failed to parse JSON: {ex.Message}"
);

// Better
var result = Try.Result(
    func: () => ParseJson(json),
    errorFunc: ex => new ParseError { 
        Input = json, 
        Message = ex.Message,
        Line = GetLineNumber(ex)
    }
);
```

### 2. Chain Operations

```csharp
var result = await Try.ResultAsync(
    func: async () => await LoadDataAsync(),
    errorFunc: ex => $"Load failed: {ex.Message}"
);

if (result.IsSuccess)
{
    var processResult = Try.Result(
        func: () => ProcessData(result.Unwrap()),
        errorFunc: ex => $"Process failed: {ex.Message}"
    );
    
    // Handle processResult...
}
```

### 3. Use Appropriate Return Types

```csharp
// Use Result for operations that return values
var dataResult = Try.Result(() => LoadData(), ex => ex.Message);

// Use Optional for void operations
var saveResult = Try.Optional(() => SaveData(), ex => ex.Message);

// Use Run for fire-and-forget operations
Try.Run(() => LogEvent(), ex => Console.WriteLine($"Log failed: {ex.Message}"));
```

### 4. Handle Errors Appropriately

```csharp
var result = await Try.ResultAsync(
    func: async () => await CriticalOperationAsync(),
    errorFunc: ex => new CriticalError(ex)
);

await result.Match(
    success: async data => await ProcessSuccess(data),
    error: async error => {
        await LogCriticalError(error);
        await NotifyAdministrators(error);
        await ShutdownGracefully();
    }
);
```
