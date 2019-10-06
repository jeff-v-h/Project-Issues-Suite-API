using ProjectIssuesSuite.API.data.Models;
using ProjectIssuesSuite.API.data.Repositories;
using ProjectIssuesSuite.API.domain.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectIssuesSuite.API.domain.Managers
{
    public class ProjectManager : IProjectManager
    {
        private IProjectRepository _projectRepo;
        private readonly ILogger<ProjectManager> _logger;

        public ProjectManager(IProjectRepository projectRepository, ILogger<ProjectManager> logger)
        {
            _projectRepo = projectRepository;
            _logger = logger;
        }

        public ICollection<ProjectViewModel> GetAllProjects()
        {
            var projects = _projectRepo.GetAll().ToList();

            if (projects.Count() < 1)
            {
                _logger.LogInformation("\tNo projects were were returned from DB.");
            }

            var projectVMs = new List<ProjectViewModel>();

            foreach (Project project in projects)
            {
                projectVMs.Add(new ProjectViewModel(project));
            }

            return projectVMs;
        }

        public ProjectViewModel GetProject(string projectName)
        {
            var project = _projectRepo.GetByName(projectName);

            if (project == null)
            {
                return null;
            }

            return new ProjectViewModel(project);
        }

        public async Task<ProjectViewModel> CreateProject(ProjectViewModel newProject)
        {
            // Ensure the project name is unique
            if (_projectRepo.GetByName(newProject.Name) != null)
            {
                _logger.LogError($"\tProject with name '{newProject.Name}' already exists in the DB");
                return null;
            }

            // Pass in the property values into a new Project and add it into the Db via the repo.
            var project = new Project
            {
                Name = newProject.Name,
                Tickets = newProject.Tickets
            };

            // create the project in the db and pass the new id into the view model
            await _projectRepo.Create(project);
            newProject.Id = project.Id;

            // A ViewModel needs to be returned for Controller's CreatedAtRoute return method
            return newProject;
        }

        public bool ReplaceProject(string projectName, ProjectViewModel newProjectObject)
        {
            // check to see if the there is a project with this name
            var projectToUpdate = _projectRepo.GetByName(projectName);

            if (projectToUpdate == null)
            {
                _logger.LogError($"\tProject with name {projectName} was not found in the DB. Nothing was updated.");
                return false;
            }

            // Partial update is not supported in CosmosDB at this current moment
            // To allow for null values, the object's properties not intending to be changed
            // should still pass in the original value (whether in front end or via earlier before here)
            projectToUpdate.Name = newProjectObject.Name;
            projectToUpdate.Tickets = newProjectObject.Tickets;

            _projectRepo.Update(projectToUpdate).Wait();

            return true;
        }

        public bool DeleteProject(string projectName)
        {
            var projectToDelete = _projectRepo.GetByName(projectName);

            if (projectToDelete == null)
            {
                _logger.LogError($"\tNo project with name '{projectName}' was found to be deleted");
                return false;
            }

            if (projectToDelete.Tickets.Count > 0)
            {
                _logger.LogError($"\tProject has ticket's in it. Delete not allowed.");
                return false;
            }

            _projectRepo.Delete(projectToDelete.Id).Wait();

            return true;
        }
    }
}
