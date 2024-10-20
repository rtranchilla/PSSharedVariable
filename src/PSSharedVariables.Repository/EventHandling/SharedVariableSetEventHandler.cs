using MediatR;

namespace PSSharedVariables.Repository.EventHandling;

internal sealed class SharedVariableSetEventHandler : INotificationHandler<SharedVariableSetEvent>
{
    public Task Handle(SharedVariableSetEvent notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
