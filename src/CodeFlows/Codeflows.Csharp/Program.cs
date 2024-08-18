using Codeflows.Csharp.Common.Configuration;
using Codeflows.Csharp.Quality.Services;
using Codeflows.Csharp.Quality.Workers;
using ConductorSharp.Engine.Extensions;
using ConductorSharp.Engine.Health;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

await Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        var builder = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddJsonFile($"appsettings.json", true, true)
            .AddJsonFile($"appsettings.Development.json", true, true);

        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Build())
            .CreateLogger();

        var configuration = builder.Build();

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(logger);
        });

        var sonarqubePat =
            configuration.GetValue<string>("Sonarqube:Token")
            ?? throw new InvalidOperationException("Sonarqube:Token configuration value not set.");

        var sonarqubeBaseUrl =
            configuration.GetValue<string>("Sonarqube:BaseUrl")
            ?? throw new InvalidOperationException(
                "Sonarqube:BaseUrl configuration value not set."
            );

        services.AddSingleton(
            new SonarqubeConfiguration { Token = sonarqubePat, BaseUrl = sonarqubeBaseUrl }
        );

        services.AddHttpClient<SonarqubeService>(opts =>
        {
            opts.BaseAddress = new Uri(sonarqubeBaseUrl);

            opts.DefaultRequestHeaders.Add("Authorization", $"Bearer {sonarqubePat}");
        });

        services
            .AddConductorSharp(
                baseUrl: configuration.GetValue<string>("Conductor:BaseUrl")
                    ?? throw new InvalidOperationException(
                        "Conductor:BaseUrl configuration value not set."
                    )
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

        services.RegisterWorkerTask<GetProjectFileLocations.Handler>(opts =>
        {
            opts.Description = "Locate C# solution files in the repository";
        });
        services.RegisterWorkerTask<GetCodeMetrics.Handler>();
        services.RegisterWorkerTask<AnalyseCode.Handler>(opts =>
        {
            opts.TimeoutSeconds = opts.ResponseTimeoutSeconds = 600;
            opts.Description =
                "Perform code analysis using well known tools to obtain a list of files that need to be refactored";
        });
        services.RegisterWorkerTask<InitMetricsProject.Handler>();
        services.RegisterWorkerTask<TestBuild.Handler>(opts =>
        {
            opts.TimeoutSeconds = opts.ResponseTimeoutSeconds = 600;
            opts.Description = "Execute test build to ensure project compiles";
        });
    })
    .Build()
    .RunAsync();
