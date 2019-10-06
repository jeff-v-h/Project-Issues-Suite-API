using ProjectIssuesSuite.API.domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectIssuesSuite.API.domain.Managers
{
    public interface ITicketManager
    {
        ICollection<TicketViewModel> GetAllTickets();
        TicketViewModel GetTicket(string ticketId);
        TicketViewModel GetTicketByName(string projectName, string ticketName);
        Task<TicketViewModel> CreateTicket(TicketViewModel newVideo);
        Task<bool> ReplaceTicket(TicketViewModel newVideoObject);
        Task DeleteTicket(string ticketId);
    }
}
