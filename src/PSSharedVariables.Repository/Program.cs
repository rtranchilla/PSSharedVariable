using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using PSSharedVariables.Repository;
using PSSharedVariables.Repository.Hosting;

//var configuration = new ConfigurationBuilder()
//    .AddJsonFile("MtsClientTools.Client.json", false)
//    .AddJsonFile("MtsClientTools.Client.Service.json", true)
//    .Build();

IHost host = Host.CreateDefaultBuilder(args)
    //.ConfigureAppConfiguration(builder =>
    //{
    //    builder.Sources.Remove(
    //        builder.Sources.First(s =>
    //            s is JsonConfigurationSource js &&
    //            StringComparer.InvariantCultureIgnoreCase.Equals(js.Path, "appsettings.json")
    //            )
    //        );
    //    builder.AddConfiguration(configuration);
    //})
    //.UseWindowsService(options =>
    //{
    //    options.ServiceName = "MTS Client Tools";
    //})
    .ConfigureServices(services =>
    {
        services.AddSingleton(new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        });
        services.AddSingleton<VariableRepository>();
        services.AddTransient<MessageSerializer>();
        services.AddTransient<RequestHost>();
        services.AddSingleton<RequestHostPool>();
        services.AddHostedService<MessagingService>();
        //services.AddMediatR(cfg =>
        //{
            //cfg.RegisterServicesFromAssemblyContaining<MtsClientTools.Client.Variables.VariableRepositoryService>();
            //cfg.RegisterServicesFromAssemblyContaining<MtsClientTools.Client.JobExecution.ConditionEvaluator>();
        //});

        //var variableConfig = configuration.GetSection("Variables");
        //services.RegisterVariableServices(variableConfig?.GetValue("JobVariableTimeoutInMinutes", 5) ?? 5);

        //services.RegisterDummyRepos();
        //var messagingConfig = configuration.GetSection("Messaging");
        //services.RegisterMessaging(cfg =>
        //{
        //    cfg.RegisterServicesFromAssemblyContaining<MtsClientTools.Client.Variables.VariableRepositoryService>();
        //    cfg.RegisterServicesFromAssemblyContaining<MtsClientTools.Client.JobExecution.ConditionEvaluator>();

        //    cfg.ExecutionContext = executionContext;

        //    var servicePipeName = messagingConfig.GetValue<string>("ServicePipeName");
        //    if (!string.IsNullOrWhiteSpace(servicePipeName))
        //        cfg.ServicePipeName = servicePipeName;
        //    var agentPipeName = messagingConfig.GetValue<string>("AgentPipeName");
        //    if (!string.IsNullOrWhiteSpace(agentPipeName))
        //        cfg.AgentPipeName = agentPipeName;
        //    var maxSimultaneousStreams = messagingConfig.GetValue("MaxSimultaneousStreams", 0);
        //    if (maxSimultaneousStreams <= 0)
        //        cfg.MaxNumberofHostInstances = maxSimultaneousStreams;
        //});
        //var jobStateConfig = configuration.GetSection("JobState");
        //services.RegisterJobStateService(jobStateConfig.GetValue("PipeName", "MCT_JobState") ?? "MCT_JobState",
        //jobStateConfig.GetValue("MaxSimultaneousStreams", 20),
        //jobStateConfig.GetValue("InitalAvailableStreams", 4),
        //jobStateConfig.GetValue("DefaultJobStateTimeoutInMinutes", 5),
        //jobStateConfig.GetValue("ScopeCleanupIntervalInSeconds", 60));
        //services.RegisterCoreServices();
        //services.RegisterClientToolsExtensionServices();
        //services.RegisterTriggerPublishing();
        //services.RegisterDummyRepos();
        //services.AddSingleton<ExtensionManager>();
        //services.AddTransient<ExtensionLoadContext>();
        //services.AddHostedService<RefreshService>();
        //services.AddHostedService<TestService2>();
        //TODO: This is replaced by RegisterJobStateClient, right? services.RegisterVariables();
        //services.RegisterJobStateClient(jobStateConfig.GetValue("PipeName", "MCT_JobState") ?? "MCT_JobState");
        //services.RegisterTaskExecutionService();
        //services.AddSingleton<IJobStateTestProvider, JobStateTestProvider>();
    })
    .Build();

await host.RunAsync();