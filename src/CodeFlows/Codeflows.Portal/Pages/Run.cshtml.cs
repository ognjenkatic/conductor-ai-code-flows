using Codeflows.Portal.Application.Services;
using Codeflows.Portal.DTOs;
using Codeflows.Portal.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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

        public List<RefactorRunTask> RefactorRunTasks { get; set; } = [];

        public async Task OnGet(int runId, CancellationToken cancellationToken = default)
        {
            var refactorRun = await dbContext.RefactorRuns.FindAsync([runId], cancellationToken);

            if (refactorRun is null)
            {
                RedirectToPage("Index");
                return;
            }

            RefactorRunTasks = await refactorRunService.GetRefactorRunTasks(
                refactorRun.Id,
                cancellationToken
            );

            return;
        }
    }
}
