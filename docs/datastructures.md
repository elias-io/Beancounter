# Data Structures API Reference

This document provides detailed API reference for the data structures in the Beancounter library.

## Table of Contents

- [Optional<T>](#optionalt)
- [Result<TSuccess, TError>](#resulttsuccess-terror)
- [AsyncBarrier](#asyncbarrier)
- [LazyAsync<T>](#lazyasynct)

## Optional<T>

The `Optional<T>` class provides a functional programming pattern for handling nullable values safely, similar to `Option` types in functional languages.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Value` | `TValue?` | The wrapped value, null if `HasValue` is false |
| `HasValue` | `bool` | Indicates whether a value is present |

### Constructors

#### `Optional(TValue? value)`
Creates an Optional with the specified value.

**Parameters:**
- `value` (`TValue?`): The value to wrap, or null for empty optional

**Example:**
```csharp
var optional = new Optional<string>("Hello");
var empty = new Optional<string>(null);
```

#### `Optional()`
Creates an empty Optional.

**Example:**
```csharp
var empty = new Optional<string>();
```

### Static Methods

#### `Some(TValue value)`
Creates an Optional with a value.

**Parameters:**
- `value` (`TValue`): The value to wrap

**Returns:** `Optional<TValue>`

**Example:**
```csharp
var optional = Optional<string>.Some("Hello");
```

#### `None`
Gets an empty Optional instance.

**Returns:** `Optional<TValue>`

**Example:**
```csharp
var empty = Optional<string>.None;
```

#### `Empty`
Gets an empty Optional instance (alias for `None`).

**Returns:** `Optional<TValue>`

### Instance Methods

#### `Unwrap()`
Gets the wrapped value, throwing an exception if empty.

**Returns:** `TValue`

**Exceptions:**
- `Exception`: Thrown when the Optional is empty

**Example:**
```csharp
var optional = Optional<string>.Some("Hello");
var value = optional.Unwrap(); // "Hello"
```

#### `UnwrapOr(TValue defaultValue)`
Gets the wrapped value or returns the default if empty.

**Parameters:**
- `defaultValue` (`TValue`): The default value to return if empty

**Returns:** `TValue`

**Example:**
```csharp
var optional = Optional<string>.None;
var value = optional.UnwrapOr("Default"); // "Default"
```

#### `Match<TOut>(Func<TValue, Task<TOut>> someFunc, Func<Task<TOut>> noneFunc)`
Executes different functions based on whether the Optional has a value.

**Parameters:**
- `someFunc` (`Func<TValue, Task<TOut>>`): Function to execute if value is present
- `noneFunc` (`Func<Task<TOut>>`): Function to execute if value is absent

**Returns:** `Task<TOut>`

**Example:**
```csharp
var result = await optional.Match(
    some: async value => await ProcessValue(value),
    none: async () => await GetDefaultValue()
);
```

#### `Match(Func<TValue, Task> someFunc, Func<Task> noneFunc)`
Executes different void functions based on whether the Optional has a value.

**Parameters:**
- `someFunc` (`Func<TValue, Task>`): Function to execute if value is present
- `noneFunc` (`Func<Task>`): Function to execute if value is absent

**Returns:** `Task`

### Implicit Operators

#### `implicit operator Optional<TValue>(TValue? value)`
Implicitly converts a value to an Optional.

**Example:**
```csharp
Optional<string> optional = "Hello"; // Implicit conversion
```

## Result<TSuccess, TError>

The `Result<TSuccess, TError>` class represents the result of an operation that can either succeed or fail, similar to `Result` types in functional languages.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Success` | `TSuccess?` | The success value, null if `IsSuccess` is false |
| `Error` | `TError?` | The error value, null if `IsSuccess` is true |
| `IsSuccess` | `bool` | Indicates whether the operation succeeded |

### Constructors

#### `Result(TSuccess success)`
Creates a successful Result.

**Parameters:**
- `success` (`TSuccess`): The success value

**Example:**
```csharp
var result = new Result<int, string>(42);
```

#### `Result(TError error)`
Creates a failed Result.

**Parameters:**
- `error` (`TError`): The error value

**Example:**
```csharp
var result = new Result<int, string>("Operation failed");
```

### Static Methods

#### `Ok(TSuccess success)`
Creates a successful Result.

**Parameters:**
- `success` (`TSuccess`): The success value

**Returns:** `Result<TSuccess, TError>`

**Example:**
```csharp
var result = Result<int, string>.Ok(42);
```

#### `Err(TError error)`
Creates a failed Result.

**Parameters:**
- `error` (`TError`): The error value

**Returns:** `Result<TSuccess, TError>`

**Example:**
```csharp
var result = Result<int, string>.Err("Operation failed");
```

### Instance Methods

#### `Unwrap()`
Gets the success value, throwing an exception if the result is an error.

**Returns:** `TSuccess`

**Exceptions:**
- `Exception`: Thrown when the result is an error

**Example:**
```csharp
var result = Result<int, string>.Ok(42);
var value = result.Unwrap(); // 42
```

#### `UnwrapError()`
Gets the error value, throwing an exception if the result is a success.

**Returns:** `TError`

**Exceptions:**
- `Exception`: Thrown when the result is a success

**Example:**
```csharp
var result = Result<int, string>.Err("Failed");
var error = result.UnwrapError(); // "Failed"
```

#### `Match<TOut>(Func<TSuccess, Task<TOut>> successFunc, Func<TError, Task<TOut>> errorFunc)`
Executes different functions based on whether the result is a success or error.

**Parameters:**
- `successFunc` (`Func<TSuccess, Task<TOut>>`): Function to execute if successful
- `errorFunc` (`Func<TError, Task<TOut>>`): Function to execute if error

**Returns:** `Task<TOut>`

**Example:**
```csharp
var result = await result.Match(
    success: async value => await ProcessSuccess(value),
    error: async error => await HandleError(error)
);
```

#### `Match(Func<TSuccess, Task> successFunc, Func<TError, Task> errorFunc)`
Executes different void functions based on whether the result is a success or error.

**Parameters:**
- `successFunc` (`Func<TSuccess, Task>`): Function to execute if successful
- `errorFunc` (`Func<TError, Task>`): Function to execute if error

**Returns:** `Task`

### Implicit Operators

#### `implicit operator Result<TSuccess, TError>(TSuccess success)`
Implicitly converts a success value to a Result.

**Example:**
```csharp
Result<int, string> result = 42; // Implicit conversion to success
```

#### `implicit operator Result<TSuccess, TError>(TError error)`
Implicitly converts an error value to a Result.

**Example:**
```csharp
Result<int, string> result = "Error"; // Implicit conversion to error
```

## AsyncBarrier

The `AsyncBarrier` class coordinates multiple asynchronous participants across discrete phases. Each participant calls `SignalAndWaitAsync(participantId, step)` with a stable `participantId` and a diagnostic `step` name. When the configured number of participants have arrived, the phase opens and up to a configured number of participants may proceed concurrently.

### Constructors

#### `AsyncBarrier(int participantCount, int parallelExecutions)`
Creates a new AsyncBarrier with the specified number of participants and maximum concurrent executions per phase.

**Parameters:**
- `participantCount` (`int`): Total number of distinct participants that will coordinate
- `parallelExecutions` (`int`): Maximum participants allowed to proceed concurrently once a phase opens; use 0 to equal `participantCount`

**Exceptions:**
- `ArgumentOutOfRangeException`: Thrown when `participantCount` <= 0 or `parallelExecutions` < 0

**Example:**
```csharp
// 5 participants, at most 2 proceed concurrently after each phase opens
var barrier = new AsyncBarrier(participantCount: 5, parallelExecutions: 2);
```

### Methods

#### `Task SignalAndWaitAsync(string participantId, string step, CancellationToken ct = default)`
Signals arrival of a participant to the current phase, waits until the phase opens, then acquires an execution slot.

**Parameters:**
- `participantId` (`string`): Stable identifier for a participant; must be non-empty
- `step` (`string`): Human-readable label for the phase (diagnostics only)
- `ct` (`CancellationToken`): Optional token to cancel the wait

**Returns:** `Task`

**Exceptions:**
- `ArgumentException`: When `participantId` or `step` is null/whitespace
- `InvalidOperationException`: When more distinct participants than `participantCount` attempt to join
- `ObjectDisposedException`: When called after disposal

**Behavior:**
- The last arrival opens the phase and unblocks all waiting participants
- At most `parallelExecutions` participants may proceed concurrently
- Each participant holds its execution slot until its next call, smoothing throughput across phases
- Thread-safe and supports concurrent callers

**Example:**
```csharp
var barrier = new AsyncBarrier(participantCount: 3, parallelExecutions: 2);

// Suppose these run in different tasks/services with stable IDs
await barrier.SignalAndWaitAsync("A", step: "load-input");
await barrier.SignalAndWaitAsync("B", step: "load-input");
await barrier.SignalAndWaitAsync("C", step: "load-input");

// Next phase
await barrier.SignalAndWaitAsync("A", step: "transform");
await barrier.SignalAndWaitAsync("B", step: "transform");
await barrier.SignalAndWaitAsync("C", step: "transform");
```

## LazyAsync<T>

The `LazyAsync<T>` class provides lazy initialization for async operations. The async operation is only executed when the value is first accessed.

### Constructors

#### `LazyAsync(Func<Task<T>> taskFactory)`
Creates a new LazyAsync with the specified task factory.

**Parameters:**
- `taskFactory` (`Func<Task<T>>`): The factory function that creates the async task

**Example:**
```csharp
var lazyValue = new LazyAsync<string>(() => LoadDataFromDatabaseAsync());
```

### Properties

#### `Value`
Gets the lazy async value. The task factory is executed on first access.

**Returns:** `Task<T>`

**Example:**
```csharp
var lazyValue = new LazyAsync<string>(() => LoadDataAsync());
var result = await lazyValue.Value; // LoadDataAsync() is called here
```

**Note:** The task factory is executed using `Task.Run()`, so it runs on a background thread.
