using ProjectIssuesSuite.API.data.Models;

namespace ProjectIssuesSuite.API.data.Repositories
{
    public interface IProjectRepository : IDocumentRepository<Project>
    {
        Project GetByName(string name);

        Project GetById(string id);
    }
}
