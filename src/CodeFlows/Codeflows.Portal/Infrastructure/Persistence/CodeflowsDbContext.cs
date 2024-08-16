using Codeflows.Portal.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Codeflows.Portal.Infrastructure.Persistence
{
    public class CodeflowsDbContext : DbContext
    {
        public CodeflowsDbContext(DbContextOptions<CodeflowsDbContext> options)
            : base(options) { }

        public DbSet<RefactorRun> RefactorRuns { get; set; }
    }
}
