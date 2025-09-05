namespace Beancounter.Datastructures;

/// <summary>
/// Provides lazy initialization for async operations.
/// The async operation is only executed when the value is first accessed.
/// </summary>
/// <typeparam name="T">The type of the value to be lazily initialized.</typeparam>
public class LazyAsync<T>
{
    private readonly Lazy<Task<T>> lazyTask;

    /// <summary>
    /// Initializes a new instance of the LazyAsync class with the specified task factory.
    /// </summary>
    /// <param name="taskFactory">The factory function that creates the async task.</param>
    public LazyAsync(Func<Task<T>> taskFactory)
    {
        lazyTask = new Lazy<Task<T>>(() => Task.Run(taskFactory));
    }
    
    /// <summary>
    /// Gets the lazy async value. The task factory is executed on first access.
    /// </summary>
    /// <value>A Task that represents the lazy async value.</value>
    public Task<T> Value => lazyTask.Value;
}