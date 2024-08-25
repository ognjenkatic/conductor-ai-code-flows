using CodeFlows.Workspace.Github.Workers;
using CodeFlows.Workspace.Util.Workers;
using ConductorSharp.Engine.Extensions;
using ConductorSharp.Engine.Health;
using LibGit2Sharp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Octokit;
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

        var ghToken =
            configuration.GetValue<string>("GH_TOKEN")
            ?? throw new InvalidOperationException("GH_TOKEN value not set.");

        services.AddScoped(ctx => new GitHubClient(new ProductHeaderValue("Codebot"))
        {
            Credentials = new Octokit.Credentials(ghToken)
        });

        services.AddSingleton(
            new UsernamePasswordCredentials()
            {
                Username =
                    configuration.GetValue<string>("GH_USERNAME")
                    ?? throw new InvalidOperationException(
                        "GH_USERNAME configuration value not set."
                    ),
                Password = ghToken
            }
        );
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

        services.RegisterWorkerTask<CloneProject.Handler>(opts =>
        {
            opts.RetryLogic = ConductorSharp.Client.Generated.TaskDefRetryLogic.FIXED;
            opts.RetryCount = 5;
            opts.Description =
                "Clones a GitHub project repository to the local workspace (NOTE: There is a GitHub auth bug in this handler but for the purpose of demonstrating retries it was left in)";
        });
        services.RegisterWorkerTask<ForkProjectDetection.Handler>();
        services.RegisterWorkerTask<ForkProjectAnalysis.Handler>();
        services.RegisterWorkerTask<CommitProjectChanges.Handler>(opts =>
        {
            opts.RetryLogic = ConductorSharp.Client.Generated.TaskDefRetryLogic.FIXED;
            opts.RetryCount = 5;
            opts.Description =
                "(NOTE: There is a GitHub auth bug in this handler but for the purpose of demonstrating retries it was left in)";
        });
        services.RegisterWorkerTask<CheckForPullRequest.Handler>(opts =>
        {
            opts.RetryLogic = ConductorSharp.Client.Generated.TaskDefRetryLogic.FIXED;
            opts.RetryCount = 5;
            opts.Description =
                "(NOTE: There is a GitHub auth bug in this handler but for the purpose of demonstrating retries it was left in)";
        });
        services.RegisterWorkerTask<CreatePullRequest.Handler>(opts =>
        {
            opts.RetryLogic = ConductorSharp.Client.Generated.TaskDefRetryLogic.FIXED;
            opts.RetryCount = 5;
            opts.Description =
                "Creates a pull request with code changes to the GitHub project repository";
        });
        services.RegisterWorkerTask<Cleanup.Handler>(opts =>
        {
            opts.Description = "Clean up local workspace by deleting the cloned repository";
        });
    })
    .Build()
    .RunAsync();
