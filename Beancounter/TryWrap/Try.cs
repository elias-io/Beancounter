using Beancounter.Datastructures;

namespace Beancounter.TryWrap;

/// <summary>
/// Provides static methods for exception-safe operations that return Result or Optional types.
/// </summary>
public static class Try {
    /// <summary>
    /// Executes an async function and returns a Result instead of throwing exceptions.
    /// </summary>
    /// <typeparam name="TSuccess">The type of the success value.</typeparam>
    /// <typeparam name="TError">The type of the error value.</typeparam>
    /// <param name="func">The async function to execute.</param>
    /// <param name="errorFunc">Function to convert exceptions to error values.</param>
    /// <returns>A Task that completes with a Result containing either the success value or error.</returns>
    public static async Task<Result<TSuccess,TError>> ResultAsync<TSuccess,TError>(
        Func<Task<TSuccess>> func, 
        Func<Exception, TError> errorFunc) {
        try {
            return new Result<TSuccess, TError>(await func());
        } catch (Exception e) {
            return new Result<TSuccess, TError>(errorFunc(e));
        }
    }
    
    /// <summary>
    /// Executes a synchronous function and returns a Result instead of throwing exceptions.
    /// </summary>
    /// <typeparam name="TSuccess">The type of the success value.</typeparam>
    /// <typeparam name="TError">The type of the error value.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <param name="errorFunc">Function to convert exceptions to error values.</param>
    /// <returns>A Result containing either the success value or error.</returns>
    public static Result<TSuccess,TError> Result<TSuccess,TError>(
        Func<TSuccess> func, 
        Func<Exception, TError> errorFunc) {
        try {
            return new Result<TSuccess, TError>(func());
        } catch (Exception e) {
            return new Result<TSuccess, TError>(errorFunc(e));
        }
    }
    
    /// <summary>
    /// Executes an async void function and returns an Optional instead of throwing exceptions.
    /// </summary>
    /// <typeparam name="TError">The type of the error value.</typeparam>
    /// <param name="func">The async void function to execute.</param>
    /// <param name="errorFunc">Function to convert exceptions to error values.</param>
    /// <returns>A Task that completes with an Optional containing either no value (success) or an error.</returns>
    public static async Task<Optional<TError>> OptionalAsync<TError>(
        Func<Task> func, 
        Func<Exception, TError> errorFunc) {
        try {
            await func();
            return new Optional<TError>();
        } catch (Exception e) {
            return new Optional<TError>(errorFunc(e));
        }
    }
    
    /// <summary>
    /// Executes a synchronous void function and returns an Optional instead of throwing exceptions.
    /// </summary>
    /// <typeparam name="TError">The type of the error value.</typeparam>
    /// <param name="func">The void function to execute.</param>
    /// <param name="errorFunc">Function to convert exceptions to error values.</param>
    /// <returns>An Optional containing either no value (success) or an error.</returns>
    public static Optional<TError> Optional<TError>(
        Action func, 
        Func<Exception, TError> errorFunc) {
        try {
            func();
            return new Optional<TError>();
        } catch (Exception e) {
            return new Optional<TError>(errorFunc(e));
        }
    }

    /// <summary>
    /// Executes a synchronous void function with optional error handling.
    /// </summary>
    /// <param name="func">The void function to execute.</param>
    /// <param name="errorFunc">Optional function to handle exceptions.</param>
    public static void Run(
        Action func,
        Action<Exception>? errorFunc = null) {
        try {
            func();
        }
        catch (Exception ex){
            errorFunc?.Invoke(ex);
        }
    }

    /// <summary>
    /// Executes an async void function with optional error handling.
    /// </summary>
    /// <param name="func">The async void function to execute.</param>
    /// <param name="errorFunc">Optional async function to handle exceptions.</param>
    /// <returns>A Task that completes when the function execution completes.</returns>
    public static async Task RunAsync(
        Func<Task> func,
        Func<Exception, Task>? errorFunc = null)
    {
        try
        {
            await func();
        }
        catch (Exception ex)
        {
            if (errorFunc is not null)
            {
                await errorFunc.Invoke(ex);
            }
        }
    }
    
    /// <summary>
    /// Executes a synchronous function and returns a fallback value when an exception occurs.
    /// </summary>
    /// <typeparam name="T">The return type of the operation.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <param name="errorFunc">Function invoked when an exception is thrown; returns a fallback value.</param>
    /// <returns>The result of <paramref name="func"/>, or the value returned by <paramref name="errorFunc"/> on error.</returns>
    public static T Run<T>(
        Func<T> func,
        Func<Exception, T> errorFunc) {
        try {
            return func();
        }
        catch (Exception ex){
            return errorFunc.Invoke(ex);
        }
    }

    /// <summary>
    /// Executes an asynchronous function and returns a fallback value when an exception occurs.
    /// </summary>
    /// <typeparam name="T">The return type of the operation.</typeparam>
    /// <param name="func">The asynchronous function to execute.</param>
    /// <param name="errorFunc">Asynchronous function invoked when an exception is thrown; returns a fallback value.</param>
    /// <returns>A task that resolves to the result of <paramref name="func"/>, or the value returned by <paramref name="errorFunc"/> on error.</returns>
    public static async Task<T> RunAsync<T>(
        Func<Task<T>> func,
        Func<Exception, Task<T>> errorFunc)
    {
        try
        {
            return await func();
        }
        catch (Exception ex)
        {
            return await errorFunc.Invoke(ex);
        }
    }
    
}