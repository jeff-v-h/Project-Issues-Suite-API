using ProjectIssuesSuite.API.common.Models;
using ProjectIssuesSuite.API.data.Models;
using ProjectIssuesSuite.API.data.Repositories;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace ProjectIssuesSuite.API.data.test
{
    public class ProjectRepositoryTests
    {
        private ProjectRepository _sut;
        private readonly Mock<IDocumentClient> _mockClient;
        private readonly ITestOutputHelper _output;

        public ProjectRepositoryTests(ITestOutputHelper output)
        {
            _output = output;
            // This method manually create a populated IOptions object and creates a wrapper around 
            // an instance of TOptions to return itself as IOptions
            var dbSettings = new DbSettings()
            {
                EndpointUri = "https://localhost",
                PrimaryKey = "PRIMARY_KEY",
                DbName = "TestDBName"
            };
            var options = Options.Create(dbSettings);

            _mockClient = new Mock<IDocumentClient>();

            _sut = new ProjectRepository(options, Mock.Of<ILogger<ProjectRepository>>(), _mockClient.Object);
        }

        internal virtual IQueryable<Project> GetProjectQuery()
        {
            return new List<Project>
            {
                new Project{ Id = "1", Name = "test1"},
                new Project{ Id = "2", Name = "test2"},
                new Project{ Id = "3", Name = "test3"},
                new Project{ Id = "4", Name = "test4"},
            }.AsQueryable();
        }

        [Fact]
        public async void Create_ReturnsProject()
        {
            // Make sure document has an Id to be able to read
            var expected = new Document()
            {
                Id = "12345"
            };

            // optional arguments also need to be passed in
            _mockClient.Setup(x => x.CreateDocumentAsync(
                It.IsAny<Uri>(),
                It.IsAny<Project>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ResourceResponse<Document>(expected));

            var result = await _sut.Create(new Project());

            Assert.IsType<Project>(result);
            Assert.Equal(expected.Id, result.Id);
        }

        [Fact]
        public void GetAll_ReturnsList()
        {
            var returnQuery = GetProjectQuery().OrderBy(x => x.Id);

            _mockClient
                .Setup(x => x.CreateDocumentQuery<Project>(It.IsAny<Uri>(), It.IsAny<FeedOptions>()))
                .Returns(returnQuery);

            List<Project> result = _sut.GetAll().ToList();

            Assert.True(result.Count() == 4);
            Assert.IsType<List<Project>>(result);
        }

        [Theory]
        [InlineData("1")]
        [InlineData("4")]
        public void GetBy_ReturnsProject_ById(string id)
        {
            var returnQuery = GetProjectQuery().OrderBy(x => x.Id);

            _mockClient
                .Setup(x => x.CreateDocumentQuery<Project>(It.IsAny<Uri>(), It.IsAny<FeedOptions>()))
                .Returns(returnQuery);

            Project result = _sut.GetBy(x => x.Id == id).ToList().FirstOrDefault();

            Assert.IsType<Project>(result);
        }

        [Theory]
        [InlineData("test1")]
        [InlineData("test2")]
        public void GetBy_ReturnsProject_ByName(string name)
        {
            var returnQuery = GetProjectQuery().OrderBy(x => x.Id);

            _output.WriteLine(returnQuery.ToString());

            _mockClient
                .Setup(x => x.CreateDocumentQuery<Project>(It.IsAny<Uri>(), It.IsAny<FeedOptions>()))
                .Returns(returnQuery);

            Project result = _sut.GetBy(x => x.Name == name).ToList().FirstOrDefault();

            Assert.IsType<Project>(result);
        }

        [Theory]
        [InlineData("no-name")]
        [InlineData(" ")]
        public void GetBy_ReturnsNull_WhenNoMatch(string name)
        {
            var returnQuery = GetProjectQuery().AsQueryable().OrderBy(x => x.Id);

            _mockClient
                .Setup(x => x.CreateDocumentQuery<Project>(It.IsAny<Uri>(), It.IsAny<FeedOptions>()))
                .Returns(returnQuery);

            Project result = _sut.GetBy(x => x.Name == name).ToList().FirstOrDefault();

            Assert.Null(result);
        }

        [Theory]
        [InlineData("test1")]
        [InlineData("test3")]
        public void GetByName_ReturnsProject(string name)
        {
            // This query (with fake data) is what will be returned for the method to act on
            var returnQuery = GetProjectQuery().OrderBy(x => x.Id);

            _mockClient
                .Setup(x => x.CreateDocumentQuery<Project>(It.IsAny<Uri>(), It.IsAny<FeedOptions>()))
                .Returns(returnQuery);

            var result = _sut.GetByName(name);

            Assert.IsType<Project>(result);
        }

        [Theory]
        [InlineData("test-none")]
        [InlineData(" ")]
        public void GetByName_ReturnsNull_WhenNoMatch(string name)
        {
            // This query (with fake data) is what will be returned for the method to act on
            var returnQuery = GetProjectQuery().OrderBy(x => x.Id);

            _mockClient
                .Setup(x => x.CreateDocumentQuery<Project>(It.IsAny<Uri>(), It.IsAny<FeedOptions>()))
                .Returns(returnQuery);

            var result = _sut.GetByName(name);

            Assert.Null(result);
        }

        [Fact]
        public void GetFirstOrDefault_ReturnsProject()
        {
            var returnQuery = GetProjectQuery().OrderBy(x => x.Id);

            _mockClient
                .Setup(x => x.CreateDocumentQuery<Project>(It.IsAny<Uri>(), It.IsAny<FeedOptions>()))
                .Returns(returnQuery);

            var result = _sut.GetFirstOrDefault(x => x.Name == "test1");

            Assert.IsType<Project>(result);
        }

        [Fact]
        public async void Update_CompletesTask()
        {
            // a resource response still needs to be returned > cannot be null
            _mockClient.Setup(x => x.ReplaceDocumentAsync(
                It.IsAny<Uri>(),
                It.IsAny<Project>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ResourceResponse<Document>());

            // make sure project has an id so that updatedDoc.Id is not null in Update
            var project = new Project { Id = "1", Name = "test1" };

            await _sut.Update(project);

            _mockClient.Verify(x => x.ReplaceDocumentAsync(
                It.IsAny<Uri>(),
                It.IsAny<Project>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<CancellationToken>()
                ), Times.Once);
        }

        [Fact]
        public async void Delete_CompletesTask()
        {
            var id = "some-id-123";

            _mockClient.Setup(x => x.DeleteDocumentAsync(
                It.IsAny<Uri>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ResourceResponse<Document>());

            await _sut.Delete(id);

            _mockClient.Verify(x => x.DeleteDocumentAsync(
                It.IsAny<Uri>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<CancellationToken>()
                ), Times.Once);
        }
    }
}
