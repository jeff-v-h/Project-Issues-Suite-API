using ProjectIssuesSuite.API.data.Models;
using ProjectIssuesSuite.API.data.Repositories;
using ProjectIssuesSuite.API.domain.Managers;
using ProjectIssuesSuite.API.domain.Models;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ProjectIssuesSuite.API.domain.test
{
    public class ProjectManagerTests
    {
        private readonly Mock<IProjectRepository> _mockProjectRepo;
        private ProjectManager _sut;

        public ProjectManagerTests()
        {
            _mockProjectRepo = new Mock<IProjectRepository>();
            // Mock.Of is more concise way of making a mock
            var mockManagerLogger = Mock.Of<ILogger<ProjectManager>>();
            // Alternative (more explicit way) to mocking the Ilogger
            //var mockLogger = new Mock<ILogger<ProjectManager>>();
            //ILogger<ProjectManager> logger = mockLogger.Object;

            _sut = new ProjectManager(_mockProjectRepo.Object, mockManagerLogger);
        }

        [Fact]
        public void GetProjects_ReturnsViewModels_OnOk()
        {
            ICollection<Project> listProjects = new List<Project>();
            listProjects.Add(
                new Project
                {
                    Name = It.IsAny<string>()
                });
            listProjects.Add(
                new Project
                {
                    Name = It.IsAny<string>()
                });
            _mockProjectRepo.Setup(x => x.GetAll()).Returns(listProjects.AsQueryable());

            ICollection<ProjectViewModel> result = _sut.GetAllProjects();

            Assert.NotNull(result);
            Assert.IsType<List<ProjectViewModel>>(result);
        }

        [Fact]
        public void GetProjects_ReturnsEmptyObject_CollectionHasNoDocs()
        {
            ICollection<Project> listProjects = new List<Project>();
            _mockProjectRepo.Setup(x => x.GetAll()).Returns(listProjects.AsQueryable());

            ICollection<ProjectViewModel> result = _sut.GetAllProjects();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void GetProject_ReturnsNull_RepoReturnsNull()
        {
            Project project = null;
            _mockProjectRepo.Setup(x => x.GetByName(It.IsAny<string>())).Returns(project);

            ProjectViewModel result = _sut.GetProject(It.IsAny<string>());

            Assert.Null(result);
        }

        [Fact]
        public void GetProject_ReturnsViewModel_OnOk()
        {
            Project project = new Project();
            _mockProjectRepo.Setup(x => x.GetByName(It.IsAny<string>())).Returns(project);

            ProjectViewModel result = _sut.GetProject(It.IsAny<string>());

            Assert.NotNull(result);
            Assert.IsType<ProjectViewModel>(result);
        }

        [Fact]
        public async void CreateProject_ReturnsNull_NameAlreadyExists()
        {
            Project project = new Project();
            _mockProjectRepo.Setup(x => x.GetByName(It.IsAny<string>())).Returns(project);

            ProjectViewModel result = await _sut.CreateProject(new ProjectViewModel());

            _mockProjectRepo.Verify(x => x.GetByName(It.IsAny<string>()), Times.Once);
            _mockProjectRepo.Verify(x => x.Create(It.IsAny<Project>()), Times.Never);
            Assert.Null(result);
        }

        [Fact]
        public async void CreateProject_ReturnsViewModel()
        {
            Project nullProject = null;
            _mockProjectRepo.Setup(x => x.GetByName(It.IsAny<string>())).Returns(nullProject);
            var project = new Project { Name = "test name" };
            _mockProjectRepo.Setup(x => x.Create(It.IsAny<Project>())).ReturnsAsync(project);
            var projectVM = new ProjectViewModel { Name = "test name" };

            ProjectViewModel actual = await _sut.CreateProject(projectVM);

            _mockProjectRepo.Verify(x => x.GetByName(It.IsAny<string>()), Times.Once);
            _mockProjectRepo.Verify(x => x.Create(It.IsAny<Project>()), Times.Once);
            Assert.IsType<ProjectViewModel>(actual);
            Assert.Equal(project.Name, actual.Name);
        }

        [Fact]
        public void ReplaceProject_ReturnsFalse_WhenProjectDoesNotExist()
        {
            Project project = null;
            _mockProjectRepo.Setup(x => x.GetByName(It.IsAny<string>())).Returns(project);

            var result = _sut.ReplaceProject(It.IsAny<string>(), new ProjectViewModel());

            _mockProjectRepo.Verify(x => x.GetByName(It.IsAny<string>()), Times.Once);
            _mockProjectRepo.Verify(x => x.Update(It.IsAny<Project>()), Times.Never);
            Assert.False(result);
        }

        [Fact]
        public void ReplaceProject_ReturnsTrue_OnUpdate()
        {
            _mockProjectRepo.Setup(x => x.GetByName(It.IsAny<string>())).Returns(new Project());

            var result = _sut.ReplaceProject(It.IsAny<string>(), new ProjectViewModel());

            _mockProjectRepo.Verify(x => x.GetByName(It.IsAny<string>()), Times.Once);
            _mockProjectRepo.Verify(x => x.Update(It.IsAny<Project>()), Times.Once);
            Assert.True(result);
        }

        [Fact]
        public void DeleteProject_ReturnsFalse_WhenProjectDoesNotExist()
        {
            Project project = null;
            _mockProjectRepo.Setup(x => x.GetByName(It.IsAny<string>())).Returns(project);

            var result = _sut.DeleteProject(It.IsAny<string>());

            _mockProjectRepo.Verify(x => x.GetByName(It.IsAny<string>()), Times.Once);
            _mockProjectRepo.Verify(x => x.Delete(It.IsAny<string>()), Times.Never);
            Assert.False(result);
        }

        [Fact]
        public void DeleteProject_ReturnsTrue_OnDelete()
        {
            _mockProjectRepo.Setup(x => x.GetByName(It.IsAny<string>())).Returns(new Project());

            var result = _sut.DeleteProject(It.IsAny<string>());

            _mockProjectRepo.Verify(x => x.GetByName(It.IsAny<string>()), Times.Once);
            _mockProjectRepo.Verify(x => x.Delete(It.IsAny<string>()), Times.Once);
            Assert.True(result);
        }
    }
}
