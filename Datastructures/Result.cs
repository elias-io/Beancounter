using System.Diagnostics.CodeAnalysis;

namespace Beancounter.Datastructures;

/// <summary>
/// Represents the result of an operation that can either succeed or fail.
/// Provides a functional programming pattern for error handling.
/// </summary>
/// <typeparam name="TSuccess">The type of the success value.</typeparam>
/// <typeparam name="TError">The type of the error value.</typeparam>
public class Result<TSuccess,TError>
{
    /// <summary>
    /// Gets the success value, null if IsSuccess is false.
    /// </summary>
    public TSuccess? Success { get; private set; }
    
    /// <summary>
    /// Gets the error value, null if IsSuccess is true.
    /// </summary>
    public TError? Error { get; private set; }
    
    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Success))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; private set; }
    
    /// <summary>
    /// Initializes a new instance of the Result class with a success value.
    /// </summary>
    /// <param name="success">The success value.</param>
    public Result(TSuccess success)
    {
        Success = success;
        IsSuccess = true;
    }
    
    /// <summary>
    /// Initializes a new instance of the Result class with an error value.
    /// </summary>
    /// <param name="error">The error value.</param>
    public Result(TError error)
    {
        Error = error;
        IsSuccess = false;
    }
    
    /// <summary>
    /// Implicitly converts a success value to a Result.
    /// </summary>
    /// <param name="success">The success value to convert.</param>
    /// <returns>A Result containing the success value.</returns>
    public static implicit operator Result<TSuccess, TError>(TSuccess success) => new Result<TSuccess, TError>(success);
    
    /// <summary>
    /// Implicitly converts an error value to a Result.
    /// </summary>
    /// <param name="error">The error value to convert.</param>
    /// <returns>A Result containing the error value.</returns>
    public static implicit operator Result<TSuccess, TError>(TError error) => new Result<TSuccess, TError>(error);
    
    /// <summary>
    /// Creates a successful Result.
    /// </summary>
    /// <param name="success">The success value.</param>
    /// <returns>A Result containing the success value.</returns>
    public static Result<TSuccess, TError> Ok(TSuccess success) => new Result<TSuccess, TError>(success);
    
    /// <summary>
    /// Creates a failed Result.
    /// </summary>
    /// <param name="error">The error value.</param>
    /// <returns>A Result containing the error value.</returns>
    public static Result<TSuccess, TError> Err(TError error) => new Result<TSuccess, TError>(error);
    
    /// <summary>
    /// Gets the success value, throwing an exception if the result is an error.
    /// </summary>
    /// <returns>The success value.</returns>
    /// <exception cref="Exception">Thrown when the result is an error.</exception>
    public TSuccess Unwrap()
    {
        if (IsSuccess)
        {
            return Success!;
        }
        throw new Exception("Cannot unwrap error result");
    }
    
    /// <summary>
    /// Gets the error value, throwing an exception if the result is a success.
    /// </summary>
    /// <returns>The error value.</returns>
    /// <exception cref="Exception">Thrown when the result is a success.</exception>
    public TError UnwrapError()
    {
        if (!IsSuccess)
        {
            return Error!;
        }
        throw new Exception("Cannot unwrap success result");
    }
    
    /// <summary>
    /// Executes different functions based on whether the result is a success or error.
    /// </summary>
    /// <typeparam name="TOut">The return type of the functions.</typeparam>
    /// <param name="successFunc">Function to execute if successful.</param>
    /// <param name="errorFunc">Function to execute if error.</param>
    /// <returns>A Task that completes with the result of the executed function.</returns>
    public async Task<TOut> Match<TOut>(Func<TSuccess, Task<TOut>> successFunc, Func<TError, Task<TOut>> errorFunc)
    {
        if (IsSuccess)
        {
            return await successFunc(Success!);
        }
        else
        {
            return await errorFunc(Error!);
        }
    }
    
    /// <summary>
    /// Executes different void functions based on whether the result is a success or error.
    /// </summary>
    /// <param name="successFunc">Function to execute if successful.</param>
    /// <param name="errorFunc">Function to execute if error.</param>
    /// <returns>A Task that completes when the executed function completes.</returns>
    public async Task Match(Func<TSuccess, Task> successFunc, Func<TError, Task> errorFunc)
    {
        if (IsSuccess)
        {
            await successFunc(Success!);
        }
        else
        {
            await errorFunc(Error!);
        }
    }
}