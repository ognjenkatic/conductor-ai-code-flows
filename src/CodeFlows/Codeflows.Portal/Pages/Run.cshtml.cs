using Codeflows.Portal.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Codeflows.Portal.Pages
{
    public class RunModel : PageModel
    {
        private readonly CodeflowsDbContext dbContext;

        public RunModel(CodeflowsDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task OnGet(int runId, CancellationToken cancellationToken = default)
        {
            var refactorRun = await dbContext.RefactorRuns.FindAsync([runId], cancellationToken);
        }
    }
}
