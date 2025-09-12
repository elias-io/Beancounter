# Extensions API Reference

This document provides detailed API reference for the extension methods in the Beancounter library.

## Table of Contents

- [String Extensions](#string-extensions)
- [IEnumerable Extensions](#ienumerable-extensions)
- [Object Extensions](#object-extensions)
- [HashSet Extensions](#hashset-extensions)
- [Guid Extensions](#guid-extensions)
- [DirectoryInfo Extensions](#directoryinfo-extensions)
- [JsonSerializer Extensions](#jsonserializer-extensions)

## String Extensions

### `ConvertToAscii(this string input)`

Converts a string to ASCII by removing diacritics and special characters.

**Parameters:**
- `input` (`string`): The input string to convert

**Returns:** `string` - The ASCII version of the input string

**Example:**
```csharp
var text = "Café naïve résumé";
var ascii = text.ConvertToAscii(); // "Cafe naive resume"
```

**Behavior:**
- Returns the original string if it's null or empty
- Normalizes the string to separate base characters from diacritics
- Removes all non-spacing mark characters (diacritics)
- Normalizes back to composed form

### `ToStringId(this string input)`

Converts a string to a URL-safe identifier by removing special characters and normalizing.

**Parameters:**
- `input` (`string`): The input string to convert

**Returns:** `string` - A URL-safe identifier

**Example:**
```csharp
var text = "Hello World! @#$%";
var id = text.ToStringId(); // "hello_world"
```

**Behavior:**
- Replaces spaces with underscores
- Converts to lowercase
- Removes diacritics using `ConvertToAscii()`
- Replaces non-alphanumeric characters with underscores
- Collapses multiple consecutive underscores into single underscores
- Trims trailing underscores

## IEnumerable Extensions

### `ForEach<T>(this IEnumerable<T> collection, Action<T> predicate)`

Executes an action for each element in the collection.

**Parameters:**
- `collection` (`IEnumerable<T>`): The collection to iterate over
- `predicate` (`Action<T>`): The action to execute for each element

**Example:**
```csharp
var numbers = new[] { 1, 2, 3, 4, 5 };
numbers.ForEach(n => Console.WriteLine(n));
```

### `ForEachAsync<T>(this IEnumerable<T> collection, Func<T, Task> predicate)`

Executes an async action for each element in the collection sequentially.

**Parameters:**
- `collection` (`IEnumerable<T>`): The collection to iterate over
- `predicate` (`Func<T, Task>`): The async action to execute for each element

**Returns:** `Task`

**Example:**
```csharp
var urls = new[] { "http://example1.com", "http://example2.com" };
await urls.ForEachAsync(async url => await ProcessUrlAsync(url));
```

### `OfType<T>(this IEnumerable<object> collection) where T : class`

Filters the collection to only include elements of the specified type.

**Parameters:**
- `collection` (`IEnumerable<object>`): The collection to filter

**Returns:** `IEnumerable<T>` - Filtered collection containing only elements of type T

**Example:**
```csharp
var objects = new object[] { "hello", 42, "world", 3.14 };
var strings = objects.OfType<string>(); // ["hello", "world"]
```

**Note:** This is different from LINQ's `OfType<T>()` as it uses exact type matching rather than inheritance checking.

## Object Extensions

### `Validate(this object? obj, out List<string> validationErrors)`

Validates an object and its properties, checking for null values in non-nullable properties.

**Parameters:**
- `obj` (`object?`): The object to validate
- `validationErrors` (`out List<string>`): List to receive validation error messages

**Returns:** `bool` - True if validation passes, false otherwise

**Example:**
```csharp
var person = new Person { Name = "John", Age = 30 };
if (!person.Validate(out var errors))
{
    foreach (var error in errors)
    {
        Console.WriteLine(error);
    }
}
```

**Validation Rules:**
- Checks that non-nullable properties are not null
- Recursively validates nested objects
- Handles arrays and collections properly
- Respects nullable reference types and nullable value types
- Provides detailed error messages with property paths

**Error Message Format:**
- `"Property 'PropertyName' is non-nullable but is null."`
- `"Property 'Parent.Child.PropertyName' is non-nullable but is null."`
- `"Property 'Items[0].PropertyName' is non-nullable but is null."`

## HashSet Extensions

### `AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> range)`

Adds multiple items to the HashSet.

**Parameters:**
- `hashSet` (`HashSet<T>`): The HashSet to add items to
- `range` (`IEnumerable<T>`): The items to add

**Returns:** `HashSet<T>` - The same HashSet instance (for method chaining)

**Example:**
```csharp
var set = new HashSet<string>();
set.AddRange(new[] { "apple", "banana", "cherry" });
```

## Guid Extensions

### `ToBase64String(this Guid guid)`

Converts a GUID to its Base64 string representation.

**Parameters:**
- `guid` (`Guid`): The GUID to convert

**Returns:** `string` - Base64 encoded string representation of the GUID

**Example:**
```csharp
var guid = Guid.NewGuid();
var base64 = guid.ToBase64String(); // "SGVsbG8gV29ybGQ="
```

### `ToPlainString(this Guid guid)`

Converts a GUID to a plain string without hyphens.

**Parameters:**
- `guid` (`Guid`): The GUID to convert

**Returns:** `string` - GUID string without hyphens

**Example:**
```csharp
var guid = Guid.NewGuid();
var plain = guid.ToPlainString(); // "1234567890abcdef1234567890abcdef"
```

## DirectoryInfo Extensions

### `Add(this DirectoryInfo directoryInfo, string path)`

Creates a subdirectory with the specified path.

**Parameters:**
- `directoryInfo` (`DirectoryInfo`): The parent directory
- `path` (`string`): The subdirectory path to create

**Returns:** `DirectoryInfo` - The created subdirectory

**Example:**
```csharp
var parent = new DirectoryInfo("/path/to/parent");
var child = parent.Add("subdirectory");
```

### `GetFile(this DirectoryInfo directoryInfo, string fileName)`

Gets a FileInfo object for a file in the directory.

**Parameters:**
- `directoryInfo` (`DirectoryInfo`): The directory to search in
- `fileName` (`string`): The name of the file

**Returns:** `FileInfo` - FileInfo object for the specified file

**Exceptions:**
- `Exception`: Thrown if the file does not exist

**Example:**
```csharp
var directory = new DirectoryInfo("/path/to/directory");
var file = directory.GetFile("config.json");
```

### `Purge(this DirectoryInfo directoryInfo)`

Deletes the directory and all its contents recursively.

**Parameters:**
- `directoryInfo` (`DirectoryInfo`): The directory to delete

**Exceptions:**
- `IOException`: Various I/O related exceptions
- `UnauthorizedAccessException`: Insufficient permissions
- `ArgumentException`: Invalid path
- `ArgumentNullException`: Null path
- `PathTooLongException`: Path too long
- `DirectoryNotFoundException`: Directory not found

**Example:**
```csharp
var directory = new DirectoryInfo("/path/to/temp");
directory.Purge(); // Deletes directory and all contents
```

## JsonSerializer Extensions

### `Serialize(this JsonSerializer jsonSerializer, object obj)`

Serializes an object to a JSON string using the JsonSerializer instance.

**Parameters:**
- `jsonSerializer` (`JsonSerializer`): The JsonSerializer instance to use
- `obj` (`object`): The object to serialize

**Returns:** `string` - JSON string representation of the object

**Example:**
```csharp
var serializer = new JsonSerializer();
var person = new { Name = "John", Age = 30 };
var json = serializer.Serialize(person); // {"Name":"John","Age":30}
```

**Note:** This extension method provides a convenient way to serialize objects to strings without manually managing StringWriter instances.
