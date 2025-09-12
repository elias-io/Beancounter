// ReSharper disable MemberCanBePrivate.Global

using System.Diagnostics.CodeAnalysis;

namespace Beancounter.Datastructures;

/// <summary>
/// Represents an optional value that may or may not be present.
/// Provides a functional programming pattern for handling nullable values safely.
/// </summary>
/// <typeparam name="TValue">The type of the optional value.</typeparam>
public class Optional<TValue>
{
    /// <summary>
    /// Gets the wrapped value, null if HasValue is false.
    /// </summary>
    public TValue? Value { get; private set; }
    
    /// <summary>
    /// Gets a value indicating whether a value is present.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Value))]
    public bool HasValue { get; private set; }
    
    /// <summary>
    /// Initializes a new instance of the Optional class with the specified value.
    /// </summary>
    /// <param name="value">The value to wrap, or null for empty optional.</param>
    public Optional(TValue? value)
    {
        if (value is null) {
            HasValue = false;
        }
        else {
            Value = value;
            HasValue = true;
        }
    }
    
    /// <summary>
    /// Initializes a new instance of the Optional class as empty.
    /// </summary>
    public Optional()
    {
        HasValue = false;
    }
    
    /// <summary>
    /// Creates an Optional with a value.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>An Optional containing the specified value.</returns>
    public static Optional<TValue> Some(TValue value) => new(value);
    
    /// <summary>
    /// Implicitly converts a value to an Optional.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>An Optional containing the value.</returns>
    public static implicit operator Optional<TValue>(TValue? value) => new(value);
    
    /// <summary>
    /// Gets an empty Optional instance.
    /// </summary>
    public static Optional<TValue> None => new();

    /// <summary>
    /// Gets an empty Optional instance (alias for None).
    /// </summary>
    public static Optional<TValue> Empty => new();

    /// <summary>
    /// Gets the wrapped value, throwing an exception if empty.
    /// </summary>
    /// <returns>The wrapped value.</returns>
    /// <exception cref="Exception">Thrown when the Optional is empty.</exception>
    public TValue Unwrap()
    {
        if (HasValue)
        {
            return Value!;
        }
        throw new Exception("Cannot unwrap empty optional");
    }

    /// <summary>
    /// Gets the wrapped value or returns the default if empty.
    /// </summary>
    /// <param name="defaultValue">The default value to return if empty.</param>
    /// <returns>The wrapped value or the default value.</returns>
    public TValue UnwrapOr(TValue defaultValue)
    {
        return HasValue ? Value! : defaultValue;
    }
    
    /// <summary>
    /// Executes different functions based on whether the Optional has a value.
    /// </summary>
    /// <typeparam name="TOut">The return type of the functions.</typeparam>
    /// <param name="someFunc">Function to execute if value is present.</param>
    /// <param name="noneFunc">Function to execute if value is absent.</param>
    /// <returns>A Task that completes with the result of the executed function.</returns>
    public async Task<TOut> Match<TOut>(Func<TValue, Task<TOut>> someFunc, Func<Task<TOut>> noneFunc)
    {
        if (HasValue)
        {
            return await someFunc(Value!);
        }
        else
        {
            return await noneFunc();
        }
    }
    
    /// <summary>
    /// Executes different void functions based on whether the Optional has a value.
    /// </summary>
    /// <param name="someFunc">Function to execute if value is present.</param>
    /// <param name="noneFunc">Function to execute if value is absent.</param>
    /// <returns>A Task that completes when the executed function completes.</returns>
    public async Task Match(Func<TValue, Task> someFunc, Func<Task> noneFunc)
    {
        if (HasValue)
        {
            await someFunc(Value!);
        }
        else
        {
            await noneFunc();
        }
    }
    
}