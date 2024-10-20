using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace PSSharedVariables.PsModule;

internal static class DependencyManager
{
    static DependencyManager()
    {
        config = new ConfigurationBuilder()
            .SetBasePath(Path.GetDirectoryName(Assembly.GetAssembly(typeof(DependencyManager))!.Location)!)
            .AddJsonFile("PSSharedVariables.PsModule.json", true)
            .Build();

        var services = new ServiceCollection();
        //services.AddMediatR(cfg =>
        //{
        //    cfg.RegisterServicesFromAssemblyContaining<PSSharedVariableDriveInfo>();
        //});

        //services.AddSingleton<ISender, Sender>();
        services.AddSingleton<IPublisher, EventDelegator>();
        services.AddSingleton<IPsObjectHandler, PsObjectHandler>();
        services.AddSingleton<IObjectConverter, ObjectConverter>();
        services.AddSingleton<IConfiguration>(config);
        services.AddTransient<SharedVariableRepository>();
        //services.AddSingleton<VariableEventDispatcher>();

        serviceProvider = services.BuildServiceProvider();
    }

    private static readonly IConfigurationRoot config;
    private static readonly IServiceProvider serviceProvider;

    public static IServiceProvider ServiceProvider => serviceProvider;
}
