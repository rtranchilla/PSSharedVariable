using MediatR;
using System;

namespace PSSharedVariables
{
    public sealed class VariableUpdateRequest : IRequest<INotification>
    {
        public VariableUpdateRequest(Guid repositoryId) => RepositoryId = repositoryId;

        public Guid RepositoryId { get; }
    }
}
