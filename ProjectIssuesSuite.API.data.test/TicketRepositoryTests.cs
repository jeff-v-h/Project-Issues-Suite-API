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

namespace ProjectIssuesSuite.API.data.test
{
    public class TicketRepositoryTests
    {
        private TicketRepository _sut;
        private readonly Mock<IDocumentClient> _mockClient;

        public TicketRepositoryTests()
        {
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

            _sut = new TicketRepository(options, Mock.Of<ILogger<TicketRepository>>(), _mockClient.Object);
        }

        internal virtual IQueryable<Ticket> GetTicketQuery()
        {
            return new List<Ticket>
            {
                new Ticket{ Id = "1", Name = "test1"},
                new Ticket{ Id = "2", Name = "test2"},
                new Ticket{ Id = "3", Name = "test3"},
                new Ticket{ Id = "4", Name = "test4"},
            }.AsQueryable();
        }

        [Fact]
        public async void Create_ReturnsIdString()
        {
            // Make sure document has an Id to be able to read
            var expected = new Document()
            {
                Id = "12345"
            };

            // optional arguments also need to be passed in
            _mockClient.Setup(x => x.CreateDocumentAsync(
                It.IsAny<Uri>(),
                It.IsAny<Ticket>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ResourceResponse<Document>(expected));

            var result = await _sut.Create(new Ticket());

            Assert.IsType<Ticket>(result);
            Assert.Equal(expected.Id, result.Id);
        }

        [Fact]
        public void GetAll_ReturnsList()
        {
            var returnQuery = GetTicketQuery().OrderBy(x => x.Id);

            _mockClient
                .Setup(x => x.CreateDocumentQuery<Ticket>(It.IsAny<Uri>(), It.IsAny<FeedOptions>()))
                .Returns(returnQuery);

            List<Ticket> result = _sut.GetAll().ToList();

            Assert.True(result.Count() == 4);
            Assert.IsType<List<Ticket>>(result);
        }

        [Theory]
        [InlineData("1")]
        [InlineData("4")]
        public void GetBy_ReturnsTicket_ById(string id)
        {
            var returnQuery = GetTicketQuery().OrderBy(x => x.Id);

            _mockClient
                .Setup(x => x.CreateDocumentQuery<Ticket>(It.IsAny<Uri>(), It.IsAny<FeedOptions>()))
                .Returns(returnQuery);

            Ticket result = _sut.GetBy(x => x.Id == id).ToList().FirstOrDefault();

            Assert.IsType<Ticket>(result);
        }

        [Theory]
        [InlineData("test1")]
        [InlineData("test2")]
        public void GetBy_ReturnsTicket_ByName(string name)
        {
            var returnQuery = GetTicketQuery().OrderBy(x => x.Id);

            _mockClient
                .Setup(x => x.CreateDocumentQuery<Ticket>(It.IsAny<Uri>(), It.IsAny<FeedOptions>()))
                .Returns(returnQuery);

            Ticket result = _sut.GetBy(x => x.Name == name).ToList().FirstOrDefault();

            Assert.IsType<Ticket>(result);
        }

        [Theory]
        [InlineData("no-name")]
        [InlineData(" ")]
        public void GetBy_ReturnsNull_WhenNoMatch(string name)
        {
            var returnQuery = GetTicketQuery().AsQueryable().OrderBy(x => x.Id);

            _mockClient
                .Setup(x => x.CreateDocumentQuery<Ticket>(It.IsAny<Uri>(), It.IsAny<FeedOptions>()))
                .Returns(returnQuery);

            Ticket result = _sut.GetBy(x => x.Name == name).ToList().FirstOrDefault();

            Assert.Null(result);
        }

        [Fact]
        public void GetById_ReturnsTicket()
        {
            // This query (with fake data) is what will be returned for the method to act on
            var returnQuery = GetTicketQuery().OrderBy(x => x.Id);

            _mockClient
                .Setup(x => x.CreateDocumentQuery<Ticket>(It.IsAny<Uri>(), It.IsAny<FeedOptions>()))
                .Returns(returnQuery);

            var result = _sut.GetById("4");

            Assert.IsType<Ticket>(result);
        }

        [Fact]
        public void GetById_ReturnsNull_WhenNoMatch()
        {
            // This query (with fake data) is what will be returned for the method to act on
            var returnQuery = GetTicketQuery().OrderBy(x => x.Id);

            _mockClient
                .Setup(x => x.CreateDocumentQuery<Ticket>(It.IsAny<Uri>(), It.IsAny<FeedOptions>()))
                .Returns(returnQuery);

            var result = _sut.GetById("99");

            Assert.Null(result);
        }

        [Fact]
        public void GetFirstOrDefault_ReturnsTicket()
        {
            var returnQuery = GetTicketQuery().OrderBy(x => x.Id);

            _mockClient
                .Setup(x => x.CreateDocumentQuery<Ticket>(It.IsAny<Uri>(), It.IsAny<FeedOptions>()))
                .Returns(returnQuery);

            var result = _sut.GetFirstOrDefault(x => x.Name == "test1");

            Assert.IsType<Ticket>(result);
        }

        [Fact]
        public async void Update_CompletesTask()
        {
            // a resource response still needs to be returned > cannot be null
            _mockClient.Setup(x => x.ReplaceDocumentAsync(
                It.IsAny<Uri>(),
                It.IsAny<Ticket>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ResourceResponse<Document>());

            // make sure Ticket has an id so that updatedDoc.Id is not null in Update
            var Ticket = new Ticket { Id = "1", Name = "test1" };

            await _sut.Update(Ticket);

            _mockClient.Verify(x => x.ReplaceDocumentAsync(
                It.IsAny<Uri>(),
                It.IsAny<Ticket>(),
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
