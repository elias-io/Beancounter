# Examples and Usage Patterns

This document provides comprehensive examples and usage patterns for the Beancounter library.

## Table of Contents

- [Getting Started](#getting-started)
- [Data Structures Examples](#data-structures-examples)
- [Configuration Examples](#configuration-examples)
- [Extension Methods Examples](#extension-methods-examples)
- [Error Handling Examples](#error-handling-examples)
- [Real-World Scenarios](#real-world-scenarios)

## Getting Started

### Basic Setup

```csharp
using Beancounter.Datastructures;
using Beancounter.Extension;
using Beancounter.Configuration;
using Beancounter.TryWrap;

// Your application code here
```

### Simple Optional Usage

```csharp
// Create an optional value
var name = Optional<string>.Some("John Doe");
var empty = Optional<string>.None;

// Check if value exists
if (name.HasValue)
{
    Console.WriteLine($"Hello, {name.Value}!");
}

// Use with default value
var displayName = name.UnwrapOr("Anonymous");
```

## Data Structures Examples

### Optional<T> - Safe Null Handling

#### Basic Usage

```csharp
public class UserService
{
    public Optional<User> FindUser(int id)
    {
        try
        {
            var user = database.GetUser(id);
            return user != null ? Optional<User>.Some(user) : Optional<User>.None;
        }
        catch
        {
            return Optional<User>.None;
        }
    }
}

// Usage
var userService = new UserService();
var user = userService.FindUser(123);

await user.Match(
    some: async u => await SendWelcomeEmail(u.Email),
    none: async () => await LogUserNotFound(123)
);
```

#### Chaining Operations

```csharp
public class OrderService
{
    public Optional<Order> GetOrder(int orderId)
    {
        // Implementation...
    }
    
    public Optional<Customer> GetCustomer(int customerId)
    {
        // Implementation...
    }
}

// Chain optional operations
var order = orderService.GetOrder(456);
if (order.HasValue)
{
    var customer = orderService.GetCustomer(order.Value.CustomerId);
    customer.Match(
        some: c => Console.WriteLine($"Order for {c.Name}"),
        none: () => Console.WriteLine("Customer not found")
    );
}
```

### Result<T, E> - Error Handling

#### API Integration

```csharp
public class WeatherService
{
    public async Task<Result<WeatherData, string>> GetWeatherAsync(string city)
    {
        try
        {
            var response = await httpClient.GetAsync($"https://api.weather.com/{city}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<WeatherData>(json);
                return Result<WeatherData, string>.Ok(data);
            }
            else
            {
                return Result<WeatherData, string>.Err($"HTTP {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            return Result<WeatherData, string>.Err($"Network error: {ex.Message}");
        }
    }
}

// Usage
var weatherService = new WeatherService();
var result = await weatherService.GetWeatherAsync("London");

await result.Match(
    success: async weather => {
        Console.WriteLine($"Temperature: {weather.Temperature}°C");
        await UpdateWeatherDisplay(weather);
    },
    error: async error => {
        Console.WriteLine($"Weather unavailable: {error}");
        await ShowErrorMessage(error);
    }
);
```

#### Database Operations

```csharp
public class ProductRepository
{
    public Result<Product, DatabaseError> SaveProduct(Product product)
    {
        try
        {
            if (string.IsNullOrEmpty(product.Name))
            {
                return Result<Product, DatabaseError>.Err(
                    new DatabaseError { Code = "VALIDATION_ERROR", Message = "Product name is required" }
                );
            }
            
            var savedProduct = database.Save(product);
            return Result<Product, DatabaseError>.Ok(savedProduct);
        }
        catch (SqlException ex)
        {
            return Result<Product, DatabaseError>.Err(
                new DatabaseError { Code = "SQL_ERROR", Message = ex.Message }
            );
        }
    }
}
```

### AsyncBarrier - Coordination

#### Parallel Processing

```csharp
public class DataProcessor
{
    public async Task ProcessDataAsync(List<DataItem> items)
    {
        var barrier = new AsyncBarrier(items.Count);
        var tasks = new List<Task>();
        
        foreach (var item in items)
        {
            tasks.Add(ProcessItemAsync(item, barrier));
        }
        
        await Task.WhenAll(tasks);
        Console.WriteLine("All items processed!");
    }
    
    private async Task ProcessItemAsync(DataItem item, AsyncBarrier barrier)
    {
        // Do some work
        await ProcessItem(item);
        
        // Signal completion and wait for all others
        await barrier.SignalAndWaitAsync();
        
        // All items are now processed, do final work
        await FinalizeProcessing(item);
    }
}
```

#### Multi-Phase Operations

```csharp
public class BatchProcessor
{
    public async Task ProcessBatchAsync()
    {
        var phase1Barrier = new AsyncBarrier(3);
        var phase2Barrier = new AsyncBarrier(3);
        
        var tasks = new[]
        {
            Task.Run(async () => {
                await LoadData();
                await phase1Barrier.SignalAndWaitAsync();
                await TransformData();
                await phase2Barrier.SignalAndWaitAsync();
                await SaveData();
            }),
            Task.Run(async () => {
                await LoadConfig();
                await phase1Barrier.SignalAndWaitAsync();
                await ValidateConfig();
                await phase2Barrier.SignalAndWaitAsync();
                await ApplyConfig();
            }),
            Task.Run(async () => {
                await InitializeServices();
                await phase1Barrier.SignalAndWaitAsync();
                await StartServices();
                await phase2Barrier.SignalAndWaitAsync();
                await MonitorServices();
            })
        };
        
        await Task.WhenAll(tasks);
    }
}
```

### LazyAsync<T> - Lazy Loading

#### Expensive Resource Loading

```csharp
public class ResourceManager
{
    private readonly LazyAsync<DatabaseConnection> _connection;
    private readonly LazyAsync<Cache> _cache;
    
    public ResourceManager()
    {
        _connection = new LazyAsync<DatabaseConnection>(() => CreateConnectionAsync());
        _cache = new LazyAsync<Cache>(() => InitializeCacheAsync());
    }
    
    public async Task<Data> GetDataAsync(int id)
    {
        var connection = await _connection.Value;
        var cache = await _cache.Value;
        
        // Use connection and cache...
        return await connection.QueryAsync<Data>(id);
    }
    
    private async Task<DatabaseConnection> CreateConnectionAsync()
    {
        // Expensive connection creation
        await Task.Delay(2000);
        return new DatabaseConnection();
    }
    
    private async Task<Cache> InitializeCacheAsync()
    {
        // Expensive cache initialization
        await Task.Delay(1000);
        return new Cache();
    }
}
```

## Configuration Examples

### Basic Configuration

```csharp
public class DatabaseConfig : EnvironmentVariableConfiguration
{
    [EnvName("DB_HOST")]
    public string Host { get; set; }
    
    [EnvName("DB_PORT", "5432")]
    public int Port { get; set; }
    
    [EnvName("DB_NAME")]
    public string DatabaseName { get; set; }
    
    [EnvName("DB_USER")]
    public string Username { get; set; }
    
    [EnvName("DB_PASSWORD")]
    public string Password { get; set; }
    
    [EnvName("DB_SSL", "true")]
    public bool UseSsl { get; set; }
    
    [EnvName("DB_TIMEOUT", "30")]
    public int TimeoutSeconds { get; set; }
}

// Usage
var dbConfig = new DatabaseConfig();
var connectionString = $"Host={dbConfig.Host};Port={dbConfig.Port};Database={dbConfig.DatabaseName};Username={dbConfig.Username};Password={dbConfig.Password};SSL Mode={dbConfig.UseSsl}";
```

### Complex Configuration

```csharp
public class AppConfig : EnvironmentVariableConfiguration
{
    [EnvName("APP_NAME", "MyApp")]
    public string AppName { get; set; }
    
    [EnvName("LOG_LEVEL", "INFO")]
    public LogLevel LogLevel { get; set; }
    
    [EnvName("MAX_WORKERS", "4")]
    public int MaxWorkers { get; set; }
    
    [EnvName("ENABLE_METRICS", "false")]
    public bool EnableMetrics { get; set; }
    
    [EnvName("CACHE_TTL", "3600")]
    public int CacheTtlSeconds { get; set; }
    
    public DatabaseConfig Database { get; set; }
    public RedisConfig Redis { get; set; }
    
    public AppConfig()
    {
        Database = new DatabaseConfig();
        Redis = new RedisConfig();
    }
}

public class RedisConfig : EnvironmentVariableConfiguration
{
    [EnvName("REDIS_HOST", "localhost")]
    public string Host { get; set; }
    
    [EnvName("REDIS_PORT", "6379")]
    public int Port { get; set; }
    
    [EnvName("REDIS_PASSWORD")]
    public string Password { get; set; }
    
    [EnvName("REDIS_DATABASE", "0")]
    public int Database { get; set; }
}
```

## Extension Methods Examples

### String Processing

```csharp
// Convert to URL-safe identifier
var title = "My Awesome Blog Post!";
var slug = title.ToStringId(); // "my_awesome_blog_post"

// Remove diacritics
var text = "Café naïve résumé";
var ascii = text.ConvertToAscii(); // "Cafe naive resume"

// Combine operations
var filename = "My Document (Final Version).pdf";
var safeFilename = filename.ConvertToAscii().ToStringId(); // "my_document_final_version_pdf"
```

### Collection Operations

```csharp
// Functional foreach
var numbers = new[] { 1, 2, 3, 4, 5 };
numbers.ForEach(n => Console.WriteLine($"Number: {n}"));

// Async foreach
var urls = new[] { "http://example1.com", "http://example2.com" };
await urls.ForEachAsync(async url => {
    var response = await httpClient.GetAsync(url);
    Console.WriteLine($"Status: {response.StatusCode}");
});

// Type filtering
var objects = new object[] { "hello", 42, "world", 3.14, new List<int>() };
var strings = objects.OfType<string>(); // ["hello", "world"]
```

### Object Validation

```csharp
public class User
{
    public string Name { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }
    public Address Address { get; set; }
}

public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
}

// Validation
var user = new User
{
    Name = "John Doe",
    Email = "john@example.com",
    Age = 30,
    Address = new Address
    {
        Street = "123 Main St",
        City = "New York",
        Country = "USA"
    }
};

if (user.Validate(out var errors))
{
    Console.WriteLine("User is valid");
}
else
{
    foreach (var error in errors)
    {
        Console.WriteLine($"Validation error: {error}");
    }
}
```

### File Operations

```csharp
var directory = new DirectoryInfo("/path/to/project");

// Create subdirectory
var logsDir = directory.Add("logs");

// Get file safely
try
{
    var configFile = directory.GetFile("config.json");
    var content = File.ReadAllText(configFile.FullName);
}
catch (Exception ex)
{
    Console.WriteLine($"Config file not found: {ex.Message}");
}

// Clean up
logsDir.Purge(); // Deletes directory and all contents
```

## Error Handling Examples

### Try Wrapper Patterns

#### Safe API Calls

```csharp
public class ApiClient
{
    public async Task<Result<ApiResponse, ApiError>> GetDataAsync(string endpoint)
    {
        return await Try.ResultAsync(
            func: async () => {
                var response = await httpClient.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ApiResponse>(json);
            },
            errorFunc: ex => new ApiError { 
                Message = ex.Message, 
                Endpoint = endpoint,
                Timestamp = DateTime.UtcNow
            }
        );
    }
}
```

#### Safe File Operations

```csharp
public class FileService
{
    public Result<string, string> ReadFile(string path)
    {
        return Try.Result(
            func: () => File.ReadAllText(path),
            errorFunc: ex => $"Failed to read file '{path}': {ex.Message}"
        );
    }
    
    public Optional<string> DeleteFile(string path)
    {
        return Try.Optional(
            func: () => File.Delete(path),
            errorFunc: ex => $"Failed to delete file '{path}': {ex.Message}"
        );
    }
}
```

#### Fire-and-Forget Operations

```csharp
public class EventService
{
    public void PublishEvent(Event eventData)
    {
        // Fire-and-forget with error logging
        Try.Run(
            func: () => {
                // Publish to message queue
                messageQueue.Publish(eventData);
                
                // Update metrics
                metrics.IncrementEventCount();
            },
            errorFunc: ex => {
                logger.LogError(ex, "Failed to publish event {EventType}", eventData.Type);
            }
        );
    }
    
    public async Task PublishEventAsync(Event eventData)
    {
        // Async fire-and-forget
        _ = Try.RunAsync(
            func: async () => {
                await messageQueue.PublishAsync(eventData);
                await metrics.IncrementEventCountAsync();
            },
            errorFunc: async ex => {
                await logger.LogErrorAsync(ex, "Failed to publish event {EventType}", eventData.Type);
            }
        );
    }
}
```

## Real-World Scenarios

### Web API Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;
    
    public UsersController(UserService userService)
    {
        _userService = userService;
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _userService.GetUserAsync(id);
        
        return await user.Match(
            some: async u => {
                var response = new UserResponse
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email
                };
                return Ok(response);
            },
            none: async () => {
                return NotFound($"User with ID {id} not found");
            }
        );
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var result = await _userService.CreateUserAsync(request);
        
        return await result.Match(
            success: async user => {
                var response = new UserResponse
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email
                };
                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, response);
            },
            error: async error => {
                return BadRequest(new { error = error.Message });
            }
        );
    }
}
```

### Background Service

```csharp
public class DataProcessingService : BackgroundService
{
    private readonly ILogger<DataProcessingService> _logger;
    private readonly DataProcessor _processor;
    private readonly AppConfig _config;
    
    public DataProcessingService(
        ILogger<DataProcessingService> logger,
        DataProcessor processor,
        AppConfig config)
    {
        _logger = logger;
        _processor = processor;
        _config = config;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync();
                await Task.Delay(TimeSpan.FromSeconds(_config.ProcessingInterval), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in data processing service");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
    
    private async Task ProcessBatchAsync()
    {
        var batch = await _processor.GetNextBatchAsync();
        
        if (batch.HasValue)
        {
            var barrier = new AsyncBarrier(batch.Value.Items.Count);
            var tasks = batch.Value.Items.Select(item => ProcessItemAsync(item, barrier));
            
            await Task.WhenAll(tasks);
            _logger.LogInformation("Processed batch of {Count} items", batch.Value.Items.Count);
        }
    }
    
    private async Task ProcessItemAsync(DataItem item, AsyncBarrier barrier)
    {
        var result = await Try.ResultAsync(
            func: async () => await _processor.ProcessItemAsync(item),
            errorFunc: ex => new ProcessingError { ItemId = item.Id, Message = ex.Message }
        );
        
        await result.Match(
            success: async processedItem => {
                await _processor.SaveProcessedItemAsync(processedItem);
                await barrier.SignalAndWaitAsync();
            },
            error: async error => {
                _logger.LogError("Failed to process item {ItemId}: {Error}", error.ItemId, error.Message);
                await barrier.SignalAndWaitAsync();
            }
        );
    }
}
```

### Plugin System

```csharp
public interface IPlugin
{
    string Name { get; }
    string Version { get; }
    Task<Result<bool, string>> InitializeAsync();
    Task<Result<bool, string>> ExecuteAsync();
}

public class PluginManager
{
    private readonly List<IPlugin> _plugins = new();
    
    public void LoadPlugins()
    {
        var pluginTypes = ReflectionHelper.FindImplementingClasses<IPlugin>();
        
        foreach (var type in pluginTypes)
        {
            try
            {
                var plugin = (IPlugin)Activator.CreateInstance(type);
                _plugins.Add(plugin);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load plugin {type.Name}: {ex.Message}");
            }
        }
    }
    
    public async Task InitializeAllAsync()
    {
        var tasks = _plugins.Select(async plugin =>
        {
            var result = await Try.ResultAsync(
                func: async () => await plugin.InitializeAsync(),
                errorFunc: ex => $"Failed to initialize {plugin.Name}: {ex.Message}"
            );
            
            await result.Match(
                success: async success => {
                    if (success)
                    {
                        Console.WriteLine($"Plugin {plugin.Name} initialized successfully");
                    }
                    else
                    {
                        Console.WriteLine($"Plugin {plugin.Name} initialization failed");
                    }
                },
                error: async error => {
                    Console.WriteLine($"Plugin {plugin.Name} error: {error}");
                }
            );
        });
        
        await Task.WhenAll(tasks);
    }
}
```

These examples demonstrate the practical usage of all the components in the Beancounter library, showing how they can be combined to create robust, maintainable applications with proper error handling and functional programming patterns.
