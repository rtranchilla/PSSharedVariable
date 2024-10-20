﻿using System.Threading.Tasks;
using System.Threading;
using MediatR;

namespace PSSharedVariables
{
    public interface IPublisher
    {
        Task Publish(object notification, CancellationToken cancellationToken = default);

        Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification;
    }
}
