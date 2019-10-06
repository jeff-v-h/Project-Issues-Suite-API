using ProjectIssuesSuite.API.domain.Managers;
using ProjectIssuesSuite.API.domain.Models;
using LazyCache;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectIssuesSuite.API.presentation.Controllers
{
    [Route("api/projects")]
    public class ProjectsController : Controller
    {
        private IProjectManager _manager { get; set; }
        private readonly IAppCache _cache;
        private TimeSpan _cacheExpiry { get; set; } = new TimeSpan(0, 0, 1);

        public ProjectsController(IProjectManager manager, IAppCache cache)
        {
            _manager = manager;
            _cache = cache;
        }

        [HttpGet("")]
        public IActionResult GetAllProjects()
        {
            Func<ICollection<ProjectViewModel>> projectsGetter = () => _manager.GetAllProjects();

            var projectsCached = _cache.GetOrAdd("ProjectsController.GetProjects", projectsGetter, _cacheExpiry);

            return Ok(projectsCached);
        }

        [HttpGet("{projectName}", Name = "GetProject")]
        public IActionResult GetProject(string projectName)
        {
            Func<ProjectViewModel> projectGetter = () => _manager.GetProject(projectName);

            ProjectViewModel projectCached = _cache.GetOrAdd(
                "ProjectsController.GetProject." + projectName,
                projectGetter,
                _cacheExpiry);

            if (projectCached == null)
            {
                return NotFound($"Project with name '{projectName}' was not found.");
            }

            return Ok(projectCached);
        }

        [HttpPost("")]
        public async Task<IActionResult> CreateProject([FromBody] ProjectViewModel project)
        {
            if (project == null)
            {
                return BadRequest("Please provide details to create a project.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // null is returned if the project was not created successfully
            ProjectViewModel projectVM = await _manager.CreateProject(project);
            if (projectVM == null)
            {
                return BadRequest($"The project name '{project.Name}' already exists.");
            }

            // Project is successfully created. Return the uri to the created project.
            return CreatedAtRoute("GetProject", new { projectName = projectVM.Name }, projectVM);
        }

        [HttpPost("{projectName}")]
        public IActionResult UpdateProject(string projectName, [FromBody] ProjectViewModel newProjectObject)
        {
            if (newProjectObject == null)
            {
                return BadRequest("Please provide details to update a project.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // replace the project with the new info above
            bool projectIsUpdated = _manager.ReplaceProject(projectName, newProjectObject);

            if (!projectIsUpdated)
            {
                return NotFound($"Project with name '{projectName}' was not found. No update was executed.");
            }

            return NoContent();
        }

        [HttpDelete("{projectName}")]
        public IActionResult DeleteProject(string projectName)
        {
            bool projectIsDeleted = _manager.DeleteProject(projectName);
            if (!projectIsDeleted)
            {
                return BadRequest($"Project '{projectName}' has tickets in it. No delete was executed.");
            }

            return NoContent();
        }
    }
}
