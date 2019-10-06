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
    public class TicketApiTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly CustomWebApplicationFactory<Startup> _factory;
        private HttpClient _httpClient;
        private Mock<IDocumentClient> _mockClient;

        private const string ticketId1 = "1";
        private const string testName1 = "test1";
        private const string createTicketId = "4";
        private const string createTicketName = "test4";
        private const string projectId1 = "10";
        private const string projName1 = "test proj 1";
        private const string projName2 = "test proj 2";

        public TicketApiTests(CustomWebApplicationFactory<Startup> factory)
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
            var returnTicketQuery = GetTicketQuery().OrderBy(x => x.Id);
            mockClient.Setup(x => x.CreateDocumentQuery<Ticket>(It.IsAny<Uri>(), It.IsAny<FeedOptions>()))
                .Returns(returnTicketQuery);

            // also arrange for Read of Project
            var returnProjectQuery = GetProjectQuery().OrderBy(x => x.Id);
            mockClient.Setup(x => x.CreateDocumentQuery<Project>(It.IsAny<Uri>(), It.IsAny<FeedOptions>()))
                .Returns(returnProjectQuery);

            return mockClient;
        }

        private IQueryable<Ticket> GetTicketQuery()
        {
            return new List<Ticket>
            {
                new Ticket{ Id = ticketId1, Name = testName1, ProjectName = projName1 },
                new Ticket{ Id = "2", Name = "test2", ProjectName = projName1 },
                new Ticket{ Id = "3", Name = "test3", ProjectName = projName2 },
                new Ticket{ Id = createTicketId, Name = createTicketName, ProjectName = projName1 },
            }.AsQueryable();
        }

        private IQueryable<Project> GetProjectQuery()
        {
            return new List<Project>
            {
                new Project{ Id = projectId1, Name = projName1 },
                new Project{ Id = "11", Name = projName2 }
            }.AsQueryable();
        }

        [Fact]
        public async Task GetAll_ReturnsSuccessAndJsonContentType()
        {
            var returnQuery = GetTicketQuery().OrderBy(x => x.Id);
            _mockClient.Setup(x => x.CreateDocumentQuery<Ticket>(It.IsAny<Uri>(), It.IsAny<FeedOptions>()))
                .Returns(returnQuery);

            var response = await _httpClient.GetAsync("/api/tickets");

            response.EnsureSuccessStatusCode();
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
        [InlineData("1")]
        [InlineData("4")]
        public async Task Get_ReturnsOkAndJsonContentType(string ticketId)
        {
            var response = await _httpClient.GetAsync("/api/tickets/" + ticketId);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/json; charset=utf-8",
                response.Content.Headers.ContentType.ToString());
        }

        [Theory]
        [InlineData("5")]
        [InlineData("100")]
        public async Task Get_NotFound_WhenTicketDoesNotExist(string ticketId)
        {
            var response = await _httpClient.GetAsync("/api/tickets/" + ticketId);
            var responseString = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal($"Ticket with id '{ticketId}' could not be found.", responseString);
        }

        [Fact]
        public async Task Create_ReturnsCreatedOnSuccess()
        {
            var expected = new Document()
            {
                Id = createTicketId
            };
            _mockClient.Setup(x => x.CreateDocumentAsync(
                It.IsAny<Uri>(),
                It.IsAny<Ticket>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ResourceResponse<Document>(expected));

            var response = await _httpClient.PostAsync("/api/tickets/",
                new StringContent(
                    JsonConvert.SerializeObject(
                        new TicketViewModel()
                        {
                            Name = createTicketName,
                            ProjectName = projName1,
                            VideoFiles = null
                        }),
                    Encoding.UTF8,
                    "application/json"));

            var responseViewModel = await response.Content.ReadAsAsync<TicketViewModel>();

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            // The 4th hardcoded ticket is the setup to be the newly created ticket
            Assert.Equal(createTicketName, responseViewModel.Name);
        }

        [Fact]
        public async Task Create_Error_ProjectDoesntExist()
        {
            var response = await _httpClient.PostAsync("/api/tickets/",
                new StringContent(
                    JsonConvert.SerializeObject(
                        new TicketViewModel()
                        {
                            Name = createTicketName,
                            ProjectName = "No Project with this name"
                        }),
                    Encoding.UTF8,
                    "application/json"));

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Fact]
        public async Task Update_ReturnsNoContent_OnSuccessWithNoNameChange()
        {
            // name and projectname are required
            var updatedTicket = new TicketViewModel()
            {
                Id = ticketId1,
                Name = testName1,
                Description = "new description",
                ProjectName = projName1
            };
            // make sure Ticket name in the route matches a Ticket in the hardcoded list
            var response = await _httpClient.PostAsync("/api/tickets/" + ticketId1,
                new StringContent(
                    JsonConvert.SerializeObject(updatedTicket),
                    Encoding.UTF8,
                    "application/json"));

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Update_ReturnsNoContent_OnSuccessWithNameChange()
        {
            // name and projectname are required
            var updatedTicket = new TicketViewModel()
            {
                Id = ticketId1,
                Name = "new name",
                ProjectName = projName1
            };
            // make sure Ticket name in the route matches a Ticket in the hardcoded list
            var response = await _httpClient.PostAsync("/api/tickets/" + ticketId1,
                new StringContent(
                    JsonConvert.SerializeObject(updatedTicket),
                    Encoding.UTF8,
                    "application/json"));

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Update_NotFound_WhenNoTicketWithId()
        {
            var updatedTicket = new TicketViewModel()
            {
                Id = "NONE",
                Name = "NEWNAME",
                ProjectName = projName1,
                VideoFiles = null
            };
            var response = await _httpClient.PostAsync("/api/tickets/NONE",
                new StringContent(
                    JsonConvert.SerializeObject(updatedTicket),
                    Encoding.UTF8,
                    "application/json"));

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Delete_ReturnsNoContentOnSuccess()
        {
            var response = await _httpClient.DeleteAsync("/api/tickets/" + ticketId1);

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}
