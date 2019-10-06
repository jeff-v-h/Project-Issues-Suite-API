using LazyCache.Mocks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ProjectIssuesSuite.API.data.Models;
using ProjectIssuesSuite.API.domain.Managers;
using ProjectIssuesSuite.API.domain.Models;
using ProjectIssuesSuite.API.presentation.Controllers;
using System.Collections.Generic;
using Xunit;

namespace ProjectIssuesSuite.API.presentation.test
{
    public class TicketsControllerTests
    {
        private readonly Mock<ITicketManager> _mockManager;
        private TicketsController _sut;

        public TicketsControllerTests()
        {
            // Arrange mock of manager here
            _mockManager = new Mock<ITicketManager>();
            // sut = system under test
            _sut = new TicketsController(Mock.Of<ILogger<TicketsController>>(), _mockManager.Object, new MockCachingService());
        }

        [Fact]
        public void GetAllTickets_ReturnsOk()
        {
            ICollection<TicketViewModel> listTickets = new List<TicketViewModel>();
            _mockManager.Setup(x => x.GetAllTickets()).Returns(listTickets);

            IActionResult result = _sut.GetTickets();

            ObjectResult objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<List<TicketViewModel>>(objectResult.Value);
        }

        [Fact]
        public void GetTicket_NotFound_WhenNull()
        {
            TicketViewModel ticket = null;
            _mockManager.Setup(x => x.GetTicket(It.IsAny<string>())).Returns(ticket);

            var result = _sut.GetTicket(It.IsAny<string>());

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public void GetTicket_ReturnsOk()
        {
            var ticketVM = new TicketViewModel();
            _mockManager.Setup(x => x.GetTicket(It.IsAny<string>()))
                .Returns(ticketVM);

            var result = _sut.GetTicket(It.IsAny<string>());

            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<TicketViewModel>(objectResult.Value);
        }

        [Fact]
        public async void CreateTicket_BadRequest_NullPost()
        {
            TicketViewModel newTicket = null;

            var result = await _sut.CreateTicket(newTicket);

            var badReqResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal($"Please provide details to create a ticket.", badReqResult.Value);
            _mockManager.Verify(x => x.CreateTicket(
                It.IsAny<TicketViewModel>()), Times.Never);
        }

        [Fact]
        public async void CreateTicket_BadRequest_InvalidModelState()
        {
            _sut.ModelState.AddModelError("x", "Test Error");

            var ticket = new TicketViewModel();

            var result = await _sut.CreateTicket(ticket);

            var badReqResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badReqResult.Value);
            _mockManager.Verify(x => x.CreateTicket(
                It.IsAny<TicketViewModel>()), Times.Never);
        }

        [Fact]
        public async void CreateTicket_InternalError_ManagerReturnsNull()
        {
            TicketViewModel TicketVM = null;
            _mockManager.Setup(x => x.CreateTicket(
                It.IsAny<TicketViewModel>())).ReturnsAsync(TicketVM);

            IActionResult result = await _sut.CreateTicket(
                new TicketViewModel { Name = "PGA" });

            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.IsType<string>(errorResult.Value);
        }

        [Theory]
        [InlineData("ticket error name")]
        [InlineData("Another name", "test desc", "test-name", "id123", "vid-title", "www.vid-url.com")]
        public async void CreateTicket_ReturnsCreatedRoute(string name, string description = null,
            string projectName = null, string vidId = null, string vidTitle = null, string vidUrl = null)
        {
            // Arrange what the manager will return
            // Name is required in below Ticket models
            TicketViewModel ticketVM = new TicketViewModel()
            {
                Id = "test-id",
                Name = name,
                Description = description,
                ProjectName = projectName,
                Videos = new List<Video>()
                {
                    new Video()
                    {
                        Id = vidId,
                        Title = vidTitle,
                        FileLocation = vidUrl
                    }
                }
            };
            _mockManager.Setup(x => x.CreateTicket(
                It.IsAny<TicketViewModel>())).ReturnsAsync(ticketVM);

            // Make a new dto object to pass in. Name is a required property
            var TicketDto = new TicketViewModel() { Name = ticketVM.Name };
            IActionResult result = await _sut.CreateTicket(TicketDto);

            // Ensure the manager method executes once
            _mockManager.Verify(x => x.CreateTicket(
                It.IsAny<TicketViewModel>()), Times.Once);

            var createdResult = Assert.IsType<CreatedAtRouteResult>(result);
            Assert.Same(ticketVM, createdResult.Value);
            Assert.Equal("GetTicket", createdResult.RouteName);
            // The RouteValues refers to the key/value pairs passed into the CreatedAtRoute method
            // (not the properties of the objects above)
            Assert.Equal(ticketVM.Id, createdResult.RouteValues["ticketId"]);
        }

        [Fact]
        public void UpdateTicket_BadRequest_NullPost()
        {
            TicketViewModel Ticket = null;

            var result = _sut.UpdateTicket("ATP", Ticket);

            var badReqResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal($"Please provide details to update ticket.", badReqResult.Value);
            // Ensure ReplaceTicketViaManager is never run since it should no be reached
            _mockManager.Verify(x => x.ReplaceTicket(It.IsAny<TicketViewModel>()), Times.Never);
        }

        [Fact]
        public void UpdateTicket_BadRequest_InvalidModelState()
        {
            _sut.ModelState.AddModelError("x", "Test Error");

            var ticket = new TicketViewModel { Name = "Updated - Logout Bug" };
            var result = _sut.UpdateTicket("5c24dd93-6046-408c-a3a5-2a2dd4752695", ticket);

            var badReqResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badReqResult.Value);
            _mockManager.Verify(x => x.ReplaceTicket(It.IsAny<TicketViewModel>()), Times.Never);
        }

        // Have inline data here since NotFound returns  and Id which we want to test
        [Theory]
        [InlineData(" ")]
        [InlineData("No-Ticket")]
        public void UpdateTicket_NotFound_NotUpdated(string ticketId)
        {
            _mockManager.Setup(x => x.ReplaceTicket(It.IsAny<TicketViewModel>())).ReturnsAsync(false);

            TicketViewModel ticket = new TicketViewModel
            {
                Id = ticketId,
                Name = "Updated - Logout Bug"
            };
            var result = _sut.UpdateTicket(ticketId, ticket);

            _mockManager.Verify(x => x.ReplaceTicket(It.IsAny<TicketViewModel>()), Times.Once);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public void UpdateTicket_ReturnsNoContent()
        {
            _mockManager.Setup(x => x.ReplaceTicket(It.IsAny<TicketViewModel>())).ReturnsAsync(true);

            var result = _sut.UpdateTicket(It.IsAny<string>(), new TicketViewModel() { Name = "testname" });

            _mockManager.Verify(x => x.ReplaceTicket(It.IsAny<TicketViewModel>()), Times.Once);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public void DeleteTicket_RunsOnce()
        {
            _sut.DeleteTicket(It.IsAny<string>());

            _mockManager.Verify(x => x.DeleteTicket(It.IsAny<string>()), Times.Once);
        }
    }
}
