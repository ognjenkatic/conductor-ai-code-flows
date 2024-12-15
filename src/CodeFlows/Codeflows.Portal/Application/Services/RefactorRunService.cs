using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Codeflows.Portal.DTOs;
using Codeflows.Portal.Infrastructure.Persistence;
using Codeflows.Portal.Infrastructure.Persistence.Entities;
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
                    continue;
                }

                if (task.WorkflowTask.Type == "SIMPLE" && runStageToTaskMap.TryGetValue(task.ReferenceTaskName, out var runTaskName))
                {
                    var durationSeconds = CalculateDurationSeconds(task.StartTime, task.EndTime);

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

        private long CalculateDurationSeconds(long? startTime, long? endTime)
        {
            if (startTime == null || startTime <= 0)
            {
                return 0;
            }

            if (endTime == null || endTime == 0)
            {
                var currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                return (currentTime - startTime.Value) / 1000;
            }

            return (endTime.Value - startTime.Value) / 1000;
        }

        private RefactorRunTaskState MapTaskStatus(TaskStatus status)
        {
            return status switch
            {
                TaskStatus.SCHEDULED => RefactorRunTaskState.Scheduled,
                TaskStatus.IN_PROGRESS => RefactorRunTaskState.InProgress,
                TaskStatus.COMPLETED => RefactorRunTaskState.Completed,
                TaskStatus.FAILED => RefactorRunTaskState.Failed,
                TaskStatus.CANCELED => RefactorRunTaskState.Canceled,
                TaskStatus.SKIPPED => RefactorRunTaskState.Skipped,
                TaskStatus.COMPLETED_WITH_ERRORS => RefactorRunTaskState.CompletedWithErrors,
                TaskStatus.TIMED_OUT => RefactorRunTaskState.TimedOut,
                TaskStatus.FAILED_WITH_TERMINAL_ERROR => RefactorRunTaskState.FailedWithTerminalError,
                _ => RefactorRunTaskState.Unknown
            };
        }
    }
}