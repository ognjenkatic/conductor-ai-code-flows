using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Codeflows.Portal.DTOs;
using Codeflows.Portal.Infrastructure.Persistence;
using ConductorSharp.Client.Generated;
using ConductorSharp.Client.Service;

namespace Codeflows.Portal.Application.Services
{
    public class RefactorRunService
    {
        private readonly CodeflowsDbContext dbContext;
        private readonly IWorkflowService workflowService;
        private readonly Dictionary<string, string> runStageToTaskMap = new Dictionary<string, string>()
        {
            { "clone_repository", "Clone Repository" },
            { "detect_projects_csharp", "Detect Solutions" },
            { "analyse_code", "Code Quality Analysis" },
            { "refactor_project", "Refactor Project" },
            { "test_build", "Test Build" },
            { "create_pull_request", "Create pull request" },
            { "cleanup", "Cleanup" }
        };

        public RefactorRunService(CodeflowsDbContext dbContext, IWorkflowService workflowService)
        {
            this.dbContext = dbContext;
            this.workflowService = workflowService;
        }

        public async Task<List<RefactorRunTask>> GetRefactorRunTasks(int refactorRunId, CancellationToken cancellationToken)
        {
            var run = await dbContext.RefactorRuns.FindAsync(new object[] { refactorRunId }, cancellationToken)
                ?? throw new InvalidOperationException("Could not find refactor run by id");

            if (run.WorkflowId is null)
            {
                return new List<RefactorRunTask>();
            }

            return await GetTasks(run.WorkflowId, cancellationToken);
        }

        private async Task<List<RefactorRunTask>> GetTasks(string workflowId, CancellationToken cancellationToken)
        {
            var taskList = new List<RefactorRunTask>();

            var workflow = await workflowService.GetExecutionStatusAsync(workflowId, true, cancellationToken)
                ?? throw new InvalidOperationException("Could not find job by id");

            foreach (var task in workflow.Tasks)
            {
                if (task.WorkflowTask.Type == "SUB_WORKFLOW")
                {
                    var subtasks = await GetTasks(task.SubWorkflowId, cancellationToken);
                    taskList.AddRange(subtasks);
                }
                else if (task.WorkflowTask.Type == "SIMPLE" && runStageToTaskMap.TryGetValue(task.ReferenceTaskName, out var runTaskName))
                {
                    var durationSeconds = CalculateDurationSeconds(task);
                    taskList.Add(new RefactorRunTask
                    {
                        DurationSeconds = durationSeconds,
                        Name = runTaskName,
                        Status = MapTaskStatus(task.Status),
                        Description = task.WorkflowTask.TaskDefinition.Description
                    });
                }
            }

            return taskList;
        }

        private static long CalculateDurationSeconds(TaskResult task)
        {
            var startTime = task.StartTime ?? 0;
            var endTime = task.EndTime ?? 0;

            if (startTime > 0)
            {
                if (endTime == 0)
                {
                    var currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    return (currentTime - startTime) / 1000;
                }
                else
                {
                    return (endTime - startTime) / 1000;
                }
            }
            return 0;
        }

        private static RefactorRunTaskState MapTaskStatus(ConductorSharp.Client.Generated.TaskStatus status)
        {
            return status switch
            {
                ConductorSharp.Client.Generated.TaskStatus.SCHEDULED => RefactorRunTaskState.Scheduled,
                ConductorSharp.Client.Generated.TaskStatus.IN_PROGRESS => RefactorRunTaskState.InProgress,
                ConductorSharp.Client.Generated.TaskStatus.COMPLETED => RefactorRunTaskState.Completed,
                ConductorSharp.Client.Generated.TaskStatus.FAILED => RefactorRunTaskState.Failed,
                ConductorSharp.Client.Generated.TaskStatus.CANCELED => RefactorRunTaskState.Canceled,
                ConductorSharp.Client.Generated.TaskStatus.SKIPPED => RefactorRunTaskState.Skipped,
                ConductorSharp.Client.Generated.TaskStatus.COMPLETED_WITH_ERRORS => RefactorRunTaskState.CompletedWithErrors,
                ConductorSharp.Client.Generated.TaskStatus.TIMED_OUT => RefactorRunTaskState.TimedOut,
                ConductorSharp.Client.Generated.TaskStatus.FAILED_WITH_TERMINAL_ERROR => RefactorRunTaskState.FailedWithTerminalError,
                _ => RefactorRunTaskState.Unknown
            };
        }
    }
}