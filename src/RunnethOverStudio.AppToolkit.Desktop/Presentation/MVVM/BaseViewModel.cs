using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace RunnethOverStudio.AppToolkit.Presentation.MVVM;

/// <summary>
/// Serves as a base class for view models, providing common properties and methods
/// for MVVM scenarios, including support for busy state, dirty tracking, and long-running processes.
/// </summary>
public partial class BaseViewModel : ObservableValidator
{
    [ObservableProperty]
    private bool _isDirty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool? _longRunningProcessSuccessful = null;

    /// <summary>
    /// Adds event handlers for the underlying model. Override in derived classes to attach model events.
    /// </summary>
    public virtual void AddModelEvents() { }

    /// <summary>
    /// Removes event handlers of the underlying model. Override in derived classes to detach model events.
    /// </summary>
    public virtual void RemoveModelEvents() { }

    /// <summary>
    /// Initiates an asynchronous long-running process and manages busy and success state.
    /// </summary>
    /// <typeparam name="T">The type of the result returned by the long-running function.</typeparam>
    /// <param name="longRunningFunction">A function that returns a task representing the long-running operation. If its process fails, it needs to return false or null.</param>
    /// <returns>A task representing the asynchronous operation, with the result of type <typeparamref name="T"/>.</returns>
    protected async Task<T> InitiateLongRunningProcessAsync<T>(Func<Task<T>> longRunningFunction)
    {
        try
        {
            IsBusy = true;

            // Run the asynchronous function
            T result = await longRunningFunction();

            // Update the success flag
            LongRunningProcessSuccessful = (result is bool boolResult) ? boolResult : result != null;

            return result;
        }
        finally
        {
            IsBusy = false;
            LongRunningProcessSuccessful = null;
        }
    }

    /// <summary>
    /// Initiates a synchronous long-running process on a background thread and manages busy and success state.
    /// </summary>
    /// <typeparam name="T">The type of the result returned by the long-running function.</typeparam>
    /// <param name="longRunningFunction">A function representing the long-running operation. If its process fails, it needs to return false or null.</param>
    /// <returns>A task representing the asynchronous operation, with the result of type <typeparamref name="T"/>.</returns>
    protected async Task<T> InitiateLongRunningProcessAsync<T>(Func<T> longRunningFunction)
    {
        return await InitiateLongRunningProcessAsync(() => Task.Run(longRunningFunction));
    }

    /// <summary>
    /// Handles property change events for observable properties and sets <see cref="IsDirty"/> to true
    /// when any property except <see cref="IsDirty"/> changes.
    /// </summary>
    /// <param name="sender">The source of the property change event.</param>
    /// <param name="e">The event data containing the name of the property that changed.</param>
    protected void ObservableProperty_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (!string.Equals(e.PropertyName, nameof(IsDirty)))
        {
            IsDirty = true;
        }
    }
}
