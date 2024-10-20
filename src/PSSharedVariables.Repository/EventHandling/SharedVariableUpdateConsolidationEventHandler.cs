using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSSharedVariables.Repository.EventHandling;

internal sealed class SharedVariableUpdateConsolidationEventHandler : INotificationHandler<SharedVariableUpdateConsolidationEvent>
{
    public Task Handle(SharedVariableUpdateConsolidationEvent notification, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
