using System.Diagnostics.CodeAnalysis;

namespace Beancounter.Datastructures;

public class Result<TSuccess,TError>
{
    public TSuccess? Success { get; private set; }
    public TError? Error { get; private set; }
    
    [MemberNotNullWhen(true, nameof(Success))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; private set; }
    
    public Result(TSuccess success)
    {
        Success = success;
        IsSuccess = true;
    }
    
    public Result(TError error)
    {
        Error = error;
        IsSuccess = false;
    }
    
    public static implicit operator Result<TSuccess, TError>(TSuccess success) => new Result<TSuccess, TError>(success);
    public static implicit operator Result<TSuccess, TError>(TError error) => new Result<TSuccess, TError>(error);
    public static Result<TSuccess, TError> Ok(TSuccess success) => new Result<TSuccess, TError>(success);
    public static Result<TSuccess, TError> Err(TError error) => new Result<TSuccess, TError>(error);
    public TSuccess Unwrap()
    {
        if (IsSuccess)
        {
            return Success!;
        }
        throw new Exception("Cannot unwrap error result");
    }
    
    public TError UnwrapError()
    {
        if (!IsSuccess)
        {
            return Error!;
        }
        throw new Exception("Cannot unwrap success result");
    }
    
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