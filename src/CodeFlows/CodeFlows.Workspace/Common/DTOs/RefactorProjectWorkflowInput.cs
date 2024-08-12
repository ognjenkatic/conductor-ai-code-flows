using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFlows.Workspace.Common.DTOs
{
    public record RefactorProjectWorkflowInput(
        string RepositoryPath,
        string RepositoryName,
        string ProjectPath
    );
}
