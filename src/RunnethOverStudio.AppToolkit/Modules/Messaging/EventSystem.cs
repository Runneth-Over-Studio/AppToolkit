using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;

namespace RunnethOverStudio.AppToolkit.Modules.Messaging;

/// <summary>
/// Provides a thread-safe implementation of <see cref="IEventSystem"/> for publishing and subscribing to events.
/// Releases its references, including handlers, when disposed.
/// </summary>
/// <remarks>
/// This implementation is intended for use within in-process applications only. It does not provide features required for distributed scenarios,
/// such as an outbox or retries. For inter-process or distributed messaging, use a dedicated messaging infrastructure. 
/// Also, if you ever start to see GC pressure or performance issues, you may want to consider a more involved solution like Disruptor-net.
/// </remarks>
public sealed class EventSystem(ILogger<IEventSystem> logger) : IEventSystem
{
    private readonly ConcurrentDictionary<Type, ImmutableList<Delegate>> _handlers = new();
    private readonly ILogger<IEventSystem> _logger = logger;
    private bool _disposed = false;

    /// <inheritdoc/>
    public void Publish<T>(object? sender, T eventData) where T : EventArgs
    {
        if (!_disposed)
        {
            Type key = typeof(EventHandler<T>);

            if (_handlers.TryGetValue(key, out ImmutableList<Delegate>? handlers))
            {
                // The use of the immutable collection allows for lock-free reads.
                foreach (EventHandler<T> handler in handlers.Cast<EventHandler<T>>())
                {
                    try
                    {
                        handler(sender, eventData);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred while invoking event handler for {EventType}.", key.Name);
                    }
                }
            }
        }
        else
        {
            throw new ObjectDisposedException(nameof(EventSystem));
        }
    }

    /// <inheritdoc/>
    public void Subscribe<T>(EventHandler<T> handler) where T : EventArgs
    {
        if (!_disposed)
        {
            Type key = typeof(EventHandler<T>);
            _handlers.AddOrUpdate(key, [handler], (k, list) => list.Contains(handler) ? list : list.Add(handler));
        }
        else
        {
            throw new ObjectDisposedException(nameof(EventSystem));
        }
    }

    /// <inheritdoc/>
    public void Unsubscribe<T>(EventHandler<T> handler) where T : EventArgs
    {
        if (!_disposed)
        {
            Type key = typeof(EventHandler<T>);
            _handlers.AddOrUpdate(key, [], (k, list) => list.Remove(handler));
        }
        else
        {
            throw new ObjectDisposedException(nameof(EventSystem));
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _handlers.Clear();
            }

            _disposed = true;
        }
    }
}
