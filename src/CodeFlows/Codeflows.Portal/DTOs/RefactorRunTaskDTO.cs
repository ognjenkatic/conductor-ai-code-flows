using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace Codeflows.Portal.DTOs
{
    public enum RefactorRunTaskState
    {
        Unknown,
        Scheduled,
        InProgress,
        TimedOut,
        Skipped,
        Canceled,
        Failed,
        FailedWithTerminalError,
        CompletedWithErrors,
        Completed
    }

    public class RefactorRunTask
    {
        public required string Name { get; set; }
        public required RefactorRunTaskState Status { get; set; }
        public required long DurationSeconds { get; set; }
        public string? Description { get; set; }
    }
}
