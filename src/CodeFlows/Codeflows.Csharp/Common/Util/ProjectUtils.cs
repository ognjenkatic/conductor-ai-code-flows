using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Codeflows.Csharp.Common.Util
{
    public class ProjectUtils
    {
        public static List<string> GetTopLevelProjects(IEnumerable<string> projectFiles)
        {
            var projectReferences = new Dictionary<string, HashSet<string>>();

            // Evaluate each project file in the provided list
            foreach (var projectFile in projectFiles)
            {
                EvaluateProject(projectFile, projectReferences);
            }

            // Find top-level projects (those that are not referenced by any other projects)
            var topLevelProjects = projectReferences
                .Keys.Where(proj =>
                    !projectReferences.Values.Any(references => references.Contains(proj))
                )
                .ToList();

            return topLevelProjects;
        }

        private static void EvaluateProject(
            string projectFile,
            Dictionary<string, HashSet<string>> projectReferences
        )
        {
            if (!File.Exists(projectFile))
                return;

            var projectDir = Path.GetDirectoryName(projectFile);
            var doc = XDocument.Load(projectFile);

            var projectName = Path.GetFileNameWithoutExtension(projectFile);
            if (!projectReferences.ContainsKey(projectName))
            {
                projectReferences[projectName] = new HashSet<string>();
            }

            var references = doc.Descendants("ProjectReference")
                .Select(pr => pr.Attribute("Include")?.Value)
                .Where(path => !string.IsNullOrEmpty(path))
                .Select(path => Path.GetFullPath(Path.Combine(projectDir, path)))
                .ToList();

            foreach (var reference in references)
            {
                var referencedProjectName = Path.GetFileNameWithoutExtension(reference);
                projectReferences[projectName].Add(referencedProjectName);

                // Recursively evaluate referenced projects
                EvaluateProject(reference, projectReferences);
            }
        }
    }
}
