using PSSharedVariables.Repository;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceExtensions
{
    public static IServiceCollection RegisterVariableServices(this IServiceCollection services, int defaultJobVariableTimeoutInMinutes = 5)
    {
        //services.AddHostedService<VariableRepositoryService>();
        services.AddSingleton<VariableRepository>();
        return services;
    }
}

