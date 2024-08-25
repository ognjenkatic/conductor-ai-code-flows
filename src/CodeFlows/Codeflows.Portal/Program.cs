using Codeflows.Portal.Application.Services;
using Codeflows.Portal.Application.Workers;
using Codeflows.Portal.Infrastructure.Persistence;
using ConductorSharp.Engine.Extensions;
using ConductorSharp.Engine.Health;
using Microsoft.EntityFrameworkCore;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder
    .Services.AddConductorSharp(
        baseUrl: builder.Configuration.GetValue<string>("Conductor:BaseUrl")
            ?? throw new InvalidOperationException("Conductor:BaseUrl configuration value not set.")
    )
    .AddExecutionManager(
        maxConcurrentWorkers: builder.Configuration.GetValue("Conductor:MaxConcurrentWorkers", 10),
        sleepInterval: builder.Configuration.GetValue("Conductor:SleepInterval", 500),
        longPollInterval: builder.Configuration.GetValue("Conductor:LongPollInterval", 100),
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

builder.Services.RegisterWorkerTask<UpdateRefactorRun.Handler>();

var whitelistedProjects =
    builder.Configuration.GetSection("Repo:Whitelist").Get<string[]>()
    ?? throw new InvalidOperationException("Repo:Whitelist configuration value not set.");

builder.Services.AddScoped<RefactorRunService>();
builder.Services.AddSingleton(new RepositoryWhitelist(whitelistedProjects));
builder.Services.AddDbContext<CodeflowsDbContext>(options =>
{
    var connectionString =
        builder.Configuration.GetConnectionString("Database")
        ?? throw new InvalidOperationException("Connection string not found.");

    options.UseNpgsql(connectionString);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

var scope = app.Services.CreateScope();

var dbContext = scope.ServiceProvider.GetRequiredService<CodeflowsDbContext>();

await dbContext.Database.MigrateAsync();

// TODO: Handle exceptions in some kind of middleware
await app.RunAsync();