using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFlows.Workspace.Common.DTOs
{
    public record class ProjectDetectionTaskInput(string RepositoryPath, string RepositoryName);
}
