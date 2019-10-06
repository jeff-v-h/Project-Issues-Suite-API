using ProjectIssuesSuite.API.common.Models;
using ProjectIssuesSuite.API.data.Models;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ProjectIssuesSuite.API.data.Repositories
{
    public class TicketRepository : CosmosRepository<Ticket>, ITicketRepository
    {
        public TicketRepository(IOptions<DbSettings> dbSettings, ILogger<TicketRepository> logger, IDocumentClient client) : base(dbSettings, logger, client)
        {
        }

        public Ticket GetById(string id)
        {
            return GetFirstOrDefault(x => x.Id.ToUpper() == id.ToUpper());
        }
    }
}
