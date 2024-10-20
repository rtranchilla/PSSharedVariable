using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSSharedVariables.Repository.EventHandling;

internal sealed class SharedVariableRemoveEventHandler : INotificationHandler<SharedVariableRemoveEvent>
{
    public Task Handle(SharedVariableRemoveEvent notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
