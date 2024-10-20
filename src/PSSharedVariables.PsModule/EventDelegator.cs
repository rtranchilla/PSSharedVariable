using MediatR;
using System.Collections.Concurrent;

namespace PSSharedVariables.PsModule;

public sealed class EventDelegator : IPublisher
{
    private readonly ConcurrentQueue<INotification> events = new();

    public Task Publish(object notification, CancellationToken cancellationToken = default) => notification is INotification n ? Publish(n, cancellationToken) : Task.CompletedTask;

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification =>
        Task.Run(() => events.Enqueue(notification), cancellationToken);
}
