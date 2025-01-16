// ReSharper disable MemberCanBePrivate.Global

using System.Diagnostics.CodeAnalysis;

namespace Beancounter.Datastructures;

public class Optional<TValue>
{
    public TValue? Value { get; private set; }
    
    [MemberNotNullWhen(true, nameof(Value))]
    public bool HasValue { get; private set; }
    
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
    
    public Optional()
    {
        HasValue = false;
    }
    
    public static Optional<TValue> Some(TValue value) => new(value);
    
    public static implicit operator Optional<TValue>(TValue? value) => new(value);
    
    public static Optional<TValue> None => new();

    public static Optional<TValue> Empty => new();

    public TValue Unwrap()
    {
        if (HasValue)
        {
            return Value!;
        }
        throw new Exception("Cannot unwrap empty optional");
    }

    public TValue UnwrapOr(TValue defaultValue)
    {
        return HasValue ? Value! : defaultValue;
    }
    
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