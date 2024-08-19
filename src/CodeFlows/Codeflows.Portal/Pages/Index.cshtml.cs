using Codeflows.Portal.Application.Services;
using CodeFlows.Portal.Application.Utils;
using Codeflows.Portal.DTOs;
using Codeflows.Portal.Infrastructure.Persistence;
using Codeflows.Portal.Infrastructure.Persistence.Entities;
using ConductorSharp.Client.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace Codeflows.Portal.Pages
{
    public class IndexModel(
        ILogger<IndexModel> logger,
        CodeflowsDbContext dbContext,
        RepositoryWhitelist repositoryWhitelist,
        IWorkflowService workflowService
    ) : PageModel
    {
        private readonly ILogger<IndexModel> _logger = logger;
        private readonly CodeflowsDbContext _dbContext = dbContext;
        private readonly RepositoryWhitelist repositoryWhitelist = repositoryWhitelist;

        private static readonly RefactorRunState[] terminalStates =
        [
            RefactorRunState.Rejected,
            RefactorRunState.Success,
            RefactorRunState.Failure
        ];

        public List<RefactorRunDTO> RefactorRuns { get; set; } = [];
        public string SelectedStatus { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string[] WhitelistedRepos { get; set; } = RepositoryWhitelist.WhitelistedRepos;

        public async Task OnGet(
            int pageNumber = 1,
            int pageSize = 10,
            string? statusFilter = null,
            CancellationToken cancellationToken = default
        )
        {
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

            var response = new List<RefactorRunDTO>();

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
            var refactorRun = new RefactorRun() { RepositoryUrl = projectUrl };
            _dbContext.RefactorRuns.Add(refactorRun);

            if (!repositoryWhitelist.IsRepositoryWhitelisted(projectUrl))
            {
                refactorRun.State = RefactorRunState.Rejected;
                await _dbContext.SaveChangesAsync(cancellationToken);
                return RedirectToPage();
            }

            var currentRepositoryRuns = _dbContext
                .RefactorRuns.Where(r =>
                    r.RepositoryUrl == projectUrl && !terminalStates.Contains(r.State)
                )
                .FirstOrDefault();

            if (currentRepositoryRuns is not null)
            {
                refactorRun.State = RefactorRunState.Rejected;
                refactorRun.Note =
                    $"There is a refactor job with id {currentRepositoryRuns.Id} already running for this repository";
                await _dbContext.SaveChangesAsync(cancellationToken);
                throw new InvalidOperationException(
                    "There is a refactor job already running for this repository"
                );
            }

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
                _logger.LogError("Could not start workflow due to {exception}", ex);
                refactorRun.State = RefactorRunState.Rejected;
                refactorRun.Note = "Job could not be started";
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            return RedirectToPage("Run", new { runId = refactorRun.Id });
        }
    }
}
