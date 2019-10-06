using ProjectIssuesSuite.API.data.Models;
using ProjectIssuesSuite.API.data.Repositories;
using ProjectIssuesSuite.API.domain.Managers;
using ProjectIssuesSuite.API.domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ProjectIssuesSuite.API.domain.test
{
    public class TicketManagerTests
    {
        private TicketManager _sut;
        private readonly Mock<IProjectRepository> _mockProjectRepo;
        private readonly Mock<ITicketRepository> _mockTicketRepo;
        private readonly Mock<IVideoManager> _mockVideoManager;

        public TicketManagerTests()
        {
            _mockProjectRepo = new Mock<IProjectRepository>();
            _mockTicketRepo = new Mock<ITicketRepository>();
            _mockVideoManager = new Mock<IVideoManager>();
            _sut = new TicketManager(Mock.Of<ILogger<TicketManager>>(), _mockProjectRepo.Object,
                _mockTicketRepo.Object, _mockVideoManager.Object);
        }

        // TODO account for addition of videos

        [Fact]
        public void GetAll_ReturnsViewModels_OnOk()
        {
            ICollection<Ticket> listTickets = new List<Ticket>();
            listTickets.Add(
                new Ticket
                {
                    Name = It.IsAny<string>()
                });
            _mockTicketRepo.Setup(x => x.GetAll()).Returns(listTickets.AsQueryable());

            ICollection<TicketViewModel> result = _sut.GetAllTickets();

            _mockTicketRepo.Verify(x => x.GetAll(), Times.Once);
            Assert.IsType<List<TicketViewModel>>(result);
        }

        [Fact]
        public void GetAll_ReturnsEmptyObject_CollectionHasNoDocs()
        {
            ICollection<Ticket> listTickets = new List<Ticket>();
            _mockTicketRepo.Setup(x => x.GetAll()).Returns(listTickets.AsQueryable());

            var result = _sut.GetAllTickets();

            _mockTicketRepo.Verify(x => x.GetAll(), Times.Once);
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void GetTicket_ReturnsNull_TicketDoesNotExist()
        {
            Ticket ticket = null;
            _mockTicketRepo.Setup(x => x.GetById(It.IsAny<string>())).Returns(ticket);

            var result = _sut.GetTicket(It.IsAny<string>());

            _mockTicketRepo.Verify(x => x.GetById(It.IsAny<string>()), Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public void GetTicket_ReturnsViewModel_OnOk()
        {
            _mockTicketRepo.Setup(x => x.GetById(It.IsAny<string>())).Returns(new Ticket());

            TicketViewModel result = _sut.GetTicket(It.IsAny<string>());

            Assert.NotNull(result);
            Assert.IsType<TicketViewModel>(result);
        }

        [Fact]
        public async void CreateTicket_ReturnsNull_ProjectNameDoesNotExist()
        {
            Project project = null;
            _mockProjectRepo.Setup(x => x.GetByName(It.IsAny<string>())).Returns(project);

            var result = await _sut.CreateTicket(new TicketViewModel());

            _mockProjectRepo.Verify(x => x.GetByName(It.IsAny<string>()), Times.Once);
            _mockTicketRepo.Verify(x => x.Create(It.IsAny<Ticket>()), Times.Never);
            _mockProjectRepo.Verify(x => x.Update(It.IsAny<Project>()), Times.Never);
            Assert.Null(result);
        }

        [Fact]
        public async void CreateTicket_ReturnsViewModel()
        {
            var ticket = new TicketViewModel
            {
                Name = "test-name",
                Description = "test-desc",
                ProjectName = "test-project"
            };
            _mockProjectRepo.Setup(x => x.GetByName(It.IsAny<string>())).Returns(new Project());

            var result = await _sut.CreateTicket(ticket);

            _mockProjectRepo.Verify(x => x.GetByName(It.IsAny<string>()), Times.Once);
            _mockTicketRepo.Verify(x => x.Create(It.IsAny<Ticket>()), Times.Once);
            _mockProjectRepo.Verify(x => x.Update(It.IsAny<Project>()), Times.Once);
            Assert.IsType<TicketViewModel>(result);
            Assert.Equal(ticket.Name, result.Name);
            Assert.Equal(ticket.Description, result.Description);
        }

        [Fact]
        public async Task ReplaceTicket_ReturnsFalse_TicketDoesNotExist()
        {
            Ticket ticket = null;
            _mockTicketRepo.Setup(x => x.GetById("hello")).Returns(ticket);
            var ticketVM = new TicketViewModel
            {
                Id = "someId"
            };

            var result = await _sut.ReplaceTicket(ticketVM);

            _mockTicketRepo.Verify(x => x.GetById(It.IsAny<string>()), Times.Once);
            _mockProjectRepo.Verify(x => x.GetByName(It.IsAny<string>()), Times.Never);
            _mockProjectRepo.Verify(x => x.Update(It.IsAny<Project>()), Times.Never);
            _mockTicketRepo.Verify(x => x.Update(It.IsAny<Ticket>()), Times.Never);
            Assert.False(result);
        }

        [Fact]
        public async Task ReplaceTicket_ReturnsTrue_WhenNameNotChanged()
        {
            var ticket = new Ticket
            {
                Name = "test-name",
                Description = "test-desc"
            };
            _mockTicketRepo.Setup(x => x.GetById(It.IsAny<string>())).Returns(ticket);

            var ticket2 = new TicketViewModel
            {
                Name = "test-name",
                Description = "another desc"
            };
            var result = await _sut.ReplaceTicket(ticket2);

            _mockTicketRepo.Verify(x => x.GetById(It.IsAny<string>()), Times.Once);
            _mockProjectRepo.Verify(x => x.GetByName(It.IsAny<string>()), Times.Never);
            _mockProjectRepo.Verify(x => x.Update(It.IsAny<Project>()), Times.Never);
            _mockTicketRepo.Verify(x => x.Update(It.IsAny<Ticket>()), Times.Once);
            Assert.True(result);
        }

        [Fact]
        public async Task ReplaceTicket_ReturnsFalse_WhenNameChangedButProjectDoesNotExist()
        {
            var ticket = new Ticket
            {
                Name = "test-name",
                Description = "test-desc"
            };
            _mockTicketRepo.Setup(x => x.GetById(It.IsAny<string>())).Returns(ticket);
            Project project = null;
            _mockProjectRepo.Setup(x => x.GetByName(It.IsAny<string>())).Returns(project);

            var ticket2 = new TicketViewModel
            {
                Name = "another name",
                Description = "another desc"
            };
            var result = await _sut.ReplaceTicket(ticket2);

            _mockTicketRepo.Verify(x => x.GetById(It.IsAny<string>()), Times.Once);
            _mockProjectRepo.Verify(x => x.GetByName(It.IsAny<string>()), Times.Once);
            _mockProjectRepo.Verify(x => x.Update(It.IsAny<Project>()), Times.Never);
            _mockTicketRepo.Verify(x => x.Update(It.IsAny<Ticket>()), Times.Never);
            Assert.False(result);
        }

        [Fact]
        public void ReplaceTicket_ReturnsTrue_WhenNameChanged()
        {
            var ticket = new Ticket
            {
                Name = "test-name",
                Description = "test-desc"
            };
            _mockTicketRepo.Setup(x => x.GetById(It.IsAny<string>())).Returns(ticket);
            _mockProjectRepo.Setup(x => x.GetByName(It.IsAny<string>())).Returns(new Project());

            var ticket2 = new TicketViewModel
            {
                Name = "another name",
                Description = "another desc"
            };
            var result = _sut.ReplaceTicket(ticket2);

            _mockTicketRepo.Verify(x => x.GetById(It.IsAny<string>()), Times.Once);
            _mockProjectRepo.Verify(x => x.GetByName(It.IsAny<string>()), Times.Once);
            _mockProjectRepo.Verify(x => x.Update(It.IsAny<Project>()), Times.Once);
            _mockTicketRepo.Verify(x => x.Update(It.IsAny<Ticket>()), Times.Once);
        }

        [Fact]
        public void DeleteTicket_ReturnsFalse_TicketDoesNotExist()
        {
            Ticket ticket = null;
            _mockTicketRepo.Setup(x => x.GetById(It.IsAny<string>())).Returns(ticket);

            _sut.DeleteTicket(It.IsAny<string>()).Wait();

            _mockTicketRepo.Verify(x => x.GetById(It.IsAny<string>()), Times.Once);
            _mockTicketRepo.Verify(x => x.Delete(It.IsAny<string>()), Times.Never);
            _mockProjectRepo.Verify(x => x.GetByName(It.IsAny<string>()), Times.Never);
            _mockProjectRepo.Verify(x => x.Update(It.IsAny<Project>()), Times.Never);
        }

        [Fact]
        public void DeleteTicket_ReturnsTrue()
        {
            _mockTicketRepo.Setup(x => x.GetById(It.IsAny<string>())).Returns(new Ticket());
            _mockProjectRepo.Setup(x => x.GetByName(It.IsAny<string>())).Returns(It.IsAny<Project>());

            _sut.DeleteTicket(It.IsAny<string>()).Wait();

            _mockTicketRepo.Verify(x => x.GetById(It.IsAny<string>()), Times.Once);
            _mockTicketRepo.Verify(x => x.Delete(It.IsAny<string>()), Times.Once);
            _mockProjectRepo.Verify(x => x.GetByName(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void DeleteTicket_RemovesTicketFromProject()
        {
            _mockTicketRepo.Setup(x => x.GetById(It.IsAny<string>())).Returns(new Ticket());
            var ticketId = "test-id";
            var project = new Project
            {
                Tickets = new List<TicketBase>
                {
                    new TicketBase
                    {
                        Id = ticketId
                    }
                }
            };
            _mockProjectRepo.Setup(x => x.GetByName(It.IsAny<string>())).Returns(project);

            _sut.DeleteTicket(It.IsAny<string>()).Wait();

            _mockTicketRepo.Verify(x => x.GetById(It.IsAny<string>()), Times.Once);
            _mockTicketRepo.Verify(x => x.Delete(It.IsAny<string>()), Times.Once);
            _mockProjectRepo.Verify(x => x.GetByName(It.IsAny<string>()), Times.Once);
            _mockProjectRepo.Verify(x => x.Update(It.IsAny<Project>()), Times.Once);
        }
    }
}
