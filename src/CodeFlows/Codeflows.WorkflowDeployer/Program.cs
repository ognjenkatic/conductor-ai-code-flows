using Codeflows.WorkflowDeployer;
using ConductorSharp.Engine.Extensions;
using ConductorSharp.Engine.Health;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

var services = new ServiceCollection();

var builder = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .AddJsonFile($"appsettings.json", true, true)
    .AddJsonFile($"appsettings.Development.json", true, true);

var logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Build()).CreateLogger();

var configuration = builder.Build();

services.AddLogging(builder =>
{
    builder.ClearProviders();
    builder.AddSerilog(logger);
});

services
    .AddConductorSharp(
        baseUrl: configuration.GetValue<string>("Conductor:BaseUrl")
            ?? throw new InvalidOperationException("Conductor:BaseUrl configuration value not set.")
    )
    .AddExecutionManager(
        maxConcurrentWorkers: configuration.GetValue("Conductor:MaxConcurrentWorkers", 10),
        sleepInterval: configuration.GetValue("Conductor:SleepInterval", 500),
        longPollInterval: configuration.GetValue("Conductor:LongPollInterval", 100),
        domain: null,
        typeof(Program).Assembly
    )
    .SetHealthCheckService<InMemoryHealthService>()
    .AddPipelines(pipelines =>
    {
        pipelines.AddExecutionTaskTracking();
        pipelines.AddContextLogging();
        pipelines.AddRequestResponseLogging();
        pipelines.AddValidation();
    });

services.RegisterWorkflow<RefactorRepository>();
services.RegisterWorkflow<CsharpRefactorProject>();
services.RegisterWorkflow<HandleRefactorFailure>();

var provider = services.BuildServiceProvider();

var deployer = provider.GetRequiredService<IDeploymentService>();
var module = provider.GetRequiredService<ModuleDeployment>();

await deployer.Deploy(module);
