using Codeflows.Portal.Application.Services;
using Codeflows.Portal.DTOs;
using Codeflows.Portal.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Codeflows.Portal.Pages
{
    public class RunModel : PageModel
    {
        private readonly CodeflowsDbContext dbContext;
        private readonly RefactorRunService refactorRunService;

        public RunModel(CodeflowsDbContext dbContext, RefactorRunService refactorRunService)
        {
            this.dbContext = dbContext;
            this.refactorRunService = refactorRunService;
        }

        public List<RefactorRunTask> RefactorRunTasks { get; set; } = new List<RefactorRunTask>();

        public RefactorRunDTO RefactorRun { get; set; } = new RefactorRunDTO();

        public string GetStatusBadgeClass(RefactorRunTaskState status)
        {
            return status switch
            {
                RefactorRunTaskState.Completed => "badge-success",
                RefactorRunTaskState.InProgress => "badge-warning",
                RefactorRunTaskState.Failed => "badge-danger",
                RefactorRunTaskState.FailedWithTerminalError => "badge-danger",
                _ => "badge-secondary",
            };
        }

        public async Task<IActionResult> OnGetAsync(
            int runId,
            CancellationToken cancellationToken = default
        )
        {
            var refactorRun = await dbContext.RefactorRuns.FindAsync(
                new object[] { runId },
                cancellationToken
            );

            if (refactorRun is null)
            {
                return RedirectToPage("Index");
            }

            RefactorRun = new()
            {
                RepositoryUrl = refactorRun.RepositoryUrl,
                Id = refactorRun.Id,
                Note = refactorRun.Note,
                State = refactorRun.State.ToString(),
                WorkflowId = refactorRun.WorkflowId,
                PullRequestUrl = refactorRun.PullRequestUrl
            };

            try
            {
                RefactorRunTasks = await refactorRunService.GetRefactorRunTasks(
                    refactorRun.Id,
                    cancellationToken
                );

                return Page();
            }
            catch (Exception)
            {
                return RedirectToPage("Index");
            }
        }
    }
}