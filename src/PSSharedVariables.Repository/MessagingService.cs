using Microsoft.Extensions.Hosting;
using PSSharedVariables.Repository.Hosting;

namespace PSSharedVariables.Repository;

public sealed class MessagingService : BackgroundService
{
    public MessagingService(RequestHostPool hostPool)
    {
        for (int i = 0; i < 10; i++)
            hostPool.Create();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;
}
