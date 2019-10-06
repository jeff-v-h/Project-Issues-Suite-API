using ProjectIssuesSuite.API.data.Models;
using ProjectIssuesSuite.API.domain.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using ProjectIssuesSuite.API.presentation;

namespace ProjectIssuesSuite.API.IntegrationTests
{
    public class ProjectApiTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly CustomWebApplicationFactory<Startup> _factory;
        private HttpClient _httpClient;
        private Mock<IDocumentClient> _mockClient;

        private const string id1 = "1";
        private const string name1 = "test1";
        private const string name2 = "test2";
        private const string newName = "new name";

        public ProjectApiTests(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
            _mockClient = SetupMockClient();
            _httpClient = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddTransient(s => _mockClient.Object);
                });
            }).CreateDefaultClient();
        }

        private Mock<IDocumentClient> SetupMockClient()
        {
            var mockClient = new Mock<IDocumentClient>();

            // arrange for Get methods > most CRUD methods access a Get function
            var returnQuery = GetProjectQuery().OrderBy(x => x.Id);
            mockClient.Setup(x => x.CreateDocumentQuery<Project>(It.IsAny<Uri>(), It.IsAny<FeedOptions>()))
                .Returns(returnQuery);

            return mockClient;
        }

        private IQueryable<Project> GetProjectQuery()
        {
            return new List<Project>
            {
                new Project{ Id = id1, Name = name1 },
                new Project{ Id = "2", Name = name2 }
            }.AsQueryable();
        }

        [Fact]
        public async Task GetAll_ReturnsSuccessAndJsonContentType()
        {
            var returnQuery = GetProjectQuery().OrderBy(x => x.Id);
            _mockClient.Setup(x => x.CreateDocumentQuery<Project>(It.IsAny<Uri>(), It.IsAny<FeedOptions>()))
                .Returns(returnQuery);

            var response = await _httpClient.GetAsync("/api/projects");

            response.EnsureSuccessStatusCode(); // Status code 200-299
            Assert.Equal("application/json; charset=utf-8",
                response.Content.Headers.ContentType.ToString());
        }

        [Theory]
        [InlineData("/api/no-collection")]
        [InlineData("/api/")]
        public async Task GetAll_NotFound_WhenCollectionDoesNotExist(string url)
        {
            var response = await _httpClient.GetAsync(url);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData(name1)]
        [InlineData("TEST2")]
        public async Task Get_ReturnsOkAndJsonContentType(string collectionName)
        {
            var response = await _httpClient.GetAsync("/api/projects/" + collectionName);

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode); // more specific
            Assert.Equal("application/json; charset=utf-8",
                response.Content.Headers.ContentType.ToString());
        }

        [Theory]
        [InlineData("NONE")]
        public async Task Get_NotFound_WhenProjectDoesNotExist(string projectName)
        {
            var response = await _httpClient.GetAsync("/api/projects/" + projectName);
            var responseString = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal($"Project with name '{projectName}' was not found.", responseString);
        }

        [Fact]
        public async Task Create_ReturnsCreatedOnSuccess()
        {
            var expected = new Document()
            {
                Id = id1
            };
            _mockClient.Setup(x => x.CreateDocumentAsync(
                It.IsAny<Uri>(),
                It.IsAny<Project>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ResourceResponse<Document>(expected));

            var response = await _httpClient.PostAsync("/api/projects/",
                new StringContent(
                    JsonConvert.SerializeObject(
                        new ProjectViewModel() { Name = newName }),
                    Encoding.UTF8,
                    "application/json"));

            var responseViewModel = await response.Content.ReadAsAsync<ProjectViewModel>();

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            // mock is setup to return a document of id '1'.
            // The hardcoded project with id '1' has name 'test1'
            Assert.Equal(newName, responseViewModel.Name);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("test1")]
        public async Task Create_BadRequest_InvalidInput(string newName)
        {
            var expected = new Document()
            {
                Id = id1
            };
            _mockClient.Setup(x => x.CreateDocumentAsync(
                It.IsAny<Uri>(),
                It.IsAny<Project>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ResourceResponse<Document>(expected));

            var response = await _httpClient.PostAsync("/api/projects/",
                new StringContent(
                    JsonConvert.SerializeObject(
                        new ProjectViewModel() { Name = newName }),
                    Encoding.UTF8,
                    "application/json"));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Update_ReturnsNoContentOnSuccess()
        {
            // name is required
            var updatedProject = new ProjectViewModel()
            {
                Name = "NEWNAME"
            };
            // make sure project name in the route matches a project in the hardcoded list
            var response = await _httpClient.PostAsync("/api/projects/" + name1,
                new StringContent(
                    JsonConvert.SerializeObject(updatedProject),
                    Encoding.UTF8,
                    "application/json"));

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Update_NotFound_WhenNoProjectWithName()
        {
            var updatedProject = new ProjectViewModel()
            {
                Name = "NEWNAME"
            };
            var response = await _httpClient.PostAsync("/api/projects/NONE",
                new StringContent(
                    JsonConvert.SerializeObject(updatedProject),
                    Encoding.UTF8,
                    "application/json"));

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Delete_ReturnsNoContentOnSuccess()
        {
            var response = await _httpClient.DeleteAsync("/api/projects/" + name2);

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Delete_BadRequest_WhenNoProjectWithName()
        {
            var response = await _httpClient.DeleteAsync("/api/projects/NONE");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
