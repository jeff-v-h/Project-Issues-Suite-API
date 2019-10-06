using ProjectIssuesSuite.API.common.Models;
using ProjectIssuesSuite.API.data.Models;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ProjectIssuesSuite.API.data.Repositories
{
    public class ProjectRepository : CosmosRepository<Project>, IProjectRepository
    {
        public ProjectRepository(IOptions<DbSettings> dbSettings, ILogger<ProjectRepository> logger, IDocumentClient client) : base(dbSettings, logger, client)
        {
        }

        public Project GetByName(string name)
        {
            return GetFirstOrDefault(x => x.Name.ToUpper() == name.ToUpper());
        }

        public Project GetById(string id)
        {
            return GetFirstOrDefault(x => x.Id.ToUpper() == id.ToUpper());
        }
    }
}
