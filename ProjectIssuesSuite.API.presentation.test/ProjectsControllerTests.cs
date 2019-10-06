using ProjectIssuesSuite.API.data.Models;
using ProjectIssuesSuite.API.domain.Managers;
using ProjectIssuesSuite.API.domain.Models;
using LazyCache.Mocks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using Xunit;
using ProjectIssuesSuite.API.presentation.Controllers;

namespace ProjectIssuesSuite.API.presentation.test
{
    public class ProjectsControllerTests
    {
        private readonly Mock<IProjectManager> _mockManager;
        private readonly MockCachingService _mockCache;
        private ProjectsController _sut;

        public ProjectsControllerTests()
        {
            // Re-usable Arrange here via DI (from Arrange, Act, Assert)
            _mockManager = new Mock<IProjectManager>();
            _mockCache = new MockCachingService();
            // sut = system under test            
            _sut = new ProjectsController(_mockManager.Object, _mockCache);
        }

        [Fact]
        public void GetProjects_ReturnsOk()
        {
            // Arrange
            //Setup the GetProjectsViaManager method in the mock manager to return a list
            // The mock manager has already been passed into the controller in the constructor above
            ICollection<ProjectViewModel> listProjects = new List<ProjectViewModel>();
            _mockManager.Setup(x => x.GetAllProjects()).Returns(listProjects);

            // Act (on the method and system wanting to test)
            IActionResult result = _sut.GetAllProjects();

            // Assert
            // Make sure the returned result is a 200 OK
            ObjectResult objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<List<ProjectViewModel>>(objectResult.Value);
        }

        [Theory]
        [InlineData("NOT-A-PROJECT")]
        [InlineData(" ")]
        [InlineData("HI")]
        public void GetProject_NotFound(string projectName)
        {
            IActionResult result = _sut.GetProject(projectName);

            ObjectResult notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal($"Project with name '{projectName}' was not found.",
                notFoundResult.Value);
        }

        [Theory]
        [InlineData("ATP")]
        [InlineData("BT")]
        public void GetProject_ReturnsOk(string projectName)
        {
            ProjectViewModel projectVM = new ProjectViewModel();
            _mockManager.Setup(x => x.GetProject(It.IsAny<string>()))
                .Returns(projectVM);

            IActionResult result = _sut.GetProject(projectName);

            ObjectResult objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<ProjectViewModel>(objectResult.Value);
        }

        [Fact]
        public async void CreateProject_BadRequest_NullPost()
        {
            _sut.ModelState.AddModelError("x", "Test Error");
            ProjectViewModel project = null;

            IActionResult result = await _sut.CreateProject(project);

            ObjectResult badReqResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal($"Please provide details to create a project.", badReqResult.Value);
            // Ensure CreateProjectViaManager is never run since it should no be reached
            _mockManager.Verify(x => x.CreateProject(
                It.IsAny<ProjectViewModel>()), Times.Never);
        }

        [Fact]
        public async void CreateProject_BadRequest_InvalidModelState()
        {
            // Arrange what the manager will return
            _sut.ModelState.AddModelError("x", "Test Error");

            // Arrange what to put into the method that is being tested
            var project = new ProjectViewModel();

            // Act
            IActionResult result = await _sut.CreateProject(project);

            ObjectResult badReqResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badReqResult.Value);
            _mockManager.Verify(x => x.CreateProject(
                It.IsAny<ProjectViewModel>()), Times.Never);
        }

        [Fact]
        public async void CreateProject_BadRequest_ManagerReturnsNull()
        {
            // Arrange what the manager will return
            ProjectViewModel projectVM = null;
            _mockManager.Setup(x => x.CreateProject(
                It.IsAny<ProjectViewModel>())).ReturnsAsync(projectVM);

            IActionResult result = await _sut.CreateProject(
                new ProjectViewModel { Name = "PGA" });

            ObjectResult errorResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<string>(errorResult.Value);
        }

        [Theory]
        [InlineData("59053e27-e0ca-4cd7-9a88-4264560de206", "PGA")]
        [InlineData("a4c723d7-dd66-4cbe-87a3-2868b8133aa2", "FIFA", "7d47ce6a-fe9f-4bd6-adb8-85a5efceb1df", "Bug")]
        public async void CreateProject_ReturnsCreatedRoute(string id,
            string name, string ticketId = null, string ticketName = null)
        {
            // Arrange what the manager will return
            // Name is required in below project models
            ProjectViewModel projectVM = new ProjectViewModel()
            {
                Id = id,
                Name = name,
                Tickets = new List<TicketBase>()
                {
                    new TicketBase()
                    {
                        Id = ticketId,
                        Name = ticketName
                    }
                }
            };
            _mockManager.Setup(x => x.CreateProject(
                It.IsAny<ProjectViewModel>())).ReturnsAsync(projectVM);

            // Make a new dto object to pass in. Name is a required property
            var projectDto = new ProjectViewModel() { Name = projectVM.Name };
            IActionResult result = await _sut.CreateProject(projectDto);

            // Ensure the manager method executes once
            _mockManager.Verify(x => x.CreateProject(
                It.IsAny<ProjectViewModel>()), Times.Once);

            var createdResult = Assert.IsType<CreatedAtRouteResult>(result);
            Assert.Same(projectVM, createdResult.Value);
            Assert.Equal("GetProject", createdResult.RouteName);
            // The RouteValues refers to the key/value pairs passed into the CreatedAtRoute method
            // (not the properties of the objects above)
            Assert.Equal(projectVM.Name, createdResult.RouteValues["projectName"]);
        }

        [Fact]
        public void UpdateProject_BadRequest_NullPost()
        {
            _sut.ModelState.AddModelError("x", "Test Error");
            ProjectViewModel project = null;

            IActionResult result = _sut.UpdateProject("ATP", project);

            ObjectResult badReqResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal($"Please provide details to update a project.", badReqResult.Value);
            // Ensure ReplaceProjectViaManager is never run since it should no be reached
            _mockManager.Verify(x => x.ReplaceProject(
                It.IsAny<string>(), It.IsAny<ProjectViewModel>()), Times.Never);
        }

        [Fact]
        public void UpdateProject_BadRequest_InvalidModelState()
        {
            _sut.ModelState.AddModelError("x", "Test Error");

            var project = new ProjectViewModel { Name = "Updated - Nitto ATP Finals" };
            IActionResult result = _sut.UpdateProject("ATP", project);

            ObjectResult badReqResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badReqResult.Value);
            _mockManager.Verify(x => x.ReplaceProject(
                It.IsAny<string>(), It.IsAny<ProjectViewModel>()), Times.Never);
        }

        // Have inline data here since NotFound returns  and Id which we want to test
        [Theory]
        [InlineData(" ")]
        [InlineData("No-Project")]
        public void UpdateProject_NotFound_NotUpdated(string projectName)
        {
            _mockManager.Setup(x => x.ReplaceProject(
                It.IsAny<string>(), It.IsAny<ProjectViewModel>())).Returns(false);

            ProjectViewModel project = new ProjectViewModel
            {
                Name = "Updated - Logout Bug"
            };
            IActionResult result = _sut.UpdateProject(projectName, project);

            _mockManager.Verify(x => x.ReplaceProject(
                It.IsAny<string>(), It.IsAny<ProjectViewModel>()), Times.Once);
            ObjectResult notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal($"Project with name '{projectName}' was not found. No update was executed.",
                notFoundResult.Value);
        }

        [Theory]
        [InlineData("59053e27-e0ca-4cd7-9a88-4264560de206", "ATP",
            "db853e6f-113d-47d3-af18-d4ea3e1b0e57", "Some Bug")]
        [InlineData("a4c723d7-dd66-4cbe-87a3-2868b8133aa2", "BT")]
        public void UpdateProject_ReturnsNoContent(string id,
            string name, string ticketId = null, string ticketName = null)
        {
            _mockManager.Setup(x => x.ReplaceProject(
                It.IsAny<string>(), It.IsAny<ProjectViewModel>())).Returns(true);

            var projectDto = new ProjectViewModel()
            {
                Name = name,
                Tickets = new List<TicketBase>()
                {
                    new TicketBase()
                    {
                        Id = ticketId,
                        Name = ticketName
                    }
                }
            };
            IActionResult result = _sut.UpdateProject(id, projectDto);

            _mockManager.Verify(x => x.ReplaceProject(
               It.IsAny<string>(), It.IsAny<ProjectViewModel>()), Times.Once);

            Assert.IsType<NoContentResult>(result);
        }

        [Theory]
        [InlineData(" ")]
        [InlineData("No-Project")]
        public void DeleteProject_BadRequest_NotDeleted(string projectName)
        {
            _mockManager.Setup(x => x.DeleteProject(
                It.IsAny<string>())).Returns(false);

            IActionResult result = _sut.DeleteProject(projectName);

            ObjectResult notFoundResult = Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void DeleteProject_ReturnsNoContent()
        {
            string anyString = It.IsAny<string>();
            _mockManager.Setup(x => x.DeleteProject(anyString)).Returns(true);

            IActionResult result = _sut.DeleteProject(anyString);

            _mockManager.Verify(x => x.DeleteProject(anyString), Times.Once);

            Assert.IsType<NoContentResult>(result);
        }
    }
}
