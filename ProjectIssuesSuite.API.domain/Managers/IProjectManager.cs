using ProjectIssuesSuite.API.domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectIssuesSuite.API.domain.Managers
{
    public interface IProjectManager
    {
        ICollection<ProjectViewModel> GetAllProjects();
        ProjectViewModel GetProject(string projectName);
        Task<ProjectViewModel> CreateProject(ProjectViewModel newProject);
        bool ReplaceProject(string projectName, ProjectViewModel newProjectObject);
        bool DeleteProject(string projectName);
    }
}
