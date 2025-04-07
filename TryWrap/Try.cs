using Beancounter.Datastructures;

namespace Beancounter.TryWrap;

public static class Try {
    public static async Task<Result<TSuccess,TError>> ResultAsync<TSuccess,TError>(
        Func<Task<TSuccess>> func, 
        Func<Exception, TError> errorFunc) {
        try {
            return new Result<TSuccess, TError>(await func());
        } catch (Exception e) {
            return new Result<TSuccess, TError>(errorFunc(e));
        }
    }
    public static Result<TSuccess,TError> Result<TSuccess,TError>(
        Func<TSuccess> func, 
        Func<Exception, TError> errorFunc) {
        try {
            return new Result<TSuccess, TError>(func());
        } catch (Exception e) {
            return new Result<TSuccess, TError>(errorFunc(e));
        }
    }
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

    public static async Task RunAsync(
        Func<Task> func,
        Func<Exception, Task>? errorFunc = null) {
        try {
            await func();
        }
        catch (Exception ex){
            if (errorFunc is not null) {
                await errorFunc.Invoke(ex);
            }
        }
    }


}