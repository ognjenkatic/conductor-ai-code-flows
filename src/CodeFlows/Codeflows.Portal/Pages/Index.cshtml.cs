using Codeflows.Portal.Application.Services;
using CodeFlows.Portal.Application.Utils;
using Codeflows.Portal.DTOs;
using Codeflows.Portal.Infrastructure.Persistence;
using Codeflows.Portal.Infrastructure.Persistence.Entities;
using ConductorSharp.Client.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Codeflows.Portal.Pages
{
    public class IndexModel(
        ILogger<IndexModel> logger,
        CodeflowsDbContext dbContext,
        RepositoryWhitelist repositoryWhitelist,
        IWorkflowService workflowService,
        IMemoryCache memoryCache
    ) : PageModel
    {
        private readonly RateLimiter userRateLimiter =
            new(memoryCache, 5, TimeSpan.FromMinutes(15));
        private readonly RateLimiter globalRateLimiter =
            new(memoryCache, 20, TimeSpan.FromHours(1));

        private readonly ILogger<IndexModel> _logger = logger;
        private readonly CodeflowsDbContext _dbContext = dbContext;
        private readonly RepositoryWhitelist repositoryWhitelist = repositoryWhitelist;
        private static readonly RefactorRunState[] terminalStates =
        [
            RefactorRunState.Rejected,
            RefactorRunState.Success,
            RefactorRunState.Failure
        ];

        private const string ErrorMessageKey = "ErrorMessage";

        public List<RefactorRunDTO> RefactorRuns { get; set; } = [];
        public string? SelectedStatus { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string[] WhitelistedRepos { get; set; } = repositoryWhitelist.WhitelistedRepos;
        public string? ErrorMessage { get; set; } // Property to hold the error message

        public async Task OnGet(
            int pageNumber = 1,
            int pageSize = 10,
            string? statusFilter = null,
            CancellationToken cancellationToken = default
        )
        {
            if (TempData.ContainsKey(ErrorMessageKey))
            {
                ErrorMessage = TempData[ErrorMessageKey] as string;
            }

            RefactorRunState? refactorRunState = null;

            if (Enum.TryParse<RefactorRunState>(statusFilter, out var parsedState))
            {
                refactorRunState = parsedState;
                SelectedStatus = parsedState.ToString();
            }
            else
            {
                SelectedStatus = "All";
            }

            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, 25);

            var totalCount = await _dbContext.RefactorRuns.CountAsync(cancellationToken);

            RefactorRuns = await _dbContext
                .RefactorRuns.OrderByDescending(rr => rr.Id)
                .Where(rr => refactorRunState == null || rr.State == refactorRunState)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(rr => new RefactorRunDTO
                {
                    Id = rr.Id,
                    WorkflowId = rr.WorkflowId,
                    RepositoryUrl = rr.RepositoryUrl,
                    State = rr.State.ToString(),
                    Note = rr.Note
                })
                .ToListAsync(cancellationToken);

            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        }

        public async Task<IActionResult> OnPostSubmitUrl(
            string projectUrl,
            CancellationToken cancellationToken = default
        )
        {
            var clientIp = HttpContext.Connection?.RemoteIpAddress?.ToString() ?? "weird-anon-user";

            var userRateLimitKey = $"RateLimit-{clientIp}";
            var globalRateLimitKey = $"GlobalRateLimit";

            if (!userRateLimiter.IsRequestAllowed(userRateLimitKey))
            {
                TempData[ErrorMessageKey] =
                    "You have exceeded the rate limit. Please try again later.";
                return RedirectToPage();
            }
            else if (!globalRateLimiter.IsRequestAllowed(globalRateLimitKey))
            {
                TempData[ErrorMessageKey] =
                    "Users have exceeded the rate limit. Please try again later.";
                return RedirectToPage();
            }

            var currentRepositoryRuns = await _dbContext
                .RefactorRuns
                .Where(r => r.RepositoryUrl == projectUrl && !terminalStates.Contains(r.State))
                .FirstOrDefaultAsync(cancellationToken);

            if (currentRepositoryRuns is not null)
            {
                TempData[ErrorMessageKey] =
                    $"There is already a refactor job with ID {currentRepositoryRuns.Id} running for this repository.";
                return RedirectToPage(); // Return to the same page with the error message
            }
            else if (!repositoryWhitelist.IsRepositoryWhitelisted(projectUrl))
            {
                TempData[ErrorMessageKey] = $"The repository is not whitelisted";
                return RedirectToPage(); // Return to the same page with the error message
            }

            var refactorRun = new RefactorRun() { RepositoryUrl = projectUrl };
            _dbContext.RefactorRuns.Add(refactorRun);

            refactorRun.State = RefactorRunState.Scheduled;

            await _dbContext.SaveChangesAsync(cancellationToken);

            try
            {
                refactorRun.WorkflowId = await workflowService.StartAsync(
                    new()
                    {
                        Version = 1,
                        Name = "refactor_repository",
                        Input = new Dictionary<string, object>
                        {
                            { "repository_url", refactorRun.RepositoryUrl },
                            { "refactor_run_id", refactorRun.Id },
                            { "repository_path", StringUtils.GetRandomString(32) }
                        }
                    },
                    cancellationToken
                );

                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not start workflow due to {exception}");
                refactorRun.State = RefactorRunState.Rejected;
                refactorRun.Note = "Job could not be started";
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            return RedirectToPage("Run", new { runId = refactorRun.Id });
        }
    }
}