using ProjectIssuesSuite.API.data.Models;

namespace ProjectIssuesSuite.API.data.Repositories
{
    public interface ITicketRepository : IDocumentRepository<Ticket>
    {
        Ticket GetById(string id);
    }
}